using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.Providers.Xml;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Console.Provisioning
{
    public static class Program
    {
        private static CustomSettings settings;
        private static IPnPContextFactory contextFactory;

        public static async Task<int> Main(string[] args)
        {
            CommandLine command = CommandLine.Parse(args);

            if (command.ShowHelp)
            {
                CommandLine.WriteUsage();
                return command.IsValid ? 0 : 1;
            }

            IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();

                    if (command.Verbose)
                    {
                        logging.SetMinimumLevel(LogLevel.Debug);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    settings = new CustomSettings();
                    context.Configuration.Bind("CustomSettings", settings);

                    services.AddPnPCore(options =>
                    {
                        options.PnPContext.GraphFirst = true;
                    });

                    services.AddPnPCoreAuthentication(options =>
                    {
                        options.Credentials.Configurations.Add("interactive",
                            new PnPCoreAuthenticationCredentialConfigurationOptions
                            {
                                ClientId = settings.ClientId,
                                TenantId = settings.TenantId,
                                Interactive = new PnPCoreAuthenticationInteractiveOptions
                                {
                                    RedirectUri = new Uri(settings.RedirectUri),
                                },
                            });

                        options.Credentials.DefaultConfiguration = "interactive";
                    });
                })
                .UseConsoleLifetime()
                .Build();

            await host.StartAsync().ConfigureAwait(false);

            int exitCode;

            using (IServiceScope scope = host.Services.CreateScope())
            {
                contextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

                if (command.IsExtract)
                {
                    exitCode = await RunExtractCommandAsync(command).ConfigureAwait(false);
                }
                else if (command.IsApply)
                {
                    exitCode = await RunApplyCommandAsync(command).ConfigureAwait(false);
                }
                else
                {
                    await RunMenuAsync().ConfigureAwait(false);
                    exitCode = 0;
                }
            }

            await host.StopAsync().ConfigureAwait(false);

            return exitCode;
        }

        #region Non-interactive apply
        private static async Task<int> RunApplyCommandAsync(CommandLine command)
        {
            string path = Path.GetFullPath(command.TemplatePath);

            if (!System.IO.File.Exists(path))
            {
                System.Console.Error.WriteLine($"No template at {path}");
                return 1;
            }

            System.Console.WriteLine($"Template: {path}");
            System.Console.WriteLine($"Target:   {command.SiteUrl}");
            System.Console.WriteLine();

            var problems = new List<string>();

            try
            {
                ProvisioningTemplate template = Load(path);

                Summarise(template);

                System.Console.WriteLine();
                System.Console.WriteLine("Connecting - a sign in window may appear...");

                using (PnPContext context = await contextFactory.CreateAsync(command.SiteUrl).ConfigureAwait(false))
                {
                    var configuration = new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            System.Console.WriteLine($"  [{type}] {message}");

                            if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                            {
                                problems.Add($"[{type}] {message}");
                            }
                        },

                        ProgressDelegate = (step, current, total) =>
                            System.Console.WriteLine($"  {current}/{total}  {step}"),
                    };

                    System.Console.WriteLine("Applying...");

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine();
                System.Console.Error.WriteLine("APPLY FAILED");
                System.Console.Error.WriteLine();
                System.Console.Error.WriteLine(ErrorReport.Describe(ex));

                if (problems.Count > 0)
                {
                    System.Console.Error.WriteLine();
                    System.Console.Error.WriteLine($"Reported before the failure ({problems.Count}):");

                    foreach (string problem in problems)
                    {
                        System.Console.Error.WriteLine($"  {problem}");
                    }
                }

                return 1;
            }

            System.Console.WriteLine();

            if (problems.Count == 0)
            {
                System.Console.WriteLine("Applied, with nothing reported.");
                return 0;
            }

            System.Console.WriteLine($"Applied, but {problems.Count} thing(s) were reported:");

            foreach (string problem in problems)
            {
                System.Console.WriteLine($"  {problem}");
            }

            return 2;
        }

        #endregion

        private static async Task RunMenuAsync()
        {
            while (true)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("=== PnP Core provisioning ===");
                System.Console.WriteLine("  1  Extract a template from a site");
                System.Console.WriteLine("  2  List saved templates");
                System.Console.WriteLine("  3  Show what a saved template contains");
                System.Console.WriteLine("  4  Apply a saved template to a site");
                System.Console.WriteLine("  0  Exit");
                System.Console.WriteLine();
                System.Console.Write("Choose: ");

                string choice = System.Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ExtractAsync().ConfigureAwait(false);
                            break;
                        case "2":
                            ListTemplates();
                            break;
                        case "3":
                            ShowTemplate();
                            break;
                        case "4":
                            await ApplyAsync().ConfigureAwait(false);
                            break;
                        case "0":
                        case null:
                            return;
                        default:
                            System.Console.WriteLine("Not a choice.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Caught rather than allowed to end the process: a mistyped url or a site you
                    // cannot reach should send you back to the menu, not out of the program.
                    //
                    // The full report, not ex.Message - PnP Core's message is a banner
                    // ("SharePoint Rest service exception") and everything diagnostic is on the
                    // error hanging off it. See ErrorReport.
                    System.Console.WriteLine();
                    System.Console.WriteLine("That did not work:");
                    System.Console.WriteLine();
                    System.Console.WriteLine(ErrorReport.Describe(ex));
                }
            }
        }

        #region Non-interactive extract

        private static async Task<int> RunExtractCommandAsync(CommandLine command)
        {
            var problems = new List<string>();

            try
            {
                System.Console.WriteLine($"Extracting {command.SiteUrl}");
                System.Console.WriteLine("Connecting - a sign in window may appear...");

                using (PnPContext context = await contextFactory.CreateAsync(command.SiteUrl).ConfigureAwait(false))
                {
                    ExtractConfiguration configuration = BuildExtractConfiguration(problems);

                    await ApplyContentOptionsAsync(context, configuration, command.IncludeItems,
                        command.ItemLists, command.IncludePages, command.IncludeHiddenLists).ConfigureAwait(false);

                    System.Console.WriteLine("Extracting...");

                    ProvisioningTemplate template = await context.GetProvisioningManager()
                        .GetTemplateAsync(configuration).ConfigureAwait(false);

                    System.IO.Directory.CreateDirectory(
                        Path.GetDirectoryName(Path.GetFullPath(command.TemplatePath)));

                    using (Stream stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
                    using (var file = System.IO.File.Create(command.TemplatePath))
                    {
                        stream.CopyTo(file);
                    }

                    System.Console.WriteLine();
                    System.Console.WriteLine($"Saved {Path.GetFullPath(command.TemplatePath)}");
                    Summarise(template);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("The extract failed:");
                System.Console.WriteLine();
                System.Console.WriteLine(ErrorReport.Describe(ex));

                return 1;
            }

            return problems.Count == 0 ? 0 : 2;
        }

        #endregion

        #region Extract

        private static async Task ExtractAsync()
        {
            Uri siteUrl = AskForSite("Extract from which site?");

            if (siteUrl == null)
            {
                return;
            }

            System.Console.Write("Save as (file name, blank for an automatic one): ");
            string name = System.Console.ReadLine()?.Trim();

            // A template is structure only unless asked otherwise, because content is the expensive
            // part - every item of every list, and every page read and rewritten.
            System.Console.WriteLine();
            System.Console.WriteLine("Structure - columns, content types, lists, security - is always included.");

            bool includeItems = AskYesNo("Include list items as well?");
            var itemLists = new List<string>();

            if (includeItems)
            {
                System.Console.Write("  Which lists (comma separated, blank for all): ");
                string lists = System.Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(lists))
                {
                    itemLists.AddRange(lists.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0));
                }

                System.Console.WriteLine("  Document libraries are skipped - see the note after the extract.");
            }

            bool includePages = AskYesNo("Include the site's pages and their contents?");
            bool includeHidden = AskYesNo("Include hidden lists?");

            System.Console.WriteLine();
            System.Console.WriteLine("Connecting - a sign in window may appear...");

            using (PnPContext context = await contextFactory.CreateAsync(siteUrl).ConfigureAwait(false))
            {
                ExtractConfiguration configuration = BuildExtractConfiguration(null);

                await ApplyContentOptionsAsync(context, configuration, includeItems, itemLists,
                    includePages, includeHidden).ConfigureAwait(false);

                System.Console.WriteLine("Extracting...");

                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync(configuration).ConfigureAwait(false);

                string path = Save(template, string.IsNullOrWhiteSpace(name)
                    ? $"{SafeName(siteUrl)}-{DateTime.Now:yyyyMMdd-HHmmss}.xml"
                    : EnsureXml(name));

                System.Console.WriteLine();
                System.Console.WriteLine($"Saved {path}");
                Summarise(template);
            }
        }

        private static ExtractConfiguration BuildExtractConfiguration(List<string> problems)
        {
            return new ExtractConfiguration
            {
                // Reported as it goes. An extract of a real site is not fast, and a console that
                // prints nothing for two minutes looks like a console that has hung.
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        System.Console.WriteLine($"  [{type}] {message}");
                        problems?.Add(message);
                    }
                },

                ProgressDelegate = (step, current, total) =>
                    System.Console.WriteLine($"  {current}/{total}  {step}"),
            };
        }

        /// <summary>
        /// Turns the "include content as well" answers into extract configuration.
        /// </summary>
        private static async Task ApplyContentOptionsAsync(PnPContext context, ExtractConfiguration configuration,
            bool includeItems, List<string> itemLists, bool includePages, bool includeHiddenLists)
        {
            configuration.Lists.IncludeHiddenLists = includeHiddenLists;

            // Pages are read by ObjectClientSidePageContents, which needs telling: without this the
            // extract records the library and none of the pages in it.
            configuration.Pages.IncludeAllClientSidePages = includePages;

            if (!includeItems)
            {
                return;
            }

            // Items are opted into per list, so "all lists" means enumerating them. Document
            // libraries are left out on purpose: ObjectListInstanceDataRows refuses them, and
            // asking anyway would produce a warning per library and nothing else.
            List<string> titles = itemLists != null && itemLists.Count > 0
                ? itemLists
                : await ListsWithItemsAsync(context, includeHiddenLists).ConfigureAwait(false);

            foreach (string title in titles)
            {
                configuration.Lists.Lists.Add(new PnP.Core.Provisioning.Model.Configuration.Lists.Lists.ExtractListsListsConfiguration
                {
                    Title = title,
                    IncludeItems = true,
                    Query = new PnP.Core.Provisioning.Model.Configuration.Lists.Lists.ExtractListsQueryConfiguration
                    {
                        IncludeAttachments = true,
                    },
                });
            }

            System.Console.WriteLine($"Including the items of {titles.Count} list(s): {string.Join(", ", titles)}");
        }

        private static async Task<List<string>> ListsWithItemsAsync(PnPContext context, bool includeHidden)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(
                l => l.Title, l => l.Hidden, l => l.BaseType)).ConfigureAwait(false);

            return context.Web.Lists.AsRequested()
                .Where(l => l.BaseType != ListBaseType.DocumentLibrary)
                .Where(l => includeHidden || !l.Hidden)
                .Select(l => l.Title)
                .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool AskYesNo(string question)
        {
            System.Console.Write($"{question} (y/N): ");
            string answer = System.Console.ReadLine()?.Trim();

            return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
        }

        private static string Save(ProvisioningTemplate template, string fileName)
        {
            System.IO.Directory.CreateDirectory(TemplateFolder);

            string path = Path.Combine(TemplateFolder, fileName);

            using (Stream stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
            using (var file = System.IO.File.Create(path))
            {
                stream.CopyTo(file);
            }

            return path;
        }

        #endregion

        #region Apply

        private static async Task ApplyAsync()
        {
            string path = AskForTemplate();

            if (path == null)
            {
                return;
            }

            Uri siteUrl = AskForSite("Apply to which site?");

            if (siteUrl == null)
            {
                return;
            }

            ProvisioningTemplate template = Load(path);

            System.Console.WriteLine();
            System.Console.WriteLine($"About to apply {Path.GetFileName(path)} to {siteUrl}");
            Summarise(template);

            System.Console.WriteLine();
            System.Console.WriteLine("Connecting - a sign in window may appear...");

            using (PnPContext context = await contextFactory.CreateAsync(siteUrl).ConfigureAwait(false))
            {
                var warnings = new List<string>();

                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                        {
                            warnings.Add($"[{type}] {message}");
                            System.Console.WriteLine($"  [{type}] {message}");
                        }
                    },

                    ProgressDelegate = (step, current, total) =>
                        System.Console.WriteLine($"  {current}/{total}  {step}"),
                };

                System.Console.WriteLine("Applying...");

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration)
                    .ConfigureAwait(false);

                System.Console.WriteLine();

                // Said plainly. The engine reports and continues rather than stopping at the first
                // problem, so "it finished" and "it all worked" are different claims.
                System.Console.WriteLine(warnings.Count == 0
                    ? "Applied, with nothing reported."
                    : $"Applied, but {warnings.Count} thing(s) were reported above - read them before " +
                      "assuming the site matches the template.");
            }
        }

        private static ProvisioningTemplate Load(string path)
        {
            using (Stream stream = System.IO.File.OpenRead(path))
            {
                ProvisioningTemplate template = XMLPnPSchemaFormatter.LatestFormatter.ToProvisioningTemplate(stream);

                // The connector is how a template reaches the files beside it - anything the template
                // uploads resolves through here. Without it, a template with files applies and
                // silently brings none of them.
                template.Connector = new PnP.Core.Provisioning.Connectors.FileSystemConnector(
                    Path.GetDirectoryName(Path.GetFullPath(path)), string.Empty);

                return template;
            }
        }

        #endregion

        #region Browsing

        private static void ListTemplates()
        {
            List<string> files = TemplateFiles();

            if (files.Count == 0)
            {
                System.Console.WriteLine($"No templates in {Path.GetFullPath(TemplateFolder)} yet - extract one first.");
                return;
            }

            System.Console.WriteLine();

            for (int i = 0; i < files.Count; i++)
            {
                var info = new FileInfo(files[i]);
                System.Console.WriteLine($"  {i + 1,2}  {info.Name}  ({info.Length / 1024} KB, {info.LastWriteTime:g})");
            }
        }

        private static void ShowTemplate()
        {
            string path = AskForTemplate();

            if (path == null)
            {
                return;
            }

            Summarise(Load(path));

            System.Console.Write("Show the raw XML? (y/N): ");

            if (string.Equals(System.Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine();
                System.Console.WriteLine(System.IO.File.ReadAllText(path));
            }
        }
        
        private static void Summarise(ProvisioningTemplate template)
        {
            var parts = new List<string>();

            Add(parts, "site column", template.SiteFields.Count);
            Add(parts, "content type", template.ContentTypes.Count);
            Add(parts, "list", template.Lists.Count);
            Add(parts, "client side page", template.ClientSidePages.Count);
            Add(parts, "file", template.Files.Count);
            Add(parts, "directory", template.Directories.Count);
            Add(parts, "term group", template.TermGroups.Count);
            Add(parts, "property bag entry", template.PropertyBagEntries.Count);
            Add(parts, "feature", template.Features.SiteFeatures.Count + template.Features.WebFeatures.Count);
            Add(parts, "custom action",
                template.CustomActions.SiteCustomActions.Count + template.CustomActions.WebCustomActions.Count);
            Add(parts, "site group", template.Security?.SiteGroups?.Count ?? 0);

            System.Console.WriteLine();
            System.Console.WriteLine(parts.Count == 0
                ? "  The template is empty."
                : "  Contains: " + string.Join(", ", parts));

            if (template.Navigation != null)
            {
                System.Console.WriteLine("  Contains navigation settings");
            }

            if (template.Header != null || template.Footer != null)
            {
                System.Console.WriteLine("  Contains site chrome settings");
            }
        }

        private static void Add(List<string> parts, string noun, int count)
        {
            if (count > 0)
            {
                parts.Add($"{count} {noun}{(count == 1 ? string.Empty : "s")}");
            }
        }

        #endregion

        #region Prompts

        private static Uri AskForSite(string question)
        {
            System.Console.Write($"{question} (full url, blank to cancel): ");

            string entered = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(entered))
            {
                return null;
            }

            if (!Uri.TryCreate(entered, UriKind.Absolute, out Uri url)
                || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
            {
                System.Console.WriteLine("That is not a url. Try https://contoso.sharepoint.com/sites/something");
                return null;
            }

            return url;
        }

        private static string AskForTemplate()
        {
            List<string> files = TemplateFiles();

            if (files.Count == 0)
            {
                System.Console.WriteLine($"No templates in {Path.GetFullPath(TemplateFolder)} yet - extract one first.");
                return null;
            }

            ListTemplates();

            System.Console.Write("Which one? (number, blank to cancel): ");

            string entered = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(entered))
            {
                return null;
            }

            if (!int.TryParse(entered, out int index) || index < 1 || index > files.Count)
            {
                System.Console.WriteLine("Not one of those.");
                return null;
            }

            return files[index - 1];
        }

        #endregion

        #region Helpers

        private static string TemplateFolder =>
            string.IsNullOrWhiteSpace(settings?.TemplateFolder) ? "Templates" : settings.TemplateFolder;

        private static List<string> TemplateFiles()
        {
            if (!System.IO.Directory.Exists(TemplateFolder))
            {
                return new List<string>();
            }

            return System.IO.Directory.GetFiles(TemplateFolder, "*.xml")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                .ToList();
        }

        private static string EnsureXml(string name)
        {
            return name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? name : $"{name}.xml";
        }

        private static string SafeName(Uri url)
        {
            var builder = new StringBuilder();

            foreach (char c in url.AbsolutePath.Trim('/'))
            {
                builder.Append(char.IsLetterOrDigit(c) ? c : '-');
            }

            string name = builder.ToString().Trim('-');

            return string.IsNullOrEmpty(name) ? "root" : name;
        }

        #endregion
    }
}
