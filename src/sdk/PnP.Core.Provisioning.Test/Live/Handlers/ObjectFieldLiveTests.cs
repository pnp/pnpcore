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
using FieldModel = PnP.Core.Provisioning.Model.Field;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectField</c>.
    /// </summary>
    [TestClass]
    public class ObjectFieldLiveTests : LiveTestBase
    {
        private const string TextFieldId = "{1a9dd12b-6e1f-4c2e-9c1a-4d0e29a91001}";
        private const string CalculatedFieldId = "{1a9dd12b-6e1f-4c2e-9c1a-4d0e29a91002}";

        private static string TextFieldName => $"{TestPrefix}Text";

        private static string CalculatedFieldName => $"{TestPrefix}Calculated";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_CreatesASiteColumnFromItsSchemaXml()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema() });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IField created = await FindFieldAsync(fresh, TextFieldName).ConfigureAwait(false);

                        Assert.IsNotNull(created, "The site column was not created.");
                        Console.WriteLine($"Created '{created.InternalName}' ({created.TypeAsString}), title '{created.Title}'");

                        Assert.AreEqual(TextFieldName, created.InternalName,
                            "The internal name was not taken from the schema - is AddFieldInternalNameHint being passed?");
                        Assert.AreEqual("Text", created.TypeAsString);
                    }
                }
                finally
                {
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_UpdatesAnExistingColumnRatherThanFailing()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    var template = new ProvisioningTemplate();
                    template.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema(displayName: "Original") });
                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    var updated = new ProvisioningTemplate();
                    updated.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema(displayName: "Updated") });
                    await manager.ApplyTemplateAsync(updated).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title)).ConfigureAwait(false);

                        List<IField> matching = fresh.Web.Fields.AsRequested()
                            .Where(f => f.InternalName == TextFieldName).ToList();

                        Assert.AreEqual(1, matching.Count, "Re-applying the template duplicated the column.");
                        Assert.AreEqual("Updated", matching[0].Title, "The column was not updated.");

                        Console.WriteLine($"'{matching[0].InternalName}' now titled '{matching[0].Title}'");
                    }
                }
                finally
                {
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_ACalculatedColumnIsHeldBackToTheSecondPass()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.SiteFields.Add(new FieldModel { SchemaXml = CalculatedFieldSchema() });
                    template.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema() });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IField calculated = await FindFieldAsync(fresh, CalculatedFieldName).ConfigureAwait(false);
                        IField text = await FindFieldAsync(fresh, TextFieldName).ConfigureAwait(false);

                        Assert.IsNotNull(text, "The column the calculated one references was not created.");
                        Assert.IsNotNull(calculated,
                            "The calculated column was not created. It references a column listed after it, " +
                            "so this is the three-pass ordering failing.");

                        Console.WriteLine($"Created '{text.InternalName}' and '{calculated.InternalName}' ({calculated.TypeAsString})");
                        Assert.AreEqual("Calculated", calculated.TypeAsString);
                    }
                }
                finally
                {
                    await DeleteFieldAsync(CalculatedFieldName).ConfigureAwait(false);
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_ALookupAtAMissingListWarnsInsteadOfFailingSilently()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string lookupName = $"{TestPrefix}Lookup";

                try
                {
                    var template = new ProvisioningTemplate();
                    template.SiteFields.Add(new FieldModel
                    {
                        SchemaXml = $"<Field ID=\"{{1a9dd12b-6e1f-4c2e-9c1a-4d0e29a91003}}\" Type=\"Lookup\" " +
                                    $"Name=\"{lookupName}\" StaticName=\"{lookupName}\" DisplayName=\"{lookupName}\" " +
                                    $"Group=\"{TestPrefix}Group\" List=\"Lists/{TestPrefix}NoSuchList\" ShowField=\"Title\" />",
                    });

                    var warnings = new List<string>();
                    var configuration = new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                        },
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                    Assert.IsTrue(warnings.Any(w => w.Contains("does not exist on this site", StringComparison.Ordinal)),
                        "An unresolvable lookup target must be reported. " +
                        $"Warnings seen: {string.Join(" | ", warnings)}");

                    Console.WriteLine($"Warning reported: {warnings.First(w => w.Contains("does not exist on this site", StringComparison.Ordinal))}");
                }
                finally
                {
                    await DeleteFieldAsync(lookupName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_AnUnknownTokenIsWrittenLiterally_KnownLimitation()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema(displayName: "{nosuchtoken:whatever}") });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IField created = await FindFieldAsync(fresh, TextFieldName).ConfigureAwait(false);
                        Assert.IsNotNull(created);
                        Assert.AreEqual("{nosuchtoken:whatever}", created.Title,
                            "If this now differs, the token validity check has been widened - update the phase 6 notes.");

                        Console.WriteLine($"Unknown token written literally as the title: '{created.Title}'");
                    }
                }
                finally
                {
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
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
        public async Task Fields_ExtractReadsTheSitesOwnColumnsAndSkipsTheBuiltInOnes()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var setup = new ProvisioningTemplate();
                    setup.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema() });
                    await context.GetProvisioningManager().ApplyTemplateAsync(setup).ConfigureAwait(false);

                    ProvisioningTemplate extracted = await context.GetProvisioningManager()
                        .GetTemplateAsync(new ExtractConfiguration
                        {
                            Handlers = { ConfigurationHandler.Fields },
                        }).ConfigureAwait(false);

                    Console.WriteLine($"Site columns extracted: {extracted.SiteFields.Count}");

                    FieldModel ours = extracted.SiteFields
                        .FirstOrDefault(f => f.SchemaXml.Contains(TextFieldName, StringComparison.Ordinal));

                    Assert.IsNotNull(ours,
                        $"The column just created was not extracted. Columns seen: {extracted.SiteFields.Count}");

                    Console.WriteLine($"Extracted: {ours.SchemaXml}");

                    Assert.IsFalse(extracted.SiteFields.Any(f => f.SchemaXml.Contains("\"Title\"", StringComparison.Ordinal)
                        && f.SchemaXml.Contains("fa564e0f-0c70-4ab9-b863-0177e6ddd247", StringComparison.OrdinalIgnoreCase)),
                        "The built-in Title column was extracted; BuiltInFieldId filtering is not working.");

                    Assert.IsNull(XElement.Parse(ours.SchemaXml).Attribute("Version"),
                        "The Version attribute survived extraction and would be rejected on apply.");
                }
                finally
                {
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Fields_ExtractPutsCalculatedColumnsLastSoTheyReapply()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var setup = new ProvisioningTemplate();
                    setup.SiteFields.Add(new FieldModel { SchemaXml = TextFieldSchema() });
                    setup.SiteFields.Add(new FieldModel { SchemaXml = CalculatedFieldSchema() });
                    await context.GetProvisioningManager().ApplyTemplateAsync(setup).ConfigureAwait(false);

                    ProvisioningTemplate extracted = await context.GetProvisioningManager()
                        .GetTemplateAsync(new ExtractConfiguration
                        {
                            Handlers = { ConfigurationHandler.Fields },
                        }).ConfigureAwait(false);

                    int textIndex = extracted.SiteFields.FindIndex(f => f.SchemaXml.Contains(TextFieldName, StringComparison.Ordinal));
                    int calculatedIndex = extracted.SiteFields.FindIndex(f => f.SchemaXml.Contains(CalculatedFieldName, StringComparison.Ordinal));

                    Console.WriteLine($"text at {textIndex}, calculated at {calculatedIndex} of {extracted.SiteFields.Count}");

                    Assert.IsTrue(textIndex >= 0 && calculatedIndex >= 0, "Both columns should have been extracted.");
                    Assert.IsTrue(calculatedIndex > textIndex,
                        "A calculated column must be extracted after the columns it references, or the template will not re-apply.");

                    FieldModel calculated = extracted.SiteFields[calculatedIndex];

                    string formula = XElement.Parse(calculated.SchemaXml).Descendants("Formula").FirstOrDefault()?.Value;
                    Console.WriteLine($"Formula: {formula}");

                    Assert.IsNotNull(formula, "The calculated column lost its formula.");
                    Assert.IsFalse(formula.Contains("[[", StringComparison.Ordinal),
                        $"The formula has nested brackets and will not re-apply: {formula}");
                    StringAssert.Contains(formula, $"[{TextFieldName}]", "The formula does not reference the column by name.");

                    Assert.IsNull(XElement.Parse(calculated.SchemaXml).Descendants("FieldRefs").FirstOrDefault(),
                        "FieldRefs survived extraction; a stale one makes the calculated column fail to provision.");
                }
                finally
                {
                    await DeleteFieldAsync(CalculatedFieldName).ConfigureAwait(false);
                    await DeleteFieldAsync(TextFieldName).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static string TextFieldSchema(string displayName = null)
        {
            return $"<Field ID=\"{TextFieldId}\" Type=\"Text\" Name=\"{TextFieldName}\" StaticName=\"{TextFieldName}\" " +
                   $"DisplayName=\"{displayName ?? TextFieldName}\" Group=\"{TestPrefix}Group\" MaxLength=\"255\" />";
        }

        private static string CalculatedFieldSchema()
        {
            return $"<Field ID=\"{CalculatedFieldId}\" Type=\"Calculated\" Name=\"{CalculatedFieldName}\" " +
                   $"StaticName=\"{CalculatedFieldName}\" DisplayName=\"{CalculatedFieldName}\" Group=\"{TestPrefix}Group\" " +
                   $"ResultType=\"Text\">" +
                   $"<Formula>=[{TextFieldName}]</Formula>" +
                   $"<FieldRefs><FieldRef Name=\"{TextFieldName}\" /></FieldRefs>" +
                   $"</Field>";
        }

        private static async Task<IField> FindFieldAsync(PnPContext context, string internalName)
        {
            await context.Web.LoadAsync(w => w.Fields.QueryProperties(
                f => f.Id, f => f.InternalName, f => f.Title, f => f.TypeAsString)).ConfigureAwait(false);

            return context.Web.Fields.AsRequested().FirstOrDefault(f => f.InternalName == internalName);
        }

        private static async Task DeleteFieldAsync(string internalName)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    IField field = await FindFieldAsync(context, internalName).ConfigureAwait(false);
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
