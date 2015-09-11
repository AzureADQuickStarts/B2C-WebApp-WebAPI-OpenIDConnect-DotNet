using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// The following using statements were added for this sample.
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using TaskWebApp.Policies;
using System.Security.Claims;

namespace TaskWebApp.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                Response.Headers.Add(PolicyOpenIdConnectAuthenticationHandler.PolicyKey, Startup.SignInPolicyId);
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }
        public void SignUp()
        {
            if (!Request.IsAuthenticated)
            {
                Response.Headers.Add(PolicyOpenIdConnectAuthenticationHandler.PolicyKey, Startup.SignUpPolicyId);
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }


        public void Profile()
        {
            if (Request.IsAuthenticated)
            {
                Response.Headers.Add(PolicyOpenIdConnectAuthenticationHandler.PolicyKey, Startup.ProfilePolicyId);
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            Response.Headers.Add(PolicyOpenIdConnectAuthenticationHandler.PolicyKey, ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value);
            HttpContext.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
	}
}