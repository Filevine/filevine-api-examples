using FV.API.SamplePartnerApp.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((ctx, config) =>
{
    if (ctx.HostingEnvironment.IsDevelopment()) config.AddJsonFile("appsettings.Development.json", optional: true);
});

var authority = builder.Configuration["FvApiGateway:Authority"];
var clientId = builder.Configuration["FvApiGateway:ClientId"];
var clientSecret = builder.Configuration["FvApiGateway:ClientSecret"];
var refreshThreshold = builder.Configuration.GetValue("TokenRefreshPrefetchTimespan", TimeSpan.FromMinutes(1));
var tokenEndpoint = new Lazy<Task<string>>(async () =>
{
    var discoveryDocument = await new HttpClient().GetDiscoveryDocumentAsync(authority);
    return discoveryDocument.TokenEndpoint!;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<FvApiGatewayService>();
builder.Services.AddHttpClient();

builder.Services.AddFusionCache()
    .WithOptions(opt =>
    {
        opt.CacheKeyPrefix = "FV.API.Examples.SamplePartnerApp/Users/";
        opt.DefaultEntryOptions = new FusionCacheEntryOptions
        {
            SkipDistributedCache = true
        };
    })
    .WithRegisteredLogger();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Events = new CookieAuthenticationEvents
        {
            // After the auth cookie has been validated, this event is called.
            // In it we see if the access token is close to expiring.  If it is
            // then we use the refresh token to get a new access token and save them.
            // If the refresh token does not work for some reason then we redirect to 
            // the login screen.
            OnValidatePrincipal = async cookieCtx =>
            {
                var expiresAt = DateTimeOffset.Parse(cookieCtx.Properties.GetTokenValue("expires_at")!);
                var timeRemaining = expiresAt.Subtract(DateTimeOffset.UtcNow);

                if (timeRemaining < refreshThreshold)
                {
                    var tokenRenewed = false;
                    var refreshToken = cookieCtx.Properties.GetTokenValue("refresh_token");
                    if (refreshToken != null)
                    {
                        var client = cookieCtx.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>()
                            .CreateClient(clientId);
                        var refreshTokenRequest = new RefreshTokenRequest
                        {
                            Address = await tokenEndpoint.Value,
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                            RefreshToken = refreshToken
                        };
                        var response = await client.RequestRefreshTokenAsync(refreshTokenRequest);

                        if (!response.IsError && response.AccessToken != null)
                        {
                            var expiresInSeconds = response.ExpiresIn;
                            var updatedExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
                            cookieCtx.Properties.UpdateTokenValue("expires_at", updatedExpiresAt.ToString());
                            cookieCtx.Properties.UpdateTokenValue("access_token", response.AccessToken);
                            cookieCtx.Properties.UpdateTokenValue("refresh_token", response.RefreshToken!);
                    
                            // Indicate to the cookie middleware that the cookie should be remade (since we have updated it)
                            cookieCtx.ShouldRenew = true;
                            tokenRenewed = true;
                        }
                    }

                    if (!tokenRenewed)
                    {
                        cookieCtx.RejectPrincipal();
                        await cookieCtx.HttpContext.SignOutAsync();
                    }
                }
            }
        };
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authority;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.UsePkce = true;

        options.Scope.Clear();
        var scopes = builder.Configuration.GetSection("FvApiGateway:Scopes").Get<List<string>>();
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = false;
        options.UseTokenLifetime = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role",
        };

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}").
    RequireAuthorization();

app.Run();