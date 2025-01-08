const createError = require("http-errors");
const express = require("express");
const path = require("path");
const cookieParser = require("cookie-parser");
const logger = require("morgan");
const fs = require("fs");
const https = require("https");
const nconf = require("nconf");
const expressSesssion = require("express-session");
const passport = require("passport");
const { Issuer, Strategy, custom } = require("openid-client");
const ensureLoggedIn = require('connect-ensure-login').ensureLoggedIn;

const indexRouter = require("./routes/index");
const userinfoRouter = require("./routes/userinfo");

// load config file
nconf.argv().env().file({ file: "config.json" });

const app = express();

// view engine setup
app.set("views", path.join(__dirname, "views"));
app.set("view engine", "ejs");

app.use(logger("dev"));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));

app.use(cookieParser());
app.use(express.static(path.join(__dirname, "public")));

app.use("/", indexRouter);

function getDefaultIdTokenAlg(issuer) {
  var alg = 'RS512'; // Default for Filevine.
  if (issuer.metadata.id_token_signing_alg_values_supported && issuer.metadata.id_token_signing_alg_values_supported.length > 0) {
    alg = issuer.metadata.id_token_signing_alg_values_supported[0];
  }
  else {
    console.warn("Expected default ID Alg was not found in issuer metadata");
  }
  return alg;
}

// for local identity server only:
//process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

const gatewayConfig = nconf.get("fvApiGateway");
const scopes = gatewayConfig.Scopes.join(" ");
// REF: https://medium.com/@nitesh_17214/how-to-create-oidc-client-in-nodejs-b8ea779e0c64
Issuer.discover(gatewayConfig.Authority).then((issuer) => {
  const client = new issuer.Client({
    client_id: gatewayConfig.ClientId,
    client_secret: gatewayConfig.ClientSecret,
    // If your client code is considered public (mobile apps or SPA applications) then the client_secret isn't valuable. In such cases
    // it can be elimated and instaed setting 'token_endpoint_auth_method' to 'none'.
    // token_endpoint_auth_method: 'none',   
    redirect_uris: [gatewayConfig.RedirectUri],
    post_logout_redirect_uris: [gatewayConfig.PostLogoutRedirectUri],
    response_types: ["code"],
    id_token_signed_response_alg: getDefaultIdTokenAlg(issuer),
  });
  client[custom.clock_tolerance] = gatewayConfig.ClockSkewSeconds;
  
  app.use(
    expressSesssion({
      secret: nconf.get("expressSessionSecret"),
      resave: false,
      saveUninitialized: true,
    })
  );
  
  app.use(passport.initialize());
  app.use(passport.session());
  
  passport.use(
    "oidc",
    new Strategy(
      { client, passReqToCallback: true, usePKCE: true },
      (req, tokenSet, userinfo, done) => {
        req.session.tokenSet = tokenSet;
        req.session.userinfo = userinfo;
        
        // save user and its token to express-session
        const user = tokenSet.claims();
        user.tokenSet = tokenSet;
        
        return done(null, user);
      }
    )
  );
  
  // handles serialization and deserialization of authenticated user
  passport.serializeUser(function (user, done) {
    done(null, user);
  });
  passport.deserializeUser(function (user, done) {
    done(null, user);
  });
  
  app.get(
    "/auth",
    (req, res, next) => {
      next();
    },
    
    passport.authenticate("oidc", { scope: [scopes] })
  );
  
  // authentication callback
  app.get("/signin-oidc", (req, res, next) => {
    passport.authenticate("oidc", (err, user, options) => {
      if (user) {
        // If the user exists log him in:
        req.login(user, (error) => {
          if (error) {
            res.send(error);
          } else {
            // HANDLE SUCCESSFUL LOGIN
            res.redirect("/userinfo");
          }
        });
      } else {
        // HANDLE FAILURE LOGGING IN
        res.redirect("/failure");
        // or
        // res.render("/login", { message: options.message || "custom message" })
      }
    })(req, res);
  });

  refreshTokenIfExpired = async (req, res, next) => {
    const tokenSet = req.user.tokenSet;
    const expirationTime = tokenSet.expires_at;
    const currentTime = Math.floor(Date.now() / 1000);
    
    if (currentTime < expirationTime) {
      // Token is not expired
    }
    else {
      const newTokenSet = await client.refresh(tokenSet.refresh_token)
      req.user.tokenSet = newTokenSet;
    }
    next();
  };
  
  app.use("/userinfo", 
    ensureLoggedIn('/'), 
    refreshTokenIfExpired, 
    userinfoRouter
  );
  
  // clear user's session
  app.get("/logout", (req, res) => {
    req.logout(function(err) {
      if (err) { return next(err); }

      // We will redirect back to the identity server to logout there as well. 
      // If this happens then the /logout/callback will be redirected to by the identity server after the session is killed.
      res.redirect(client.endSessionUrl()); 
    });
  });  

  // Post logout callback should be called after we redirect to the identity server to end the session there.
  app.get("/logout/callback", (req, res) => {
    res.redirect('/');
  })
  
  // catch 404 and forward to error handler
  app.use(function (req, res, next) {
    next(createError(404));
  });
  
  // error handler
  app.use(function (err, req, res, next) {
    // set locals, only providing error in development
    res.locals.message = err.message;
    res.locals.error = req.app.get("env") === "development" ? err : {};
    
    // render the error page
    res.status(err.status || 500);
    res.render("error");
  });
  
  const httpsServer = https.createServer(
    {
      key: fs.readFileSync("./cert/cert.key"),
      cert: fs.readFileSync("./cert/cert.pem"),
    },
    app
  );
  
  const port = nconf.get("port") || 3000;
  httpsServer.listen(port, () => {
    console.log("HTTPS Server running on port " + port);
  });
});

module.exports = app;
