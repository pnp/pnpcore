using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using PnP.Core.Provisioning.Providers.Xml.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;

using File = System.IO.File;
using Path = System.IO.Path;

namespace PnP.Core.Provisioning.Test.Offline.Providers
{
    [TestClass]
    public class FlexibleLayoutSectionSchemaTests
    {
        private const string FlexibleControlData = "{\"position\":{\"layoutIndex\":1,\"zoneIndex\":2,\"sectionIndex\":1,\"sectionFactor\":100,\"controlIndex\":1},\"controlType\":4,\"flexibleLayoutPosition\":{\"lg\":{\"x\":8,\"y\":7,\"w\":18,\"h\":11}}}";
        private const string VerticalColumnControlData = "{\"position\":{\"layoutIndex\":2,\"zoneIndex\":3,\"sectionIndex\":1,\"sectionFactor\":12,\"controlIndex\":1},\"controlType\":4}";
        private const string OneColumnControlData = "{\"position\":{\"layoutIndex\":1,\"zoneIndex\":1,\"sectionIndex\":1,\"sectionFactor\":12,\"controlIndex\":1},\"controlType\":4}";

        private static readonly CanvasSectionType[] SectionTypes =
        {
            CanvasSectionType.FlexibleLayoutSection,
            CanvasSectionType.FlexibleLayoutVerticalSection,
            CanvasSectionType.OneColumn,
        };

        /// <summary>
        /// The schema versions that predate flexible layout sections but know vertical sections.
        /// </summary>
        public static IEnumerable<object[]> SchemaVersionsWithoutFlexibleLayoutSections()
        {
            yield return new object[] { XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_09 };
            yield return new object[] { XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2020_02 };
            yield return new object[] { XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2021_03 };
            yield return new object[] { XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09 };
        }

        /// <summary>
        /// Every schema version the tests save to.
        /// </summary>
        public static IEnumerable<object[]> SchemaVersions()
        {
            return SchemaVersionsWithoutFlexibleLayoutSections()
                .Append(new object[] { XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09 });
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Serialize_WritesFlexibleLayoutSectionsWithSchema202609()
        {
            CollectionAssert.AreEqual(
                new[] { "FlexibleLayoutSection", "FlexibleLayoutVerticalSection", "OneColumn" },
                SerializedSectionTypes(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Serialize_LatestSchemaWritesFlexibleLayoutSections()
        {
            string xml = CreateTemplate(withTranslation: false).ToXML();

            StringAssert.Contains(xml, XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09);
            StringAssert.Contains(xml, "Type=\"FlexibleLayoutSection\"");
            StringAssert.Contains(xml, "Type=\"FlexibleLayoutVerticalSection\"");
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersionsWithoutFlexibleLayoutSections), DynamicDataSourceType.Method)]
        public void Serialize_OlderSchemaWritesTheOneColumnSectionsTheyAreBuiltOn(string namespaceUri)
        {
            CollectionAssert.AreEqual(
                new[] { "OneColumn", "OneColumnVerticalSection", "OneColumn" },
                SerializedSectionTypes(namespaceUri));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Serialize_SchemaWithoutVerticalSectionsWritesOneColumnSections()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            CollectionAssert.AreEqual(
                new[] { "OneColumn", "OneColumn", "OneColumn" },
                SerializedSectionTypes(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_03));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Serialize_SchemaWithoutVerticalSectionsRejectsNonFlexibleVerticalSections()
        {
            var template = CreateTemplate(withTranslation: false);
            template.ClientSidePages[0].Sections.Add(new CanvasSection
            {
                Type = CanvasSectionType.OneColumnVerticalSection,
                Order = 4,
            });

#pragma warning disable CS0618 // Type or member is obsolete
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_03);
#pragma warning restore CS0618 // Type or member is obsolete

            NotSupportedException exception = Assert.ThrowsException<NotSupportedException>(
                () => formatter.ToFormattedTemplate(template));

            StringAssert.Contains(exception.Message, nameof(CanvasSectionType.OneColumnVerticalSection));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Serialize_SchemaWithoutVerticalSectionsFlattensFlexibleVerticalControls()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_03);
#pragma warning restore CS0618 // Type or member is obsolete

            XDocument document;
            using (Stream serialized = formatter.ToFormattedTemplate(CreateTemplate(withTranslation: false)))
            {
                document = XDocument.Load(serialized);
            }

            XNamespace schemaNamespace = "http://schemas.dev.office.com/PnP/2019/03/ProvisioningSchema";
            XElement verticalSection = document.Descendants(schemaNamespace + "Section")
                .Single(section => (int?)section.Attribute("Order") == 2);
            Assert.AreEqual("OneColumn", (string)verticalSection.Attribute("Type"));

            XElement[] controls = verticalSection.Descendants()
                .Where(control => control.Attribute("Column") != null)
                .ToArray();

            CollectionAssert.AreEqual(new[] { "1", "1" }, controls.Select(control => (string)control.Attribute("Column")).ToArray());
            CollectionAssert.AreEqual(new[] { "1", "2" }, controls.Select(control => (string)control.Attribute("Order")).ToArray());
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersions), DynamicDataSourceType.Method)]
        public void Serialize_OutputValidatesAgainstItsSchema(string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            using (Stream serialized = formatter.ToFormattedTemplate(CreateTemplate(withTranslation: true)))
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
        public void RoundTrip_Schema202609KeepsFlexibleLayoutSections()
        {
            ProvisioningTemplate roundTripped = RoundTrip(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09);

            CollectionAssert.AreEqual(SectionTypes, roundTripped.ClientSidePages[0].Sections.Select(s => s.Type).ToList());
            CollectionAssert.AreEqual(SectionTypes, roundTripped.ClientSidePages[0].Translations[0].Sections.Select(s => s.Type).ToList());
        }

        [TestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(SchemaVersionsWithoutFlexibleLayoutSections), DynamicDataSourceType.Method)]
        public void RoundTrip_OlderSchemaRestoresFlexibleLayoutSectionsFromControlData(string namespaceUri)
        {
            ProvisioningTemplate roundTripped = RoundTrip(namespaceUri);

            CollectionAssert.AreEqual(SectionTypes, roundTripped.ClientSidePages[0].Sections.Select(s => s.Type).ToList());
            CollectionAssert.AreEqual(SectionTypes, roundTripped.ClientSidePages[0].Translations[0].Sections.Select(s => s.Type).ToList());
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Deserialize_RestoresFlexibleLayoutSectionsFromLegacyTemplates()
        {
            // Flexible sections as PnP Framework writes them to schemas without flexible section types.
            string xml = $@"<pnp:Provisioning xmlns:pnp=""{XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09}"">
  <pnp:Preferences Generator=""PnP.Framework"" />
  <pnp:Templates ID=""CONTAINER-TEMPLATE"">
    <pnp:ProvisioningTemplate ID=""LegacyFlexibleLayout"" Version=""1"">
      <pnp:ClientSidePages>
        <pnp:ClientSidePage PageName=""Flexible.aspx"" Title=""Flexible"">
          <pnp:Sections>
            <pnp:Section Order=""1"" Type=""OneColumn"">
              <pnp:Controls>
                <pnp:CanvasControl WebPartType=""Text"" Column=""1"" Order=""1"" JsonControlData=""{SecurityElement.Escape(FlexibleControlData)}"" />
              </pnp:Controls>
            </pnp:Section>
            <pnp:Section Order=""2"" Type=""OneColumnVerticalSection"">
              <pnp:Controls>
                <pnp:CanvasControl WebPartType=""Text"" Column=""1"" Order=""1"" JsonControlData=""{SecurityElement.Escape(FlexibleControlData)}"" />
                <pnp:CanvasControl WebPartType=""Text"" Column=""2"" Order=""1"" JsonControlData=""{SecurityElement.Escape(VerticalColumnControlData)}"" />
              </pnp:Controls>
            </pnp:Section>
            <pnp:Section Order=""3"" Type=""OneColumn"">
              <pnp:Controls>
                <pnp:CanvasControl WebPartType=""Text"" Column=""1"" Order=""1"" JsonControlData=""{SecurityElement.Escape(OneColumnControlData)}"" />
              </pnp:Controls>
            </pnp:Section>
          </pnp:Sections>
        </pnp:ClientSidePage>
      </pnp:ClientSidePages>
    </pnp:ProvisioningTemplate>
  </pnp:Templates>
</pnp:Provisioning>";

            ProvisioningTemplate template;
            using (Stream legacy = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                template = XMLPnPSchemaFormatter.GetSpecificFormatter(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09).ToProvisioningTemplate(legacy);
            }

            CollectionAssert.AreEqual(SectionTypes, template.ClientSidePages[0].Sections.Select(s => s.Type).ToList());
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void Deserialize_ReadsFlexibleLayoutSectionsFromTheFullSample()
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09);

            ProvisioningTemplate template;
            using (Stream fixture = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestAssets", "Templates", "ProvisioningSchema-2026-09-FullSample-01.xml")))
            {
                template = formatter.ToProvisioningTemplate(fixture);
            }

            ClientSidePage page = template.ClientSidePages.Single(p => p.PageName == "SamplePageWithFlexibleLayoutSections");

            CollectionAssert.AreEqual(
                new[] { CanvasSectionType.FlexibleLayoutSection, CanvasSectionType.FlexibleLayoutVerticalSection },
                page.Sections.Select(s => s.Type).ToList());
            Assert.AreEqual(Emphasis.Soft, page.Sections[1].VerticalSectionEmphasis);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ToSchemaValue_FallsBackToOneColumnWhenTheSchemaHasNoVerticalSections()
        {
            Assert.AreEqual(Core.Provisioning.Providers.Xml.V201903.CanvasSectionType.OneColumn,
                CanvasSectionTypeMapper.ToSchemaValue(CanvasSectionType.FlexibleLayoutSection, typeof(Core.Provisioning.Providers.Xml.V201903.CanvasSectionType)));
            Assert.AreEqual(Core.Provisioning.Providers.Xml.V201903.CanvasSectionType.OneColumn,
                CanvasSectionTypeMapper.ToSchemaValue(CanvasSectionType.FlexibleLayoutVerticalSection, typeof(Core.Provisioning.Providers.Xml.V201903.CanvasSectionType)));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ToSchemaValue_KeepsSectionTypesTheSchemaKnows()
        {
            Assert.AreEqual(Core.Provisioning.Providers.Xml.V202209.CanvasSectionType.TwoColumnLeftVerticalSection,
                CanvasSectionTypeMapper.ToSchemaValue(CanvasSectionType.TwoColumnLeftVerticalSection, typeof(Core.Provisioning.Providers.Xml.V202209.CanvasSectionType)));
            Assert.AreEqual(Core.Provisioning.Providers.Xml.V202609.CanvasSectionType.FlexibleLayoutVerticalSection,
                CanvasSectionTypeMapper.ToSchemaValue(CanvasSectionType.FlexibleLayoutVerticalSection, typeof(Core.Provisioning.Providers.Xml.V202609.CanvasSectionType)));
        }

        private static List<string> SerializedSectionTypes(string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            using (Stream serialized = formatter.ToFormattedTemplate(CreateTemplate(withTranslation: false)))
            {
                XDocument document = XDocument.Load(serialized);
                return document.Descendants(XName.Get("Section", namespaceUri))
                    .Select(s => (string)s.Attribute("Type"))
                    .ToList();
            }
        }

        private static ProvisioningTemplate RoundTrip(string namespaceUri)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(namespaceUri);

            using (Stream serialized = formatter.ToFormattedTemplate(CreateTemplate(withTranslation: true)))
            {
                return formatter.ToProvisioningTemplate(serialized);
            }
        }

        private static ProvisioningTemplate CreateTemplate(bool withTranslation)
        {
            var template = new ProvisioningTemplate { Id = "FlexibleLayout" };

            var page = new ClientSidePage { PageName = "Flexible.aspx", Title = "Flexible" };
            AddSections(page);

            if (withTranslation)
            {
                var translation = new TranslatedClientSidePage { LCID = 1043, PageName = "Flexible.aspx", Title = "Flexibel" };
                AddSections(translation);
                page.Translations.Add(translation);
            }

            template.ClientSidePages.Add(page);
            return template;
        }

        private static void AddSections(BaseClientSidePage page)
        {
            page.Sections.Add(CreateSection(CanvasSectionType.FlexibleLayoutSection, 1, FlexibleControlData));

            CanvasSection vertical = CreateSection(CanvasSectionType.FlexibleLayoutVerticalSection, 2, FlexibleControlData);
            vertical.Controls.Add(CreateText(VerticalColumnControlData, column: 2));
            page.Sections.Add(vertical);

            page.Sections.Add(CreateSection(CanvasSectionType.OneColumn, 3, OneColumnControlData));
        }

        private static CanvasSection CreateSection(CanvasSectionType type, int order, string controlData)
        {
            var section = new CanvasSection { Type = type, Order = order };
            section.Controls.Add(CreateText(controlData, column: 1));
            return section;
        }

        private static CanvasControl CreateText(string controlData, int column)
        {
            return new CanvasControl
            {
                Type = WebPartType.Text,
                Column = column,
                Order = 1,
                JsonControlData = controlData,
                ControlProperties = new Dictionary<string, string> { { "Text", "<p>Text</p>" } },
            };
        }
    }
}
