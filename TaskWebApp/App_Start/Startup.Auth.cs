using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// The following using statements were added for this sample
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.Web.Mvc;
using System.Configuration;
using TaskWebApp.Policies;
using TaskWebApp.Utils;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;

namespace TaskWebApp
{
	public partial class Startup
	{
        private const string discoverySuffix = "/.well-known/openid-configuration";
        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";

        // App config settings
        public static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        public static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        public static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        // B2C policy identifiers
        public static string SignUpPolicyId = ConfigurationManager.AppSettings["ida:SignUpPolicyId"];
        public static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        public static string ProfilePolicyId = ConfigurationManager.AppSettings["ida:UserProfilePolicyId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            OpenIdConnectAuthenticationOptions options = new OpenIdConnectAuthenticationOptions
            {
                // Standard OWIN OpenID Connect parameters
                ClientId = clientId,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                { 
                    AuthenticationFailed = OnAuthenticationFailed,
                    AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                },

                // Required for AAD B2C
                Scope = "openid offline_access",

                // The PolicyConfigurationManager takes care of getting the correct Azure AD authentication
                // endpoints from the OpenID Connect metadata endpoint.  It is included in the PolicyAuthHelpers folder.
                ConfigurationManager = new PolicyConfigurationManager(String.Format(aadInstance, tenant, "/v2.0", discoverySuffix)),

                // Optional - used for displaying the user's name in the navigation bar when signed in.
                TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                {  
                    NameClaimType = "name",
                },
            };

            // The PolicyOpenIdConnectAuthenticationMiddleware is a small extension of the default OpenIdConnectMiddleware
            // included in OWIN.  It is included in this sample in the PolicyAuthHelpers folder, along with a few other
            // supplementary classes related to policies.
            app.Use(typeof(PolicyOpenIdConnectAuthenticationMiddleware), app, options);
                
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
        {
            // The user's objectId is extracted from the claims provided in the id_token, and used to cache tokens in ADAL
            // The authority is constructed by appending your B2C directory's name to "https://login.microsoftonline.com/"
            // The client credential is where you provide your application secret, and is used to authenticate the application to Azure AD
            string userObjectID = notification.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant, string.Empty, string.Empty);
            ClientCredential credential = new ClientCredential(clientId, clientSecret);

            // We don't care which policy is used to access the TaskService, so let's use the most recent policy
            string mostRecentPolicy = notification.AuthenticationTicket.Identity.FindFirst(Startup.AcrClaimType).Value;

            // The Authentication Context is ADAL's primary class, which represents your connection to your B2C directory
            // ADAL uses an in-memory token cache by default.  In this case, we've extended the default cache to use a simple per-user session cache
            AuthenticationContext authContext = new AuthenticationContext(authority, new NaiveSessionCache(userObjectID));

            // Here you ask for a token using the web app's clientId as the scope, since the web app and service share the same clientId.
            // The token will be stored in the ADAL token cache, for use in our controllers
            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(notification.Code, new Uri(redirectUri), credential, new string[] { clientId }, mostRecentPolicy);
        }

        // Used for avoiding yellow-screen-of-death
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}