const nconf = require("nconf");

// todo: remove this, for local testing with self signed certificate only:
const https = require("https");
const agent = new https.Agent({
  rejectUnauthorized: false,
});

const ApiPath = "fv-app";
const GetMe = ApiPath + "/v2/Users/Me";
const GetUserOrgsForToken = ApiPath + "/v2/utils/GetUserOrgsWithToken";

async function getUserOrgsForToken(tokenSet) {
  
  const fetch = await import("node-fetch");
  const gatewayConfig = nconf.get("fvApiGateway");
  const gatewayUrl = gatewayConfig.Url;
  const headers = {
    Authorization: "Bearer " + tokenSet.access_token,
    "Content-Type": "application/json",
  };
  const response = await fetch.default(gatewayUrl + "/" + GetUserOrgsForToken, {
    agent,
    method: "post",
    headers,
  });

  if (!response.ok) {
    throw new Error(
      "Response is not successful: " +
        response.status +
        " " +
        response.statusText
    );
  }

  const parsedResponse = await response.json();
  return parsedResponse;
}

async function getCurrentUser(tokenSet, userNativeId, orgId) {

  const fetch = await import("node-fetch");
  const gatewayConfig = nconf.get("fvApiGateway");
  const gatewayUrl = gatewayConfig.Url;

  const headers = {
    Authorization: "Bearer " + tokenSet.access_token,
    "Content-Type": "application/json",
    "x-fv-orgid": orgId,
    "x-fv-userid": userNativeId,
  };

  const response = await fetch.default(gatewayUrl + "/" + GetMe, {
    agent,
    method: "get",
    headers,
  });

  if (!response.ok) {
    throw new Error(
      "Response is not successful: " +
        response.status +
        " " +
        response.statusText
    );
  }

  const parsedResponse = await response.json();
  return parsedResponse;
}

module.exports = {
  getCurrentUser,
  getUserOrgsForToken,
};
