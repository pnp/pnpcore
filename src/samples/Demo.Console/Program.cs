using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Consumer
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
                logging.AddEventSourceLogger();
                logging.AddConsole();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                // Read the custom configuration from the appsettings.<environment>.json file
                var customSettings = new CustomSettings();
                hostingContext.Configuration.Bind("CustomSettings", customSettings);

                // Create an instance of the Authentication Provider that uses Credential Manager
                //var authenticationProvider = new CredentialManagerAuthenticationProvider(
                //                customSettings.ClientId,
                //                customSettings.TenantId,
                //                customSettings.CredentialManager);                

                var authenticationProvider = new InteractiveAuthenticationProvider(
                                customSettings.ClientId,
                                customSettings.TenantId,
                                customSettings.RedirectUri);

                // Add the PnP Core SDK services
                services
                .AddPnPCore(options => {

                    // You can explicitly configure all the settings, or you can
                    // simply use the default values

                    //options.PnPContext.GraphFirst = true;
                    //options.PnPContext.GraphCanUseBeta = true;
                    //options.PnPContext.GraphAlwaysUseBeta = false;

                    //options.HttpRequests.UserAgent = "NONISV|SharePointPnP|PnPCoreSDK";
                    //options.HttpRequests.MicrosoftGraph = new PnPCoreHttpRequestsGraphOptions
                    //{
                    //    UseRetryAfterHeader = true,
                    //    MaxRetries = 10,
                    //    DelayInSeconds = 3,
                    //    UseIncrementalDelay = true,
                    //};
                    //options.HttpRequests.SharePointRest = new PnPCoreHttpRequestsSharePointRestOptions
                    //{
                    //    UseRetryAfterHeader = true,
                    //    MaxRetries = 10,
                    //    DelayInSeconds = 3,
                    //    UseIncrementalDelay = true,
                    //};

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
                    Console.WriteLine($"Master page url: {web.MasterUrl}");
                    Console.ResetColor();

                    // Getting the team connected to this Modern Team site ==> Microsoft Graph query
                    var team = await context.Team.GetAsync();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Team (Graph v1)===");
                    Console.WriteLine($"Name: {team.DisplayName}");
                    Console.WriteLine($"Visibility: {team.Visibility}");
                    Console.WriteLine($"Funsettings.AllowGiphy: {team.FunSettings.AllowGiphy}");
                    Console.ResetColor();

                    // Getting a specific list and loading it's items
                    var demo1List = web.Lists.FirstOrDefault(p => p.Title == "Demo1");
                    if (demo1List != null)
                    {
                        // Load list items
                        await demo1List.GetAsync(p => p.Items);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("===List (Graph v1)===");
                        Console.WriteLine($"Title: {demo1List.Title}");
                        Console.WriteLine($"# Items: {demo1List.Items.Count()}");
                        Console.ResetColor();
                    }

                    // Getting the messages in the default team channel, first ensure 
                    await team.EnsurePropertiesAsync(p => p.PrimaryChannel);
                    await team.PrimaryChannel.GetAsync(p => p.Messages);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Team channel messages (Graph beta)===");
                    Console.WriteLine($"Title: {team.PrimaryChannel.DisplayName}");
                    Console.WriteLine($"# Messages: {team.PrimaryChannel.Messages.Count()}");
                    Console.ResetColor();
                }
                #endregion

                #region Linq query support!
                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    context.GraphFirst = false;

                    // We can retrieve the whole list of lists 
                    // and their items in the context web
                    var listsQuery = (from l in context.Web.Lists
                                      orderby l.Title descending
                                      select l)
                                    .Load(l => l.Id, l => l.Title, l => l.Description)
                                    .Include(l => l.Items);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===LINQ: Retrieve list and list items===");
                    foreach (var list in listsQuery.ToList())
                    {
                        Console.WriteLine($"{list.Id} - {list.Title} - Items count: {list.Items.Length}");
                    }
                    Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // Or we can easily get a specific list
                    var demo1List = context.Web.Lists.GetByTitle("Demo1", l => l.Id, l => l.Title, l => l.Description);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===LINQ: Retrieve specific list===");
                    Console.WriteLine($"Just got list '{demo1List.Title}' with ID '{demo1List.Id}'");
                    Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // We can retrieve items of a specific list 
                    // eventually partitioning the results
                    var itemsQuery = (from i in context.Web.Lists.GetByTitle("Demo1").Items
                                      select i).Take(2).Skip(1);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===LINQ: Retrieve specific list with partitioned items===");
                    foreach (var item in itemsQuery)
                    {
                        Console.WriteLine($"Item with title '{item.Title}' has ID: {item.Id}");
                    }
                    Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // Or we can retrieve a specific item
                    var listItem = context.Web.Lists.GetByTitle("Demo1").Items.GetById(4,
                        i => i.Id, i => i.Title);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===LINQ: Retrieve list item by id===");
                    Console.WriteLine($"Item with title '{listItem.Title}' has ID: {listItem.Id}");
                    Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // Or we can retrieve a specific document from a library
                    var document = context.Web.Lists.GetByTitle("Documents").Items
                            .Where(i => i.Title == "Sample-File-03")
                            .Load(i => i.Id, i => i.Title)
                            .FirstOrDefault();

                    if (document != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("===LINQ: Retrieve document by title===");
                        Console.WriteLine($"Document with title '{document.Title}' has ID: {document.Id}");
                        Console.ResetColor();
                    }
                }
                #endregion

                #region Add/Update/Delete/Batching
                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===ADD/UPDATE/DELETE: create/delete list and add 20 items in batch===");
                    Console.ResetColor();
                    // Check if lists exists and delete first if needed                    
                    var newList = context.Web.Lists.GetByTitle("AddTest", l => l.Id, l => l.Title, l => l.Description);
                    if (newList != null)
                    {
                        await newList.DeleteAsync();
                    }
                    // Add the new list
                    newList = await context.Web.Lists.AddAsync("AddTest", ListTemplateType.GenericList);

                    // In batch add 20 items
                    for (int i = 0; i < 20; i++)
                    {
                        Dictionary<string, object> listItem = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                        await newList.Items.AddBatchAsync(listItem);
                    }
                    await newList.GetBatchAsync(p => p.Items, p => p.NoCrawl);

                    // Send 20 adds + reload as a single operation (=batch) to server
                    await context.ExecuteAsync();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"List with title '{newList.Title}' has {newList.Items.Count()} items");
                    Console.ResetColor();

                    // Update item
                    var itemToUpdate = newList.Items.FirstOrDefault();
                    itemToUpdate.Values["Title"] = $"Updated at {DateTime.UtcNow}";
                    await itemToUpdate.UpdateAsync();

                    // Update item using dynamic syntax
                    //dynamic itemToUpdateDynamic = itemToUpdate;
                    //itemToUpdateDynamic["Title"] = $"Updated again at {DateTime.UtcNow}";
                    //await itemToUpdateDynamic.UpdateAsync();

                    //dynamic itemToUpdate = listToAdd.Items.FirstOrDefault();
                    //itemToUpdate.Title = $"Updated at {DateTime.UtcNow}";
                    //await itemToUpdate.UpdateAsync();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===ADD/UPDATE/DELETE: Add and configure a team channel===");
                    Console.ResetColor();

                    // Get team channels and primary channel
                    var team = await context.Team.GetAsync(p => p.Channels, p => p.PrimaryChannel, p => p.FunSettings);
                    // Ensure the needed properties were loaded
                    await team.PrimaryChannel.EnsurePropertiesAsync(p => p.DisplayName, p => p.Tabs, p => p.Messages);

                    // Add/Delete a new tab in the primary channel
                    var pnpTab = team.PrimaryChannel.Tabs.FirstOrDefault(p => p.DisplayName == "PnPTab");
                    if (pnpTab != null)
                    {
                        await pnpTab.DeleteAsync();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted tab PnPTab");
                        Console.ResetColor();
                    }

                    await team.PrimaryChannel.Tabs.AddDocumentLibraryTabAsync("PnPTab", new Uri($"{context.Uri}/Shared Documents"));

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Added tab PnPTab");
                    Console.ResetColor();

                    // Update the fun settings of the team
                    team.FunSettings.AllowGiphy = !team.FunSettings.AllowGiphy;
                    await team.UpdateAsync();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Updated team fun settings");
                    Console.ResetColor();


                    // Add a message in the general channel
                    await team.PrimaryChannel.Messages.AddAsync($"PnP rocks - {DateTime.UtcNow}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Added a message to the primary channel");
                    Console.ResetColor();
                }
                #endregion

                #region Paging
                using (var context = pnpContextFactory.Create("DemoSite"))
                {

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Paging support===");
                    Console.ResetColor();

                    // Get team channels and primary channel
                    var team = await context.Team.GetAsync(p => p.PrimaryChannel);
                    // Load the first set of messages
                    await team.PrimaryChannel.GetAsync(p => p.Messages);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Count()}");
                    Console.ResetColor();

                    if (team.PrimaryChannel.Messages.CanPage)
                    {
                        await team.PrimaryChannel.Messages.GetNextPageAsync();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("===Next page loaded==");
                        Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Count()}");
                        Console.ResetColor();

                        await team.PrimaryChannel.Messages.GetAllPagesAsync();
                        Console.WriteLine("===All pages loaded==");
                        Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Count()}");
                        Console.ResetColor();
                    }
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
