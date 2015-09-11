using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TaskWebApp.Policies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PolicyAuthorize : System.Web.Mvc.AuthorizeAttribute
    {
        public string Policy { get; set; }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.Headers.Add(PolicyOpenIdConnectAuthenticationHandler.PolicyKey, Policy);
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
