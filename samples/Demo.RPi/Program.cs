using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Demo.RPi
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()

           // Ensure you do consent to the PnP App when using another tenant (update below url to match your aad domain): 
           // https://login.microsoftonline.com/a830edad9050849523e17050400.onmicrosoft.com/adminconsent?client_id=31359c7f-bd7e-475c-86db-fdb8c937548e&state=12345&redirect_uri=https://www.pnp.com
           //.UseEnvironment("officedevpnp")
           .ConfigureLogging((hostingContext, logging) =>
           {
               logging.AddConsole();
           })
           .ConfigureServices((hostingContext, services) =>
           {
               var customSettings = new CustomSettings();
               hostingContext.Configuration.Bind("CustomSettings", customSettings);

              //Create an instance of the Authentication Provider that uses Credential Manager
              var authenticationProvider = new UsernamePasswordAuthenticationProvider(
                              customSettings.ClientId,
                              customSettings.TenantId,
                              customSettings.UserPrincipalName,
                              StringToSecureString(customSettings.Password));

               services.AddPnPCore(options =>
               {
                   options.DefaultAuthenticationProvider = authenticationProvider;

                   options.Sites.Add("DemoSite",
                       new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                       {
                           SiteUrl = customSettings.DemoSiteUrl,
                           AuthenticationProvider = authenticationProvider
                       });
               });               
           })
           // Let the builder know we're running in a console
           .UseConsoleLifetime()
           // Add services to the container
           .Build();

            await host.StartAsync();

            using (var scope = host.Services.CreateScope())
            {
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

                #region Interactive GET's
                using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
                {
                    // ================================================================
                    // Getting data - everything is async!
                    // Same programming model, regardless of wether under the covers Microsoft Graph is used or SharePoint REST

                    // Interactive GET samples

                    // Retrieving web with lists and masterpageurl loaded ==> SharePoint REST query
                    var web = await context.Web.GetAsync(p => p.Title, p => p.Lists, p => p.MasterUrl);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Web (REST)===");
                    Console.WriteLine($"Title: {web.Title}");
                    Console.WriteLine($"# Lists: {web.Lists.Count()}");
                    Console.ResetColor();                    
                }
                #endregion
            }

            host.Dispose();
        }

        private static SecureString StringToSecureString(string inputString)
        {
            var securityString = new SecureString();
            char[] chars = inputString.ToCharArray();
            foreach (var c in chars)
            {
                securityString.AppendChar(c);
            }
            return securityString;
        }
    
    }
}
