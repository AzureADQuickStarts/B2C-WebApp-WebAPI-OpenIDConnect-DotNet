using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskWebApp.Policies
{
    public class PolicyOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly ILogger _logger;

        public PolicyOpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options)
            : base(next, app, options)
        {
            _logger = app.CreateLogger<PolicyOpenIdConnectAuthenticationMiddleware>();
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new PolicyOpenIdConnectAuthenticationHandler(_logger);
        }
    }
}
