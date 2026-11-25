using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Teams;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using File = System.IO.File;
using Path = System.IO.Path;

namespace PnP.Core.Provisioning.Test.Offline.Providers
{
    /// <summary>
    /// The migration phase 2 exit gate: every supported provisioning schema version must deserialize
    /// into the domain model, serialize back out, and survive the trip intact.
    /// </summary>
    [TestClass]
    public class SchemaRoundTripTests
    {
        /// <summary>
        /// Every schema version the engine supports, with the sample template that exercises it.
        /// </summary>
        public static IEnumerable<object[]> SchemaVersions()
        {
            yield return new object[] { "ProvisioningSchema-2019-03-FullSample-01.xml", XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_03 };
            yield return new object[] { "ProvisioningSchema-2019-09-FullSample-01.xml", XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_09 };
            yield return new object[] { "ProvisioningSchema-2020-02-FullSample-01.xml", XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2020_02 };
            yield return new object[] { "ProvisioningSchema-2021-03-FullSample-01.xml", XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2021_03 };
            yield return new object[] { "ProvisioningSchema-2022-09-FullSample-01.xml", XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09 };
        }

        private static string TemplatesFolder => Path.Combine(AppContext.BaseDirectory, "TestAssets", "Templates");

        private static Stream OpenFixture(string fileName)
        {
            string path = Path.Combine(TemplatesFolder, fileName);
            Assert.IsTrue(File.Exists(path), $"Fixture not found: {path}");
            return File.OpenRead(path);
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void Formatter_IsResolvedFromTheTemplateNamespace(string fileName, string namespaceUri)
        {
            _ = fileName;

            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            Assert.IsNotNull(formatter);
            Assert.AreEqual(namespaceUri, ((IXMLSchemaFormatter)formatter).NamespaceUri,
                "The formatter selected for a namespace must serialize into that same namespace.");
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void Deserialize_ProducesAPopulatedTemplate(string fileName, string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            ProvisioningTemplate template;
            using (Stream fixture = OpenFixture(fileName))
            {
                template = formatter.ToProvisioningTemplate(fixture);
            }

            Assert.IsNotNull(template, "Deserialization returned no template");

            Assert.IsFalse(string.IsNullOrEmpty(template.Id), "Template Id was not deserialized");
            Assert.IsTrue(template.SiteFields.Count > 0, "Site fields were not deserialized");
            Assert.IsTrue(template.ContentTypes.Count > 0, "Content types were not deserialized");
            Assert.IsTrue(template.Lists.Count > 0, "Lists were not deserialized");
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void Serialize_ProducesXmlInTheExpectedNamespace(string fileName, string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            ProvisioningTemplate template;
            using (Stream fixture = OpenFixture(fileName))
            {
                template = formatter.ToProvisioningTemplate(fixture);
            }

            using (Stream serialized = formatter.ToFormattedTemplate(template))
            {
                XDocument document = XDocument.Load(serialized);

                Assert.IsNotNull(document.Root);
                Assert.AreEqual(namespaceUri, document.Root.Name.NamespaceName,
                    "Serialized output landed in the wrong schema namespace.");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void Serialize_OutputValidatesAgainstItsSchema(string fileName, string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            ProvisioningTemplate template;
            using (Stream fixture = OpenFixture(fileName))
            {
                template = formatter.ToProvisioningTemplate(fixture);
            }

            using (Stream serialized = formatter.ToFormattedTemplate(template))
            {
                ValidationResult validation = ((ITemplateFormatterWithValidation)formatter).GetValidationResults(serialized);

                string failures = validation.Exceptions == null
                    ? string.Empty
                    : string.Join(Environment.NewLine, validation.Exceptions.Select(e => e.Message));

                Assert.IsTrue(validation.IsValid, $"Serialized template did not validate against its schema:{Environment.NewLine}{failures}");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void RoundTrip_ModelSurvivesSerializeAndDeserialize(string fileName, string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            ProvisioningTemplate original;
            using (Stream fixture = OpenFixture(fileName))
            {
                original = formatter.ToProvisioningTemplate(fixture);
            }

            ProvisioningTemplate roundTripped;
            using (Stream serialized = formatter.ToFormattedTemplate(original))
            {
                roundTripped = formatter.ToProvisioningTemplate(serialized);
            }

            Assert.IsNotNull(roundTripped);

            Assert.AreEqual(original.Id, roundTripped.Id, nameof(original.Id));
            Assert.AreEqual(original.SiteFields.Count, roundTripped.SiteFields.Count, nameof(original.SiteFields));
            Assert.AreEqual(original.ContentTypes.Count, roundTripped.ContentTypes.Count, nameof(original.ContentTypes));
            Assert.AreEqual(original.Lists.Count, roundTripped.Lists.Count, nameof(original.Lists));
            Assert.AreEqual(original.Features?.SiteFeatures?.Count, roundTripped.Features?.SiteFeatures?.Count, "SiteFeatures");
            Assert.AreEqual(original.Features?.WebFeatures?.Count, roundTripped.Features?.WebFeatures?.Count, "WebFeatures");
            Assert.AreEqual(original.CustomActions?.SiteCustomActions?.Count, roundTripped.CustomActions?.SiteCustomActions?.Count, "SiteCustomActions");
            Assert.AreEqual(original.CustomActions?.WebCustomActions?.Count, roundTripped.CustomActions?.WebCustomActions?.Count, "WebCustomActions");
            Assert.AreEqual(original.PropertyBagEntries.Count, roundTripped.PropertyBagEntries.Count, nameof(original.PropertyBagEntries));
            Assert.AreEqual(original.Files.Count, roundTripped.Files.Count, nameof(original.Files));
            Assert.AreEqual(original.TermGroups.Count, roundTripped.TermGroups.Count, nameof(original.TermGroups));
            Assert.AreEqual(original.Security?.SiteGroups?.Count, roundTripped.Security?.SiteGroups?.Count, "SiteGroups");
            Assert.AreEqual(original.Security?.SiteSecurityPermissions?.RoleDefinitions?.Count,
                            roundTripped.Security?.SiteSecurityPermissions?.RoleDefinitions?.Count, "RoleDefinitions");
        }

        // Parameter values, theme palettes and team templates are stored as XML text. Starting with .NET 11, XmlSerializer
        // splits an [XmlText] string[] member on whitespace, so the schema classes keep that text as a plain string
        // (see Providers\Xml\readme.md). These tests also fail if a regenerated schema file brings the string[] back.

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void RoundTrip_ParametersAndThemesKeepTheirWhitespace(string fileName, string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            List<string> expectedPalettes;
            using (Stream fixture = OpenFixture(fileName))
            {
                expectedPalettes = XDocument.Load(fixture).Descendants()
                    .Where(e => e.Name.LocalName == "Theme" && e.Parent?.Name.LocalName == "Themes")
                    .Select(e => e.Value.Trim())
                    .ToList();
            }

            Assert.IsTrue(expectedPalettes.Count > 0, "The fixture has no tenant themes to check.");

            ProvisioningTemplate original;
            using (Stream fixture = OpenFixture(fileName))
            {
                original = formatter.ToProvisioningTemplate(fixture);
            }

            ProvisioningTemplate roundTripped;
            using (Stream serialized = formatter.ToFormattedTemplate(original))
            {
                roundTripped = formatter.ToProvisioningTemplate(serialized);
            }

            foreach (var (stage, template) in new[] { ("read", original), ("round trip", roundTripped) })
            {
                Assert.AreEqual("Contoso Inc.", template.Parameters["CompanyName"], $"The parameter value changed ({stage}).");
                CollectionAssert.AreEqual(expectedPalettes, template.Tenant.Themes.Select(t => t.Palette?.Trim()).ToList(),
                    $"The theme palettes changed ({stage}).");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void RoundTrip_HierarchyKeepsWhitespaceInParametersThemesAndTeamTemplates()
        {
            const string palette = "{\n  \"themePrimary\": \"#ecb534\",\n  \"name\": \"My orange theme\"\n}";
            const string teamTemplate = "{ \"displayName\": \"My team\", \"description\": \"Two  spaces\" }";
            var parameters = new Dictionary<string, string>
            {
                ["Spaced"] = "a  b   c",
                ["Edges"] = " leading and trailing ",
                ["MultiLine"] = "line 1\nline 2",
            };

            var hierarchy = new ProvisioningHierarchy();
            foreach (var parameter in parameters)
            {
                hierarchy.Parameters[parameter.Key] = parameter.Value;
            }
            hierarchy.Tenant.Themes.Add(new Theme { Name = "Orange", Palette = palette });
            hierarchy.Teams.TeamTemplates.Add(new TeamTemplate { DisplayName = "My team", Description = "A team template", JsonTemplate = teamTemplate });
            hierarchy.Templates.Add(new ProvisioningTemplate { Id = "TEMPLATE-1" });

            var formatter = (IProvisioningHierarchyFormatter)XMLPnPSchemaFormatter.LatestFormatter;

            ProvisioningHierarchy read;
            using (Stream saved = formatter.ToFormattedHierarchy(hierarchy))
            {
                read = formatter.ToProvisioningHierarchy(saved);
            }

            CollectionAssert.AreEquivalent(parameters.ToList(), read.Parameters.ToList(), "The parameter values changed.");
            Assert.AreEqual(palette, read.Tenant.Themes.Single().Palette?.Trim(), "The theme palette changed.");
            Assert.AreEqual(teamTemplate, read.Teams.TeamTemplates.Single().JsonTemplate, "The team template changed.");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ToXml_UsesTheLatestSchemaByDefault()
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09);

            ProvisioningTemplate template;
            using (Stream fixture = OpenFixture("ProvisioningSchema-2022-09-FullSample-01.xml"))
            {
                template = formatter.ToProvisioningTemplate(fixture);
            }

            string xml = template.ToXML();

            Assert.IsFalse(string.IsNullOrEmpty(xml));
            Assert.IsTrue(xml.Contains(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09),
                "ToXML() should emit the latest schema namespace.");
        }
    }
}
