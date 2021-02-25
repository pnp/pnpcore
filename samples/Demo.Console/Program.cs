using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // .UseEnvironment("officedevpnp")
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

                // Add the PnP Core SDK services
                services.AddPnPCore(options => {

                    // You can explicitly configure all the settings, or you can
                    // simply use the default values

                    options.PnPContext.GraphFirst = true;
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

                    //options.DefaultAuthenticationProvider = authenticationProvider;

                    options.Sites.Add("DemoSite",
                        new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                        {
                            SiteUrl = customSettings.DemoSiteUrl
                           
                        });
                });

                // PnP Core Authentication
                // To check out more authentication options check out the documentation for more information:
                //  https://pnp.github.io/pnpcore/using-the-sdk/configuring%20authentication.html
                services.AddPnPCoreAuthentication(
                    options =>
                    {
                        options.Credentials.Configurations.Add("interactive",
                            new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationCredentialConfigurationOptions
                            {
                                ClientId = customSettings.ClientId,
                                TenantId = customSettings.TenantId,
                                Interactive = new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationInteractiveOptions
                                {
                                    RedirectUri = customSettings.RedirectUri
                                }
                            });

                        options.Credentials.Configurations.Add("credentials",
                            new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationCredentialConfigurationOptions
                            {
                                ClientId = customSettings.ClientId,
                                TenantId = customSettings.TenantId,
                                Interactive = new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationInteractiveOptions
                                {
                                    RedirectUri = customSettings.RedirectUri
                                }
                                //},
                                //CredentialManager = new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationCredentialManagerOptions
                                //{
                                //    CredentialManagerName = customSettings.CredentialManager
                                //}
                                
                            });

                        // Configure the default authentication provider
                        options.Credentials.DefaultConfiguration = "interactive";

                        // Map the site defined in AddPnPCore with the 
                        // Authentication Provider configured in this action
                        options.Sites.Add("DemoSite",
                            new PnP.Core.Auth.Services.Builder.Configuration.PnPCoreAuthenticationSiteOptions
                            {
                                AuthenticationProviderName = "interactive"
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
                    Console.WriteLine($"# Lists: {web.Lists.Length}");
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
                        Console.WriteLine($"# Items: {demo1List.Items.Length}");
                        Console.ResetColor();
                    }

                    // Getting the messages in the default team channel, first ensure 
                    await team.LoadAsync(p => p.PrimaryChannel);
                    await team.PrimaryChannel.LoadAsync(p => p.Messages);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Team channel messages (Graph beta)===");
                    Console.WriteLine($"Title: {team.PrimaryChannel.DisplayName}");
                    Console.WriteLine($"# Messages: {team.PrimaryChannel.Messages.Length}");
                    Console.ResetColor();
                }
                #endregion

                #region Linq query support!
                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    context.GraphFirst = false;

                    // We can retrieve the whole list of lists 
                    // and their items in the context web
                    var listsQuery = (from l in context.Web.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Description)
                                      orderby l.Title descending
                                      select l);

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
                    var demo1List = context.Web.Lists.GetByTitle("Site Assets", l => l.Id, l => l.Title, l => l.Description);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===LINQ: Retrieve specific list===");
                    Console.WriteLine($"Just got list '{demo1List.Title}' with ID '{demo1List.Id}'");
                    Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // We can retrieve items of a specific list 
                    // eventually partitioning the results
                    var itemsQuery = (from i in context.Web.Lists.GetByTitle("Site Assets").Items
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
                    // TODO: Issue with Cascading sychronous loads
                    //var listItem = context.Web.Lists.GetByTitle("Site Assets").Items.GetById(1);

                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.WriteLine("===LINQ: Retrieve list item by id===");
                    //Console.WriteLine($"Item with title '{listItem.Title}' has ID: {listItem.Id}");
                    //Console.ResetColor();
                }

                using (var context = pnpContextFactory.Create("DemoSite"))
                {
                    // Or we can retrieve a specific document from a library
                    var document = context.Web.Lists.GetByTitle("Site Assets").Items
                        .QueryProperties(i => i.Id, i => i.Title)
                        .Where(i => i.Title == "__siteIcon__.png")
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
                    await newList.LoadBatchAsync(context.CurrentBatch, p => p.Items, p => p.NoCrawl);

                    // Send 20 adds + reload as a single operation (=batch) to server
                    await context.ExecuteAsync();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"List with title '{newList.Title}' has {newList.Items.Length} items");
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
                    var team = await context.Team.GetAsync(p => p.Channels, 
                        p => p.PrimaryChannel, p => p.FunSettings);

                    // Ensure the needed properties were loaded
                    await team.PrimaryChannel.EnsurePropertiesAsync(p => p.DisplayName, p => p.Tabs, p => p.Messages);

                    // Add/Delete a new tab in the primary channel
                    // TODO: Investigate 
                    //var pnpTab = team.PrimaryChannel.Tabs.FirstOrDefault(p => p.DisplayName == "PnPTab");
                    //if (pnpTab != null)
                    //{
                    //    await pnpTab.DeleteAsync();
                    //    Console.ForegroundColor = ConsoleColor.Yellow;
                    //    Console.WriteLine("Deleted tab PnPTab");
                    //    Console.ResetColor();
                    //}

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
                    await team.PrimaryChannel.EnsurePropertiesAsync(p => p.Messages);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Length}");
                    Console.ResetColor();

                    if (team.PrimaryChannel.Messages.Length > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("===Next page loaded==");
                        Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Take(2).AsEnumerable().Count()}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("===Next page loaded==");
                        Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Take(2).Skip(2).AsEnumerable().Count()}");
                        Console.ResetColor();

                        Console.WriteLine("===All pages loaded==");
                        Console.WriteLine($"Current number of messages: {team.PrimaryChannel.Messages.Length}");
                        Console.ResetColor();
                    }
                }
                #endregion

                #region Pages API

                var pageName = $"Page-{Guid.NewGuid().ToString("N")}.aspx";

                // Create a modern page
                using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
                {
                    var page = await context.Web.NewPageAsync();

                    // A simple section and text control to the page
                    page.AddSection(CanvasSectionTemplate.TwoColumnRight, 1);
                    page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                    // and a countdown web part
                    var availableComponents = await page.AvailablePageComponentsAsync();
                    var countdownWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.CountDown));
                    var countdownWebPart = page.NewWebPart(countdownWebPartComponent);

                    // Read the json settings for the countdown web part
                    countdownWebPart.PropertiesJson = await new StreamReader(@"..\..\..\countdownWebPart.json").ReadToEndAsync();
                    page.AddControl(countdownWebPart, page.Sections[0].Columns[1]);

                    // Set the page title
                    page.PageTitle = "Modern Page created with PnP Core SDK";

                    // Save the page
                    await page.SaveAsync(pageName);
                    
                    // And publish it
                    await page.PublishAsync();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Modern Pages API (REST)===");
                    Console.WriteLine($"Page Title: {page.PageTitle}");
                    Console.WriteLine($"# Page Name: {pageName}");
                    Console.ResetColor();
                }

                // Inspect a modern page
                using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
                {
                    var page = (await context.Web.GetPagesAsync(pageName)).FirstOrDefault();

                    // Read the Countdown Web Part Configuration
                    var countdownWebPartJson = page.Sections[0].Controls[1].JsonControlData;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Modern Pages API (REST)===");
                    Console.WriteLine($"Image Web Part Configuraiton");
                    Console.WriteLine(countdownWebPartJson);
                    Console.ResetColor();
                }

                #endregion
            }

            host.Dispose();
        }       
    }
}
