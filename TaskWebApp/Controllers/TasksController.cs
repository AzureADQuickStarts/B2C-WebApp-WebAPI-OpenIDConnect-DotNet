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

namespace TaskService.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private static string serviceUrl = ConfigurationManager.AppSettings["api:TaskServiceUrl"];

        // GET: TodoList
        public async Task<ActionResult> Index()
        {
            try { 

                // TODO: Get the BootstrapContext in order to access the sign in token

                // TODO: Attach the sign in token to the outgoing request

                // TODO: Call the task web API to get tokens
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=An Error Occurred Reading To Do List: " + ex.Message);
            }
        }

        // POST: TodoList/Create
        [HttpPost]
        public async Task<ActionResult> Create(string description)
        {
            try
            {
                var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;

                HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Text", description) });
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl + "/api/tasks");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bootstrapContext.Token);
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new RedirectResult("/Tasks");
                }
                else
                {
                    // If the call failed with access denied, show the user an error indicating they might need to sign-in again.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return new RedirectResult("/Error?message=Error: " + response.ReasonPhrase + " You might need to sign in again.");
                    }
                }

                return new RedirectResult("/Error?message=Error reading your To-Do List.");
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
            try
            {
                // TODO: Get a token and call the web API to delete tasks
            }
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=Error deleting your To-Do Item.  " + ex.Message);
            }
        }
    }
}