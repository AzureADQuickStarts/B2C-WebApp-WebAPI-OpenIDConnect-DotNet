using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TaskWebApp;
using TaskWebApp.Policies;
using TaskWebApp.Utils;

namespace TaskService.Controllers
{
    [PolicyAuthorize(Policy = "{Enter the name of your sign in policy, e.g. b2c_1_my_sign_in}")]
    public class TasksController : Controller
    {
        private static string serviceUrl = ConfigurationManager.AppSettings["api:TaskServiceUrl"];

        // GET: TodoList
        public async Task<ActionResult> Index()
        {
            // TODO: Get a token from the ADAL cache

            // TODO: Get tasks from the TaskService
        }

        // POST: TodoList/Create
        [HttpPost]
        public async Task<ActionResult> Create(string description)
        {
            AuthenticationResult result = null;

            try
            {
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                string authority = String.Format(CultureInfo.InvariantCulture, Startup.aadInstance, Startup.tenant, string.Empty, string.Empty);
                ClientCredential credential = new ClientCredential(Startup.clientId, Startup.clientSecret);

                string mostRecentPolicy = ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value;

                AuthenticationContext authContext = new AuthenticationContext(authority, new NaiveSessionCache(userObjectID));
                result = await authContext.AcquireTokenSilentAsync(new string[] { Startup.clientId }, credential, UserIdentifier.AnyUser, mostRecentPolicy);

                HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("task", description) });
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl + "/api/tasks");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new RedirectResult("/Tasks");
                }
                else
                {
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    // and show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Scope.Contains(Startup.clientId));
                        foreach (TokenCacheItem tci in todoTokens)
                            authContext.TokenCache.DeleteItem(tci);

                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=Error reading your To-Do List.");
            }
            catch (AdalException ex)
            {
                // If ADAL could not get a token silently, show the user an error indicating they might need to sign in again.
                return new RedirectResult("/Error?message=Error: " + ex.Message + " You might need to sign in again.");
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=Error reading your To-Do List.  " + ex.Message);
            }
        }

        // POST: /TodoList/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            AuthenticationResult result = null;

            try
            {
                // TODO: Get a token from ADAL

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, serviceUrl + "/api/tasks/" + id);
                
                // TODO: Attach the token to the HTTP DELETE request

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new RedirectResult("/Tasks");
                }
                else
                {
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    // and show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Scope.Contains(Startup.clientId));
                        foreach (TokenCacheItem tci in todoTokens)
                            authContext.TokenCache.DeleteItem(tci);

                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=Error deleting your To-Do Item.");
            }
            catch (AdalException ex)
            {
                // If ADAL could not get a token silently, show the user an error indicating they might need to sign in again.
                return new RedirectResult("/Error?message=Error: " + ex.Message + " You might need to sign in again.");
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=Error deleting your To-Do Item.  " + ex.Message);
            }
        }
    }
}