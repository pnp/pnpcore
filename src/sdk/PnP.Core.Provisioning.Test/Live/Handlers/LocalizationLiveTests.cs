using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CustomActionModel = PnP.Core.Provisioning.Model.CustomAction;
using Directory = System.IO.Directory;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    [TestClass]
    public class LocalizationLiveTests : LiveTestBase
    {
        #region T6 - writing per-language values

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task Localization_ACustomActionTitleIsWrittenInEveryLanguageTheTemplateSupplies()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (CultureInfo first, CultureInfo second) = await PickTwoSupportedCulturesAsync(context).ConfigureAwait(false);

                string actionName = $"{TestPrefix}LocalizedAction";
                string resourceKey = "CustomAction_Title";
                string firstValue = $"Title in {first.Name}";
                string secondValue = $"Title in {second.Name}";

                string resourceFolder = CreateResourceFiles(new Dictionary<CultureInfo, string>
                {
                    { first, firstValue },
                    { second, secondValue },
                }, resourceKey);

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        Connector = new FileSystemConnector(resourceFolder, ""),
                    };

                    template.Localizations.Add(new Localization { LCID = first.LCID, Name = "test", ResourceFile = $"test.{first.Name}.resx" });
                    template.Localizations.Add(new Localization { LCID = second.LCID, Name = "test", ResourceFile = $"test.{second.Name}.resx" });

                    template.CustomActions.WebCustomActions.Add(new CustomActionModel
                    {
                        Name = actionName,
                        Title = $"{{res:{resourceKey}}}",
                        Description = "Localization test",
                        Location = "ClientSideExtension.ApplicationCustomizer",
                        ClientSideComponentId = new Guid("d0454bb0-3b4d-4e6d-9b0e-2e7ff5b6b2ea"),
                        Sequence = 100,
                        Enabled = true,
                    });

                    try
                    {
                        await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("APPLY FAILED:");
                        Console.WriteLine(Describe(ex));
                        throw;
                    }

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IUserCustomAction created = await FindActionAsync(fresh, actionName).ConfigureAwait(false);
                        Assert.IsNotNull(created, "The custom action was not created.");

                        (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(fresh).ConfigureAwait(false);

                        foreach ((CultureInfo culture, string expected) in new[] { (first, firstValue), (second, secondValue) })
                        {
                            string actual = await CsomRequestSender.SendAsync(fresh,
                                new GetValueForUICultureRequest(
                                    UserResourcePath.ForUserCustomAction(siteId, webId, created.Id, ResourceProperty.Title),
                                    culture.Name)).ConfigureAwait(false);

                            Console.WriteLine($"{culture.Name}: '{actual}'");

                            Assert.AreEqual(expected, actual,
                                $"The title was not persisted for {culture.Name}. " +
                                "A staged SetValueForUICulture reports success and writes nothing, so this is the real check.");
                        }
                    }
                }
                finally
                {
                    await DeleteActionAsync(actionName).ConfigureAwait(false);
                    TryDeleteFolder(resourceFolder);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task Localization_ATokenThatResolvesToNothingWarnsRatherThanWritingTheTokenItself()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string actionName = $"{TestPrefix}UnresolvedAction";

                try
                {
                    var template = new ProvisioningTemplate();
                    template.CustomActions.WebCustomActions.Add(new CustomActionModel
                    {
                        Name = actionName,
                        Title = "{res:NoSuchKey}",
                        Location = "ClientSideExtension.ApplicationCustomizer",
                        ClientSideComponentId = new Guid("d0454bb0-3b4d-4e6d-9b0e-2e7ff5b6b2ea"),
                        Enabled = true,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IUserCustomAction created = await FindActionAsync(fresh, actionName).ConfigureAwait(false);

                        Assert.IsNotNull(created, "An unresolvable token must not stop the action being created.");
                        Console.WriteLine($"Title as written: '{created.Title}'");
                    }
                }
                finally
                {
                    await DeleteActionAsync(actionName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task Localization_EveryRequestInABatchedReadGetsItsOwnResult()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (CultureInfo first, CultureInfo second) = await PickTwoSupportedCulturesAsync(context).ConfigureAwait(false);

                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);
                UserResourcePath resource = UserResourcePath.ForWeb(siteId, webId, ResourceProperty.Title);

                string firstValue = $"Batched {first.Name} {Guid.NewGuid():N}".Substring(0, 24);
                string secondValue = $"Batched {second.Name} {Guid.NewGuid():N}".Substring(0, 24);

                string originalFirst = await ReadAsync(context, resource, first).ConfigureAwait(false);
                string originalSecond = await ReadAsync(context, resource, second).ConfigureAwait(false);

                try
                {
                    await CsomRequestSender.SendManyAsync(context, new List<IRequest<object>>
                    {
                        new SetValueForUICultureRequest(resource, first.Name, firstValue),
                        new SetValueForUICultureRequest(resource, second.Name, secondValue),
                    }).ConfigureAwait(false);

                    var firstRead = new GetValueForUICultureRequest(resource, first.Name);
                    var secondRead = new GetValueForUICultureRequest(resource, second.Name);

                    await CsomRequestSender.SendManyAsync(context, new List<IRequest<object>> { firstRead, secondRead })
                        .ConfigureAwait(false);

                    Console.WriteLine($"{first.Name}: '{firstRead.Result}'");
                    Console.WriteLine($"{second.Name}: '{secondRead.Result}'");

                    Assert.AreEqual(firstValue, firstRead.Result, "The first request in the batch read the wrong value.");
                    Assert.AreEqual(secondValue, secondRead.Result,
                        "The SECOND request in the batch got nothing. Every request must see the shared response, " +
                        "not just CSOMRequests[0].");
                }
                finally
                {
                    try
                    {
                        await CsomRequestSender.SendManyAsync(context, new List<IRequest<object>>
                        {
                            new SetValueForUICultureRequest(resource, first.Name, originalFirst),
                            new SetValueForUICultureRequest(resource, second.Name, originalSecond),
                        }).ConfigureAwait(false);

                        Console.WriteLine("Restored the original web titles.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE the web title: {Describe(ex)}");
                    }
                }
            }
        }

        private static async Task<string> ReadAsync(PnPContext context, UserResourcePath resource, CultureInfo culture)
        {
            return await CsomRequestSender.SendAsync(context,
                new GetValueForUICultureRequest(resource, culture.Name)).ConfigureAwait(false);
        }

        #endregion

        #region ObjectLocalization - writing the resource files

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task Localization_ExtractWritesAResourceFilePerLanguage()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (CultureInfo first, CultureInfo second) = await PickTwoSupportedCulturesAsync(context).ConfigureAwait(false);

                string actionName = $"{TestPrefix}ExtractedAction";
                string sourceFolder = CreateResourceFiles(new Dictionary<CultureInfo, string>
                {
                    { first, $"Extracted {first.Name}" },
                    { second, $"Extracted {second.Name}" },
                }, "CustomAction_Title");

                string outputFolder = Path.Combine(Path.GetTempPath(), $"{TestPrefix}{Guid.NewGuid():N}");
                Directory.CreateDirectory(outputFolder);

                try
                {
                    var setup = new ProvisioningTemplate { Connector = new FileSystemConnector(sourceFolder, "") };
                    setup.Localizations.Add(new Localization { LCID = first.LCID, Name = "test", ResourceFile = $"test.{first.Name}.resx" });
                    setup.Localizations.Add(new Localization { LCID = second.LCID, Name = "test", ResourceFile = $"test.{second.Name}.resx" });
                    setup.CustomActions.WebCustomActions.Add(new CustomActionModel
                    {
                        Name = actionName,
                        Title = "{res:CustomAction_Title}",
                        Location = "ClientSideExtension.ApplicationCustomizer",
                        ClientSideComponentId = new Guid("d0454bb0-3b4d-4e6d-9b0e-2e7ff5b6b2ea"),
                        Enabled = true,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(setup).ConfigureAwait(false);

                    var configuration = new ExtractConfiguration
                    {
                        Handlers = { ConfigurationHandler.SupportedUILanguages, ConfigurationHandler.CustomActions },
                        FileConnector = new FileSystemConnector(outputFolder, ""),
                    };
                    configuration.MultiLanguage.PersistResources = true;
                    configuration.MultiLanguage.ResourceFilePrefix = "Extracted";

                    ProvisioningTemplate extracted = await context.GetProvisioningManager()
                        .GetTemplateAsync(configuration).ConfigureAwait(false);

                    Console.WriteLine($"Localizations recorded: {extracted.Localizations.Count}");
                    foreach (Localization localization in extracted.Localizations)
                    {
                        Console.WriteLine($"  {localization.LCID} -> {localization.ResourceFile}");
                    }

                    CustomActionModel extractedAction = extracted.CustomActions.WebCustomActions
                        .FirstOrDefault(a => a.Name == actionName);

                    Assert.IsNotNull(extractedAction, "The custom action was not extracted.");
                    Console.WriteLine($"Extracted title: '{extractedAction.Title}'");

                    StringAssert.StartsWith(extractedAction.Title, "{res:",
                        "A localized custom action should extract as a {res:} token, not as its default-language text.");

                    Assert.IsTrue(extracted.Localizations.Count >= 2,
                        $"Expected a resource file per language; got {extracted.Localizations.Count}.");

                    foreach (Localization localization in extracted.Localizations)
                    {
                        string path = Path.Combine(outputFolder, localization.ResourceFile);
                        Assert.IsTrue(System.IO.File.Exists(path), $"'{localization.ResourceFile}' was recorded but never written.");

                        string content = System.IO.File.ReadAllText(path);
                        Console.WriteLine($"--- {localization.ResourceFile}");
                        Console.WriteLine(content);

                        StringAssert.Contains(content, "CustomAction_", $"'{localization.ResourceFile}' has no entry for the action.");
                        StringAssert.Contains(content, "Extracted ", $"'{localization.ResourceFile}' has no localized value in it.");
                    }
                }
                finally
                {
                    await DeleteActionAsync(actionName).ConfigureAwait(false);
                    TryDeleteFolder(sourceFolder);
                    TryDeleteFolder(outputFolder);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Picks two languages the site actually supports.
        /// </summary>
        private static async Task<(CultureInfo First, CultureInfo Second)> PickTwoSupportedCulturesAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Language, w => w.IsMultilingual, w => w.SupportedUILanguageIds).ConfigureAwait(false);

            List<int> supported = context.Web.SupportedUILanguageIds?.ToList() ?? new List<int>();
            Console.WriteLine($"Web language {context.Web.Language}, multilingual={context.Web.IsMultilingual}, " +
                $"{supported.Count} supported UI language(s)");

            if (!context.Web.IsMultilingual || supported.Count < 2)
            {
                Assert.Inconclusive(
                    "This site is not multilingual, or supports fewer than two UI languages, so per-language values " +
                    "cannot be verified. Enable multilingual pages and add a second language to cover this.");
            }

            int webLanguage = (int)context.Web.Language;
            var first = new CultureInfo(supported.Contains(webLanguage) ? webLanguage : supported[0]);
            var second = new CultureInfo(supported.First(l => l != first.LCID));

            Console.WriteLine($"Using {first.Name} and {second.Name}");

            return (first, second);
        }

        /// <summary>
        /// Writes a resx per culture into a fresh temp folder and returns it.
        /// </summary>
        private static string CreateResourceFiles(Dictionary<CultureInfo, string> valuesByCulture, string key)
        {
            string folder = Path.Combine(Path.GetTempPath(), $"{TestPrefix}{Guid.NewGuid():N}");
            Directory.CreateDirectory(folder);

            foreach (KeyValuePair<CultureInfo, string> pair in valuesByCulture)
            {
                string resx =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                    "<root>" + Environment.NewLine +
                    "  <resheader name=\"resmimetype\"><value>text/microsoft-resx</value></resheader>" + Environment.NewLine +
                    "  <resheader name=\"version\"><value>2.0</value></resheader>" + Environment.NewLine +
                    $"  <data name=\"{key}\" xml:space=\"preserve\"><value>{pair.Value}</value></data>" + Environment.NewLine +
                    "</root>";

                System.IO.File.WriteAllText(Path.Combine(folder, $"test.{pair.Key.Name}.resx"), resx);
            }

            return folder;
        }

        private static async Task<IUserCustomAction> FindActionAsync(PnPContext context, string name)
        {
            await context.Web.LoadAsync(w => w.UserCustomActions).ConfigureAwait(false);

            return context.Web.UserCustomActions.AsRequested()
                .FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.Ordinal));
        }

        private static async Task DeleteActionAsync(string name)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    IUserCustomAction action = await FindActionAsync(context, name).ConfigureAwait(false);
                    if (action != null)
                    {
                        await action.DeleteAsync().ConfigureAwait(false);
                        Console.WriteLine($"Deleted custom action '{name}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE custom action '{name}': {Describe(ex)}");
            }
        }

        private static void TryDeleteFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE '{folder}': {ex.Message}");
            }
        }

        #endregion
    }
}
