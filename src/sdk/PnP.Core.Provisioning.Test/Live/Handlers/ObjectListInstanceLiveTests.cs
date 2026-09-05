using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreList = PnP.Core.Model.SharePoint.IList;
using FieldModel = PnP.Core.Provisioning.Model.Field;
using ViewModel = PnP.Core.Provisioning.Model.View;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectListInstance</c>.
    /// </summary>
    [TestClass]
    public class ObjectListInstanceLiveTests : LiveTestBase
    {
        private static string SourceListTitle => $"{TestPrefix}Source";

        private static string TargetListTitle => $"{TestPrefix}Target";

        private const string LookupFieldId = "{2b8ee23c-7f20-4d3f-ad2b-5e1f30b92001}";
        private const string TextFieldId = "{2b8ee23c-7f20-4d3f-ad2b-5e1f30b92002}";

        #region Creation

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_CreatesAListWithAUrlThatDiffersFromItsTitle()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.Lists.Add(new ListInstance
                    {
                        Title = $"{TestPrefix}Renamed List",
                        Url = $"Lists/{TestPrefix}OriginalUrl",
                        Description = "Created by ObjectListInstanceLiveTests",
                        TemplateType = (int)ListTemplateType.GenericList,
                        OnQuickLaunch = true,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        CoreList created = await FindListAsync(fresh, $"{TestPrefix}Renamed List").ConfigureAwait(false);

                        Assert.IsNotNull(created, "The list was not created.");
                        Console.WriteLine($"'{created.Title}' -> {created.RootFolder.ServerRelativeUrl}");

                        Assert.AreEqual("Created by ObjectListInstanceLiveTests", created.Description);
                        Assert.IsTrue(created.OnQuickLaunch, "The list was not put on the quick launch.");

                        StringAssert.EndsWith(created.RootFolder.ServerRelativeUrl, $"Lists/{TestPrefix}OriginalUrl",
                            "The url the template asked for was not used.");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_UpdatesAnExistingListRatherThanFailing()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(TemplateWith(new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        Description = "First",
                        TemplateType = (int)ListTemplateType.GenericList,
                        EnableVersioning = false,
                    })).ConfigureAwait(false);

                    await manager.ApplyTemplateAsync(TemplateWith(new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        Description = "Second",
                        TemplateType = (int)ListTemplateType.GenericList,
                        EnableVersioning = true,
                        MaxVersionLimit = 42,
                    })).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        List<CoreList> matches = await FindListsByPrefixAsync(fresh).ConfigureAwait(false);

                        Assert.AreEqual(1, matches.Count,
                            $"Expected exactly one list, found {matches.Count}: {string.Join(", ", matches.Select(l => l.Title))}");

                        CoreList updated = matches[0];
                        Console.WriteLine($"'{updated.Title}': {updated.Description}, versioning {updated.EnableVersioning} limit {updated.MaxVersionLimit}");

                        Assert.AreEqual("Second", updated.Description, "The description was not updated.");
                        Assert.IsTrue(updated.EnableVersioning, "Versioning was not turned on.");
                        Assert.AreEqual(42, updated.MaxVersionLimit, "The version limit was not applied.");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region The three-pass ordering

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_ALookupColumnAcrossListsResolves()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();

                    var target = new ListInstance
                    {
                        Title = TargetListTitle,
                        Url = $"Lists/{TestPrefix}Target",
                        TemplateType = (int)ListTemplateType.GenericList,
                    };
                    target.Fields.Add(new FieldModel { SchemaXml = LookupFieldSchema() });
                    template.Lists.Add(target);

                    template.Lists.Add(new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        TemplateType = (int)ListTemplateType.GenericList,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        CoreList source = await FindListAsync(fresh, SourceListTitle).ConfigureAwait(false);
                        CoreList lookupTarget = await FindListAsync(fresh, TargetListTitle).ConfigureAwait(false);

                        Assert.IsNotNull(source, "The source list was not created.");
                        Assert.IsNotNull(lookupTarget, "The list carrying the lookup was not created.");

                        await lookupTarget.LoadAsync(l => l.Fields.QueryProperties(
                            f => f.Id, f => f.InternalName, f => f.TypeAsString, f => f.SchemaXml)).ConfigureAwait(false);

                        IField lookup = lookupTarget.Fields.AsRequested()
                            .FirstOrDefault(f => f.InternalName == $"{TestPrefix}Lookup");

                        Assert.IsNotNull(lookup, "The lookup column was not created on the target list.");
                        Assert.AreEqual("Lookup", lookup.TypeAsString);

                        string listAttribute = (string)XElement.Parse(lookup.SchemaXml).Attribute("List");
                        Console.WriteLine($"Lookup List attribute: {listAttribute}");

                        Assert.IsNotNull(listAttribute, "The lookup has no List attribute at all.");
                        Assert.AreEqual(source.Id, Guid.Parse(listAttribute.Trim('{', '}')),
                            "The lookup does not point at the source list - did the {listid:} token resolve?");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Contents

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_CreatesColumnsViewsAndFolders()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var list = new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        TemplateType = (int)ListTemplateType.GenericList,
                        EnableFolderCreation = true,
                    };

                    list.Fields.Add(new FieldModel { SchemaXml = TextFieldSchema() });
                    list.Views.Add(new ViewModel { SchemaXml = ViewSchema() });
                    list.Folders.Add(new Model.Folder($"{TestPrefix}Parent")
                    {
                        Folders = { new Model.Folder($"{TestPrefix}Child") },
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(TemplateWith(list), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        CoreList created = await FindListAsync(fresh, SourceListTitle).ConfigureAwait(false);
                        Assert.IsNotNull(created, "The list was not created.");

                        await created.LoadAsync(
                            l => l.Fields.QueryProperties(f => f.InternalName, f => f.Title),
                            l => l.Views.QueryProperties(v => v.Title, v => v.RowLimit),
                            l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl, f => f.Folders)).ConfigureAwait(false);

                        IField column = created.Fields.AsRequested().FirstOrDefault(f => f.InternalName == $"{TestPrefix}Text");
                        Assert.IsNotNull(column, "The list column was not created.");

                        IView view = created.Views.AsRequested().FirstOrDefault(v => v.Title == $"{TestPrefix}View");
                        Assert.IsNotNull(view,
                            $"The view was not created. Views found: {string.Join(", ", created.Views.AsRequested().Select(v => v.Title))}");
                        Assert.AreEqual(7, view.RowLimit, "The view's row limit came from somewhere other than the schema.");

                        string listUrl = created.RootFolder.ServerRelativeUrl;

                        Assert.IsNotNull(await FindFolderAsync(fresh, $"{listUrl}/{TestPrefix}Parent").ConfigureAwait(false),
                            "The folder was not created.");

                        Assert.IsNotNull(await FindFolderAsync(fresh, $"{listUrl}/{TestPrefix}Parent/{TestPrefix}Child").ConfigureAwait(false),
                            $"The nested folder was not created. Under the list root: {await DescribeFoldersAsync(fresh, listUrl).ConfigureAwait(false)}");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_BindsAContentTypeAndMakesItTheDefault()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var list = new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        TemplateType = (int)ListTemplateType.GenericList,
                        ContentTypesEnabled = true,
                    };

                    list.ContentTypeBindings.Add(new ContentTypeBinding { ContentTypeId = "0x0104", Default = true });
                    list.ContentTypeBindings.Add(new ContentTypeBinding { ContentTypeId = "0x01" });

                    await context.GetProvisioningManager().ApplyTemplateAsync(TemplateWith(list), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        CoreList created = await FindListAsync(fresh, SourceListTitle).ConfigureAwait(false);
                        Assert.IsNotNull(created, "The list was not created.");

                        Assert.IsTrue(created.ContentTypesEnabled, "Content types were not turned on.");

                        List<string> order = await created.GetContentTypeOrderAsync().ConfigureAwait(false);
                        Console.WriteLine($"Content type order: {string.Join(", ", order)}");

                        Assert.IsTrue(order.Count > 0, "The list has no unique content type order.");

                        StringAssert.StartsWith(order[0], "0x0104",
                            "Announcement is not first in the order, so it is not the default content type.");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Extract

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_ExtractsAListItJustCreated()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var list = new ListInstance
                    {
                        Title = SourceListTitle,
                        Url = $"Lists/{TestPrefix}Source",
                        Description = "Round trip",
                        TemplateType = (int)ListTemplateType.GenericList,
                        EnableVersioning = true,
                        MaxVersionLimit = 13,
                        OnQuickLaunch = true,
                    };
                    list.Fields.Add(new FieldModel { SchemaXml = TextFieldSchema() });

                    await context.GetProvisioningManager().ApplyTemplateAsync(TemplateWith(list), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        var configuration = new ExtractConfiguration();
                        configuration.Handlers.Add(ConfigurationHandler.Lists);
                        configuration.Lists.Lists.Add(new Model.Configuration.Lists.Lists.ExtractListsListsConfiguration
                        {
                            Title = SourceListTitle,
                        });

                        ProvisioningTemplate extracted;
                        try
                        {
                            extracted = await fresh.GetProvisioningManager()
                                .GetTemplateAsync(configuration).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(Describe(ex));
                            throw;
                        }

                        Console.WriteLine($"Extracted {extracted.Lists.Count} list(s): {string.Join(", ", extracted.Lists.Select(l => $"{l.Title} ({l.Url})"))}");

                        ListInstance roundTripped = extracted.Lists.FirstOrDefault(l => l.Title == SourceListTitle);
                        Assert.IsNotNull(roundTripped, "The list was not extracted.");

                        Assert.AreEqual($"Lists/{TestPrefix}Source", roundTripped.Url, "The url was not read back.");
                        Assert.AreEqual("Round trip", roundTripped.Description);
                        Assert.AreEqual((int)ListTemplateType.GenericList, roundTripped.TemplateType);
                        Assert.IsTrue(roundTripped.EnableVersioning, "Versioning was not read back.");
                        Assert.AreEqual(13, roundTripped.MaxVersionLimit, "The version limit was not read back.");
                        Assert.IsTrue(roundTripped.OnQuickLaunch, "The quick launch flag was not read back.");

                        bool hasColumn = roundTripped.Fields.Any(f =>
                            f.SchemaXml.IndexOf($"{TestPrefix}Text", StringComparison.Ordinal) >= 0);

                        Assert.IsTrue(hasColumn,
                            $"The list column was not extracted. Fields: {roundTripped.Fields.Count}, FieldRefs: {roundTripped.FieldRefs.Count}");

                        Assert.IsTrue(roundTripped.Views.Count > 0, "No views were extracted.");
                    }
                }
                finally
                {
                    await SweepListsAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static ProvisioningTemplate TemplateWith(ListInstance list)
        {
            var template = new ProvisioningTemplate();
            template.Lists.Add(list);
            return template;
        }

        /// <summary>
        /// An apply configuration that prints the handler's warnings and errors.
        /// </summary>
        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        private static string TextFieldSchema()
        {
            return $"<Field ID=\"{TextFieldId}\" Type=\"Text\" Name=\"{TestPrefix}Text\" " +
                $"StaticName=\"{TestPrefix}Text\" DisplayName=\"Suite Text\" Group=\"{TestPrefix}Group\" />";
        }

        private static string LookupFieldSchema()
        {
            return $"<Field ID=\"{LookupFieldId}\" Type=\"Lookup\" Name=\"{TestPrefix}Lookup\" " +
                $"StaticName=\"{TestPrefix}Lookup\" DisplayName=\"Suite Lookup\" Group=\"{TestPrefix}Group\" " +
                $"List=\"{{listid:{SourceListTitle}}}\" ShowField=\"Title\" />";
        }

        private static string ViewSchema()
        {
            return $"<View DisplayName=\"{TestPrefix}View\" Type=\"HTML\" Scope=\"Recursive\">" +
                "<ViewFields><FieldRef Name=\"Title\" /></ViewFields>" +
                "<Query><OrderBy><FieldRef Name=\"Title\" /></OrderBy></Query>" +
                "<RowLimit Paged=\"TRUE\">7</RowLimit>" +
                "</View>";
        }

        private static async Task<IFolder> FindFolderAsync(PnPContext context, string serverRelativeUrl)
        {
            try
            {
                return await context.Web.GetFolderByServerRelativeUrlAsync(serverRelativeUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<string> DescribeFoldersAsync(PnPContext context, string serverRelativeUrl)
        {
            try
            {
                IFolder root = await context.Web.GetFolderByServerRelativeUrlAsync(serverRelativeUrl,
                    f => f.Folders.QueryProperties(sub => sub.Name)).ConfigureAwait(false);

                return string.Join(", ", root.Folders.AsRequested().Select(f => f.Name));
            }
            catch (Exception ex)
            {
                return Describe(ex);
            }
        }

        private static async Task<CoreList> FindListAsync(PnPContext context, string title)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Description,
                l => l.OnQuickLaunch, l => l.EnableVersioning, l => l.MaxVersionLimit, l => l.ContentTypesEnabled,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            return context.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == title);
        }

        private static async Task<List<CoreList>> FindListsByPrefixAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Description,
                l => l.EnableVersioning, l => l.MaxVersionLimit,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            return context.Web.Lists.AsRequested()
                .Where(l => l.Title != null && l.Title.StartsWith(TestPrefix, StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// Deletes every list this suite created, and the site columns its templates may have left.
        /// </summary>
        private static async Task SweepListsAsync()
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

                    foreach (CoreList list in context.Web.Lists.AsRequested()
                        .Where(l => l.Title != null && l.Title.StartsWith(TestPrefix, StringComparison.Ordinal))
                        .ToList())
                    {
                        string name = list.Title;
                        try
                        {
                            await list.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted list '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE list '{name}': {Describe(ex)}");
                        }
                    }

                    await context.Web.LoadAsync(w => w.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Group))
                        .ConfigureAwait(false);

                    foreach (IField field in context.Web.Fields.AsRequested()
                        .Where(f => f.Group == $"{TestPrefix}Group").ToList())
                    {
                        try
                        {
                            await field.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted site column '{field.InternalName}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE column '{field.InternalName}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP: {Describe(ex)}");
            }
        }

        #endregion
    }
}
