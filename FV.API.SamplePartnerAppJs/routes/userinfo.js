const express = require('express');
const router = express.Router();
const ensureLoggedIn = require('connect-ensure-login').ensureLoggedIn;
const { getCurrentUser, getUserOrgsForToken } = require('./../gateway-api/gatewayApi')

router.get('/', async (req, res) => {
  try {

    // get list of all orgs for user
    const userOrgs = await getUserOrgsForToken(req.user.tokenSet);    
    
    // get current native user id and first org id.
    // There might be multiple orgs, in which case we want to ask the user which org they want to use.
    // save our orgId and native user id in session so we can use them to make further API requests
    req.user.tokenSet.expires_in_seconds = req.user.tokenSet.expires_at - Math.round(Date.now()/1000);

    // get user info
    const orgUser = await getCurrentUser(req.user.tokenSet, userOrgs.user.userId.native, userOrgs.orgs[0].orgId);

    // display all of the above data
    res.render('userinfo', { user: req.user, orgs: userOrgs.orgs, orgUser: orgUser });
  } catch (error) {
    console.error(error);
    res.status(500).send('Internal Server Error:' + error);
  }
});


module.exports = router;