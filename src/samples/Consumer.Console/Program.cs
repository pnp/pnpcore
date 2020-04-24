using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core;
using PnP.Core.Services;
using PnP.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using PnP.Core.Model.SharePoint.Core;

namespace Consumer
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()

            // Ensure you do consent to the PnP App when using another tenant (update below url to match your aad domain): 
            // https://login.microsoftonline.com/a830edad9050849523e17050400.onmicrosoft.com/adminconsent?client_id=31359c7f-bd7e-475c-86db-fdb8c937548e&state=12345&redirect_uri=https://www.pnp.com
            .UseEnvironment("paolopia")
            //.ConfigureAppConfiguration((hostingContext, config) =>
            //{
            //    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
            //    config.AddEnvironmentVariables();
            //    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            //    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            //    if (args != null)
            //    {
            //        config.AddCommandLine(args);
            //    }
            //})
            .ConfigureLogging((hostingContext, logging) =>
            {
                //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddEventSourceLogger();
                logging.AddConsole();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                var customSettings = new CustomSettings();
                hostingContext.Configuration.Bind("CustomSettings", customSettings);

                services
                .AddFakeAuthenticationProvider()
                .AddAuthenticationProviderFactory(options =>
                {
                    options.Configurations.Add(new OAuthCredentialManagerConfiguration
                    {
                        Name = "CredentialManagerAuthentication",
                        CredentialManagerName = customSettings.CredentialManager,
                    });
                    options.Configurations.Add(new OAuthUsernamePasswordConfiguration
                    {
                        Name = "UsernameAndPasswordAuthentication",
                        Username = customSettings.UserPrincipalName,
                        Password = customSettings.Password.ToSecureString(),
                    });
                    options.Configurations.Add(new FakeAuthenticationProviderConfiguration
                    {
                        Name = "FakeAuthentication",
                        FakeSetting = "This is just a fake setting",
                    });
                    options.Configurations.Add(new OAuthCertificateConfiguration
                    {
                        Name = "CertificateAuthentication",
                        Certificate = X509CertificateUtility.LoadCertificate(
                            System.Security.Cryptography.X509Certificates.StoreName.My,
                            System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                            customSettings.X509CertificateThumbprint),
                    });

                    options.DefaultConfiguration = "CredentialManagerAuthentication";
                })
                .AddPnPContextFactory(options =>
                {
                    options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                    {
                        Name = "DevSite",
                        SiteUrl = new Uri(customSettings.TargetSiteUrl),
                        AuthenticationProviderName = "CredentialManagerAuthentication",
                    });
                    options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                    {
                        Name = "DevSubSite",
                        SiteUrl = new Uri(customSettings.TargetSubSiteUrl),
                        AuthenticationProviderName = "CredentialManagerAuthentication",
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
                using (var context = pnpContextFactory.Create("DevSite"))
                {
                    /**
                    // -- Graph only support - Teams + batch splitting --
                    //var team = await context.Team.GetAsync(p => p.Owners, p => p.Members);
                    var team = context.Team.Get(p => p.Owners, p => p.Members, p => p.InstalledApps);
                    // Adding a request to the batch that has to be handled via rest ==> will trigger the batch splitting logic
                    var web = context.Web.Get(p => p.SearchScope);
                    await context.ExecuteAsync();
                    
                    Console.WriteLine("Waiting...");
                    */

                    /**
                    // -- Load collection via REST and then reload it again via Graph -- 
                    // ==> resulting items in the collection should be correctly merged
                    var web = await context.Web.GetAsync(p => p.Title, p => p.Lists, p => p.SearchScope);
                    await context.Web.GetAsync(p => p.Lists);

                    Console.WriteLine("Waiting...");
                    */

                    /**
                    // -- SharePoint Graph testing --
                    //var web = context.Web.Get(p => p.Title, p => p.Lists, p => p.SearchScope);
                    var web = context.Web.Get(p => p.Lists);
                    context.Web.Get(p => p.SearchScope);
                    await context.ExecuteAsync();

                    var demoList1 = web.Lists.Where(p => p.Title.Equals("DemoList1", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (demoList1 != null)
                    {
                        // Get list items via Graph
                        //await demoList1.GetAsync(p => p.Items);
                        await demoList1.GetAsync();

                        // Update via rest
                        var firstItem = demoList1.Items.FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = $"Updated on {DateTime.Now.ToString()}";
                            await firstItem.UpdateAsync();
                        }
                    }

                    Console.WriteLine("Waiting...");
                    */

                    // -- ListItem add/update/delete test -- 
                    var web = context.Web.Get(p => p.Url, p => p.Title, p => p.Lists, p => p.SearchScope);
                    await context.ExecuteAsync();

                    string listTitle = "demolist1";
                    var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (myList == null)
                    {
                        // Add a new list
                        myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // get items from the list
                    await myList.GetAsync(p => p.Items);

                    if (myList.Items.Count() == 0)
                    {

                        // Add a number of list items and fetch them again in a single server call
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", "Yesssss" }
                        };
                        
                        var item = await myList.Items.AddAsync(values);
                        //var item1 = myList.Items.Add(values);
                        
                        // Combining add and getting the added item again in a single batch is not possible, batching does not allow this
                        //item1.Get(p => p.CommentsDisabled);

                        myList.Items.Add(values);
                        myList.Items.Add(values);
                        myList.Get(p => p.Items);
                        await context.ExecuteAsync();
                    }
                    else
                    {
                        // Delete the first list item
                        //var itemToDelete = myList.Items.OrderBy(p => p.Id).FirstOrDefault();
                        //if (itemToDelete != null)
                        //{
                        //    await itemToDelete.DeleteAsync();
                        //}

                        //Delete all items in batch
                        //foreach (var item in myList.Items)
                        //{
                        //    item.Delete();
                        //}
                        //await context.ExecuteAsync();

                        //var itemToUpdate = myList.Items.OrderBy(p => p.Id).LastOrDefault();
                        //if (itemToUpdate != null)
                        //{
                        //    itemToUpdate.Values["Title"] = "Updated!!";
                        //    await itemToUpdate.UpdateAsync();
                        //}

                        foreach (var item in myList.Items)
                        {
                            item.Values["Title"] = $"Updated on via Values {DateTime.Now.ToString()}";
                            dynamic dItem = item;
                            dItem.Title = $"Updated on via Expando {DateTime.Now.ToString()}";
                            //dItem.MyCustomField = 10;
                            //dItem.AnotherCustomField = "Something else";
                            item.Update();
                        }
                        await context.ExecuteAsync();

                        var firstItem = myList.Items.First();

                        dynamic dynamicFirtItem = firstItem;
                    }

                    //myList.Items.First().Get(p => p.CommentsDisabled);
                    //await context.ExecuteAsync();

                    Console.WriteLine("Waiting...");

                    /**
                    // -- List Add/Update/Delete test --
                    var web = context.Web.Get(p => p.Url, p => p.Title, p => p.Lists, p => p.Webs);
                    await context.ExecuteAsync();

                    string listTitle = "new list";
                    var listToCreateOrDelete = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (listToCreateOrDelete == null)
                    {
                        // Add a new list
                        var newList = await web.Lists.AddAsync(listTitle, 100);
                        
                        // Update the added list
                        newList.Description = "yes";
                        await newList.UpdateAsync();
                    }
                    else
                    {
                        // Delete a list
                        await listToCreateOrDelete.DeleteAsync();
                    }
                    */

                    /**
                    // -- Loading webs of webs of webs --
                    Console.WriteLine($"Webs was requested: {context.Web.Webs.Requested}");

                    var web = context.Web.Get(p => p.AlternateCSS, p => p.Url, p => p.Title, p => p.Lists, p => p.Webs);
                    await context.ExecuteAsync();

                    context.Web.Get(p => p.Webs);
                    await context.ExecuteAsync();

                    Console.WriteLine($"Webs was requested: {context.Web.Webs.Requested}");

                    Console.WriteLine($"Web at URL '{web.Url}' has title: '{web.Title}'");

                    // load the lists, sub webs from each sub web in a batch
                    foreach (var subWeb in web.Webs)
                    {
                        subWeb.Get(p => p.AlternateCSS, p => p.Lists);
                        subWeb.Get(p => p.Url, p => p.Webs);
                    }
                    await context.ExecuteAsync();

                    Console.WriteLine($"Web has {web.Webs.Count()} sub webs");
                    */

                    /**
                    // -- List Item test code --
                    //var list = await context.Web.Lists.GetByTitleAsync("demolist1", p => p.Title, p => p.Items);   

                    // The same list is fetched twice in this batch - both objects are consolidated automatically
                    var list1 = context.Web.Lists.GetByTitle("demolist1", p => p.Title, p => p.NoCrawl);
                    var list1Again = context.Web.Lists.GetByTitle("demolist1", p => p.Title, p => p.EnableVersioning, p => p.Items);
                    //var list2 = context.Web.Lists.GetByTitle("Style library", p => p.Title, p => p.Items);
                    await context.ExecuteAsync();

                    Console.WriteLine($"Number of items: {list1.Items.Count()}");

                    Console.WriteLine($"Number of lists: {context.Web.Lists.Count()}");                    
                    Console.WriteLine($"Was title loaded: {list1.HasValue("title")}");

                    //await list1.Items.First().GetAsync(p => p.CommentsDisabled);
                    foreach (var item in list1.Items)
                    {
                        item.Get(p => p.CommentsDisabled);
                    }
                    await context.ExecuteAsync();

                    Console.WriteLine($"Number of items: {list1.Items.Count()}");
                    */

                    /**
                    // -- Batch test code --
                    // Explicit batching
                    //var batch = context.NewBatch();
                    //var web = context.Web.Get(batch, p => p.SiteLogo, p => p.Title, p => p.Id, p => p.Description, p => p.Lists, p => p.MasterPageUrl, p => p.CustomMasterPageUrl, p => p.SearchScope, p => p.NoCrawl);
                    //var site = context.Site.Get(batch, p => p.Id, p => p.RootWeb);
                    //await context.ExecuteAsync(batch);

                    // Implicit batching
                    var web = context.Web.Get(p => p.SiteLogo, p => p.Title, p => p.Id, p => p.Description, p => p.Lists, p => p.MasterPageUrl, p => p.CustomMasterPageUrl, p => p.SearchScope, p => p.NoCrawl);
                    var site = context.Site.Get(p => p.Id, p => p.RootWeb);
                    await context.ExecuteAsync();

                    // Direct loads
                    //var web = await context.Web.GetAsync(p => p.SiteLogo, p => p.Title, p => p.Id, p => p.Description, p => p.Lists, p => p.MasterPageUrl, p => p.CustomMasterPageUrl, p => p.SearchScope, p => p.NoCrawl);
                    //var site = await context.Site.GetAsync(p => p.Id, p => p.RootWeb);

                    // Save the original title value
                    var originalTitle = web.Title;

                    // Load properties of a child object
                    web.Title = "this title should not be overwritten with below load";
                    await site.RootWeb.GetAsync(p => p.Id);

                    // Load the lists again...we should still see the same amount of lists in our model
                    context.Web.Get(p => p.Lists);
                    await context.ExecuteAsync();
                    */

                    /**
                    // -- Update test code --
                    // Save the original title value
                    var web = await context.Web.GetAsync(p => p.SiteLogo, p => p.Title, p => p.Id, p => p.Description, p => p.Lists, p => p.MasterPageUrl, p => p.CustomMasterPageUrl, p => p.SearchScope, p => p.NoCrawl);
                    var originalTitle = web.Title;

                    // Update property of web
                    web.Title = $"1 - Updated on {DateTime.Now.ToLongTimeString()}";
                    web.Description = $"Updated on {DateTime.Now.ToString()}";
                    web.SearchScope = PnP.Core.Model.SharePoint.SearchScopes.Site;
                    web.Update();

                    // Make a get request to include it into the current batch
                    //web = context.Web.Get(p => p.Title);

                    await context.ExecuteAsync();

                    Console.WriteLine($"Web title: {web.Title}");

                    // Make a get request to include it into the current batch
                    //web = context.Web.Get(p => p.Title);

                    // Update the web title back to the original value
                    web.Title = $"2 - Updated on {DateTime.Now.ToLongTimeString()}";
                    web.Update();

                    await context.ExecuteAsync();

                    Console.WriteLine($"Web title: {web.Title}");
                    */
                }
            }

            host.Dispose();
        }
    }
}
