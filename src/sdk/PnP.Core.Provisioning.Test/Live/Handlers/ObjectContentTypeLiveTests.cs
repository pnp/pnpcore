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
using ContentTypeModel = PnP.Core.Provisioning.Model.ContentType;
using FieldModel = PnP.Core.Provisioning.Model.Field;
using FieldRefModel = PnP.Core.Provisioning.Model.FieldRef;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    [TestClass]
    public class ObjectContentTypeLiveTests : LiveTestBase
    {
        private const string ContentTypeId = "0x0100A9C1B2D34E5F6071829304A5B6C7D8E9";
        private const string DocumentSetContentTypeId = "0x0120D520009A1B2C3D4E5F60718293A4B5C6D7E8F9";

        private const string FirstFieldId = "{2b9dd12b-6e1f-4c2e-9c1a-4d0e29a92001}";
        private const string SecondFieldId = "{2b9dd12b-6e1f-4c2e-9c1a-4d0e29a92002}";

        private static string ContentTypeName => $"{TestPrefix}CT";

        private static string DocumentSetName => $"{TestPrefix}DocSet";

        private static string FirstFieldName => $"{TestPrefix}First";

        private static string SecondFieldName => $"{TestPrefix}Second";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ContentTypes_CreatesAContentTypeWithItsColumns()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await ApplyAsync(context, BuildTemplate()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IContentType created = await FindContentTypeAsync(fresh, ContentTypeId).ConfigureAwait(false);

                        Assert.IsNotNull(created, "The content type was not created.");
                        Console.WriteLine($"Created '{created.Name}' ({created.StringId}) in group '{created.Group}'");

                        Assert.AreEqual(ContentTypeName, created.Name);
                        Assert.AreEqual($"{TestPrefix}Group", created.Group);

                        List<string> linkNames = created.FieldLinks.AsRequested().Select(fl => fl.Name).ToList();
                        Console.WriteLine($"Columns: {string.Join(", ", linkNames)}");

                        Assert.IsTrue(linkNames.Contains(FirstFieldName), "The first column was not linked.");
                        Assert.IsTrue(linkNames.Contains(SecondFieldName), "The second column was not linked.");
                    }
                }
                finally
                {
                    await CleanUpAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ContentTypes_PutsTheColumnsInTheTemplatesOrder()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    ProvisioningTemplate template = BuildTemplate(SecondFieldName, FirstFieldName);

                    await ApplyAsync(context, template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IContentType created = await FindContentTypeAsync(fresh, ContentTypeId).ConfigureAwait(false);
                        Assert.IsNotNull(created);

                        List<string> linkNames = created.FieldLinks.AsRequested().Select(fl => fl.Name).ToList();
                        Console.WriteLine($"Columns in order: {string.Join(", ", linkNames)}");

                        int secondIndex = linkNames.IndexOf(SecondFieldName);
                        int firstIndex = linkNames.IndexOf(FirstFieldName);

                        Assert.IsTrue(secondIndex >= 0 && firstIndex >= 0, "Both columns should be linked.");
                        Assert.IsTrue(secondIndex < firstIndex,
                            $"The template asked for '{SecondFieldName}' before '{FirstFieldName}'. " +
                            $"Got: {string.Join(", ", linkNames)}. PnP Core has no FieldLinks.Reorder, so this is " +
                            "ReorderFieldLinksRequest failing.");
                    }
                }
                finally
                {
                    await CleanUpAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ContentTypes_UpdatesAnExistingContentTypeRatherThanDuplicatingIt()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await ApplyAsync(context, BuildTemplate()).ConfigureAwait(false);

                    ProvisioningTemplate updated = BuildTemplate();
                    updated.ContentTypes[0].Description = "Updated description";
                    await ApplyAsync(context, updated).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.ContentTypes.QueryProperties(
                            ct => ct.StringId, ct => ct.Name, ct => ct.Description)).ConfigureAwait(false);

                        List<IContentType> matching = fresh.Web.ContentTypes.AsRequested()
                            .Where(ct => ct.StringId == ContentTypeId).ToList();

                        Assert.AreEqual(1, matching.Count, "Re-applying the template duplicated the content type.");
                        Assert.AreEqual("Updated description", matching[0].Description, "The content type was not updated.");

                        Console.WriteLine($"'{matching[0].Name}' now described as '{matching[0].Description}'");
                    }
                }
                finally
                {
                    await CleanUpAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Pins the PnP Core limitation that <c>UpdateContentTypeRequest</c> exists to work around.
        /// </summary>
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ContentTypes_PnPCoreCannotUpdateAContentTypeReadFromTheCollection()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                bool createdObjectUpdateWorked = false;
                bool readBackUpdateWorked = false;

                try
                {
                    IContentType created = await context.Web.ContentTypes
                        .AddAsync(ContentTypeId, ContentTypeName, "Original", $"{TestPrefix}Group").ConfigureAwait(false);

                    Console.WriteLine($"Created '{created.Name}'");

                    try
                    {
                        created.Description = "Updated via the created object";
                        await created.UpdateAsync().ConfigureAwait(false);
                        createdObjectUpdateWorked = true;
                        Console.WriteLine("(a) update via the created object: OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"(a) update via the created object: FAILED{Environment.NewLine}{Describe(ex)}");
                    }

                    try
                    {
                        using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                        {
                            await fresh.Web.LoadAsync(w => w.ContentTypes.QueryProperties(
                                ct => ct.StringId, ct => ct.Name, ct => ct.Description)).ConfigureAwait(false);

                            IContentType readBack = fresh.Web.ContentTypes.AsRequested()
                                .First(ct => ct.StringId == ContentTypeId);

                            readBack.Description = "Updated via a plain read back";
                            await readBack.UpdateAsync().ConfigureAwait(false);
                            readBackUpdateWorked = true;
                            Console.WriteLine("(b) update via a plain read back: OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"(b) update via a plain read back: FAILED{Environment.NewLine}{Describe(ex)}");
                    }

                    Assert.IsTrue(createdObjectUpdateWorked,
                        "Updating the object AddAsync returned should work - if this broke, PnP Core changed.");

                    Assert.IsFalse(readBackUpdateWorked,
                        "PnP Core can now update a content type read from the collection. " +
                        "UpdateContentTypeRequest exists only because it could not - revisit whether the CSOM " +
                        "request is still needed for the property half (UpdateChildren still needs it).");
                }
                finally
                {
                    await DeleteContentTypeAsync(ContentTypeId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task ContentTypes_Diagnose_IsTheDocumentSetsFeatureOn()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                await context.Web.LoadAsync(w => w.ContentTypes.QueryProperties(ct => ct.StringId, ct => ct.Name)).ConfigureAwait(false);

                IContentType documentSetParent = context.Web.ContentTypes.AsRequested()
                    .FirstOrDefault(ct => ct.StringId == "0x0120D520");

                Console.WriteLine($"Document Set parent content type present: {documentSetParent != null}");

                foreach (IContentType ct in context.Web.ContentTypes.AsRequested()
                    .Where(ct => ct.StringId.StartsWith("0x0120", StringComparison.Ordinal)))
                {
                    Console.WriteLine($"  {ct.StringId} - {ct.Name}");
                }

                await context.Site.LoadAsync(s => s.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                var documentSetsFeature = new Guid("3bae86a2-776d-499d-9db8-fa4cdc7884f8");
                bool active = context.Site.Features.AsRequested().Any(f => f.DefinitionId == documentSetsFeature);

                Console.WriteLine($"Document Sets site feature ({documentSetsFeature}) active: {active}");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task ContentTypes_Diagnose_ReapplyOnAFreshContext()
        {
            try
            {
                using (PnPContext first = await GetContextAsync().ConfigureAwait(false))
                {
                    await ApplyAsync(first, BuildTemplate()).ConfigureAwait(false);
                }

                using (PnPContext second = await GetContextAsync(1).ConfigureAwait(false))
                {
                    ProvisioningTemplate updated = BuildTemplate();
                    updated.ContentTypes[0].Description = "Updated description";
                    await ApplyAsync(second, updated).ConfigureAwait(false);
                }

                using (PnPContext fresh = await GetContextAsync(2).ConfigureAwait(false))
                {
                    IContentType contentType = await FindContentTypeAsync(fresh, ContentTypeId).ConfigureAwait(false);
                    Console.WriteLine($"Description after a fresh-context re-apply: '{contentType?.Description}'");
                }

                Console.WriteLine("A fresh context re-applies cleanly - the same-context failure is stale client state.");
            }
            finally
            {
                await CleanUpAsync().ConfigureAwait(false);
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ContentTypes_CreatesADocumentSetWithItsSharedColumns()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    ProvisioningTemplate template = BuildTemplate();
                    var documentSet = new ContentTypeModel
                    {
                        Id = DocumentSetContentTypeId,
                        Name = DocumentSetName,
                        Description = "Document set created by a live test",
                        Group = $"{TestPrefix}Group",
                        DocumentSetTemplate = new DocumentSetTemplate(
                            welcomePage: null,
                            allowedContentTypes: null,
                            defaultDocuments: null,
                            sharedFields: new[] { Guid.Parse(FirstFieldId) },
                            welcomePageFields: new[] { Guid.Parse(SecondFieldId) }),
                    };

                    template.ContentTypes.Add(documentSet);

                    var warnings = new List<string>();
                    var configuration = new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                        },
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                    foreach (string warning in warnings)
                    {
                        Console.WriteLine($"warning: {warning}");
                    }

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IContentType created = await FindContentTypeAsync(fresh, DocumentSetContentTypeId).ConfigureAwait(false);

                        Assert.IsNotNull(created, "The document set content type was not created.");
                        Console.WriteLine($"Created document set '{created.Name}' ({created.StringId})");

                        IDocumentSet asDocumentSet = await created.AsDocumentSetAsync().ConfigureAwait(false);
                        Assert.IsNotNull(asDocumentSet, "The content type was created but is not a document set.");

                        List<string> shared = asDocumentSet.SharedColumns?.Select(f => f.InternalName).ToList() ?? new List<string>();
                        Console.WriteLine($"Shared columns: {string.Join(", ", shared)}");

                        Assert.IsTrue(shared.Contains(FirstFieldName),
                            $"The shared column was not applied. Shared columns seen: {string.Join(", ", shared)}");
                    }
                }
                finally
                {
                    await CleanUpAsync().ConfigureAwait(false);
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
        public async Task ContentTypes_ExtractReadsTheSitesOwnContentTypesAndSkipsTheBuiltInOnes()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await ApplyAsync(context, BuildTemplate()).ConfigureAwait(false);

                    ProvisioningTemplate extracted = await context.GetProvisioningManager()
                        .GetTemplateAsync(new ExtractConfiguration
                        {
                            Handlers = { ConfigurationHandler.ContentTypes },
                        }).ConfigureAwait(false);

                    Console.WriteLine($"Content types extracted: {extracted.ContentTypes.Count}");

                    ContentTypeModel ours = extracted.ContentTypes
                        .FirstOrDefault(ct => ct.Id.Equals(ContentTypeId, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(ours,
                        $"The content type just created was not extracted. Ids seen: {string.Join(", ", extracted.ContentTypes.Select(c => c.Id))}");

                    Console.WriteLine($"Extracted '{ours.Name}' with {ours.FieldRefs.Count} column ref(s)");

                    Assert.AreEqual(ContentTypeName, ours.Name);
                    Assert.IsTrue(ours.FieldRefs.Any(fr => fr.Name == FirstFieldName),
                        "The column references did not survive extraction.");

                    Assert.IsFalse(extracted.ContentTypes.Any(ct => ct.Id == "0x01"),
                        "The built-in Item content type was extracted; BuiltInContentTypeId filtering is not working.");
                }
                finally
                {
                    await CleanUpAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// A template with two site columns and a content type that links them.
        /// </summary>
        private static ProvisioningTemplate BuildTemplate(params string[] fieldOrder)
        {
            var template = new ProvisioningTemplate();

            template.SiteFields.Add(new FieldModel { SchemaXml = FieldSchema(FirstFieldId, FirstFieldName) });
            template.SiteFields.Add(new FieldModel { SchemaXml = FieldSchema(SecondFieldId, SecondFieldName) });

            var contentType = new ContentTypeModel
            {
                Id = ContentTypeId,
                Name = ContentTypeName,
                Description = "Created by a live test",
                Group = $"{TestPrefix}Group",
            };

            string[] order = fieldOrder.Length > 0 ? fieldOrder : new[] { FirstFieldName, SecondFieldName };

            foreach (string name in order)
            {
                contentType.FieldRefs.Add(new FieldRefModel(name)
                {
                    Id = Guid.Parse(name == FirstFieldName ? FirstFieldId : SecondFieldId),
                });
            }

            template.ContentTypes.Add(contentType);

            return template;
        }

        private static string FieldSchema(string id, string name)
        {
            return $"<Field ID=\"{id}\" Type=\"Text\" Name=\"{name}\" StaticName=\"{name}\" " +
                   $"DisplayName=\"{name}\" Group=\"{TestPrefix}Group\" MaxLength=\"255\" />";
        }

        /// <summary>
        /// Applies a template and prints what SharePoint said if it fails.
        /// </summary>
        private static async Task ApplyAsync(PnPContext context, ProvisioningTemplate template)
        {
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
        }

        private static async Task<IContentType> FindContentTypeAsync(PnPContext context, string stringId)
        {
            await context.Web.LoadAsync(w => w.ContentTypes.QueryProperties(
                ct => ct.StringId, ct => ct.Name, ct => ct.Description, ct => ct.Group,
                ct => ct.FieldLinks.QueryProperties(fl => fl.Id, fl => fl.Name))).ConfigureAwait(false);

            return context.Web.ContentTypes.AsRequested()
                .FirstOrDefault(ct => string.Equals(ct.StringId, stringId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Removes the content type first, then its columns.
        /// </summary>
        private static async Task CleanUpAsync()
        {
            await DeleteContentTypesByPrefixAsync().ConfigureAwait(false);

            await DeleteFieldAsync(FirstFieldName).ConfigureAwait(false);
            await DeleteFieldAsync(SecondFieldName).ConfigureAwait(false);
        }

        private static async Task DeleteContentTypesByPrefixAsync()
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.ContentTypes.QueryProperties(
                        ct => ct.StringId, ct => ct.Name)).ConfigureAwait(false);

                    List<IContentType> ours = context.Web.ContentTypes.AsRequested()
                        .Where(ct => ct.Name != null && ct.Name.StartsWith(TestPrefix, StringComparison.Ordinal))
                        .ToList();

                    foreach (IContentType contentType in ours)
                    {
                        try
                        {
                            string name = contentType.Name;
                            await contentType.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted content type '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE content type '{contentType.Name}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP content types: {Describe(ex)}");
            }
        }

        private static async Task DeleteContentTypeAsync(string stringId)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    IContentType contentType = await FindContentTypeAsync(context, stringId).ConfigureAwait(false);
                    if (contentType != null)
                    {
                        await contentType.DeleteAsync().ConfigureAwait(false);
                        Console.WriteLine($"Deleted content type '{stringId}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE content type '{stringId}': {Describe(ex)}");
            }
        }

        private static async Task DeleteFieldAsync(string internalName)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.Fields.QueryProperties(f => f.Id, f => f.InternalName)).ConfigureAwait(false);

                    IField field = context.Web.Fields.AsRequested().FirstOrDefault(f => f.InternalName == internalName);
                    if (field != null)
                    {
                        await field.DeleteAsync().ConfigureAwait(false);
                        Console.WriteLine($"Deleted site column '{internalName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE site column '{internalName}': {Describe(ex)}");
            }
        }

        #endregion
    }
}
