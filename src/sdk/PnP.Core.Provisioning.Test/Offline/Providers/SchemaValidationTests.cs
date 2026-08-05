using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComposedLookModel = PnP.Core.Provisioning.Model.ComposedLook;
using FieldModel = PnP.Core.Provisioning.Model.Field;

namespace PnP.Core.Provisioning.Test.Offline.Providers
{
    /// <summary>
    /// Checks that what the engine <em>produces</em> validates against the schema it claims to write.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class SchemaValidationTests
    {
        #region The regression that prompted these tests

        [TestMethod]
        public void ComposedLook_WithOnlyANameStillValidates()
        {
            ProvisioningTemplate template = NewTemplate();

            template.ComposedLook = new ComposedLookModel
            {
                Name = "Office",
                ColorFile = string.Empty,
                FontFile = string.Empty,
                BackgroundFile = string.Empty,
            };

            AssertValid(template);
        }

        [TestMethod]
        public void ComposedLook_WithNullFilesIsWhatBrokeAndTheSchemaSaysSo()
        {
            ProvisioningTemplate template = NewTemplate();
            template.ComposedLook = new ComposedLookModel { Name = "Current" };

            ValidationResult result = Validate(template);

            Assert.IsFalse(result.IsValid,
                "A ComposedLook with null file attributes is expected to fail validation - if this " +
                "now passes, the schema changed and the workaround in ObjectComposedLook can go.");

            string message = string.Join(" | ", result.Exceptions.Select(e => e.Message));

            StringAssert.Contains(message, "ColorFile", "The failure should name the missing attribute.");
        }

        #endregion

        #region What a clean site actually extracts to

        [TestMethod]
        public void AnEmptyTemplateValidates()
        {
            AssertValid(NewTemplate());
        }

        [TestMethod]
        public void ACleanCommunicationSiteExtractValidates()
        {
            ProvisioningTemplate template = NewTemplate();

            template.WebSettings = new WebSettings
            {
                Title = "Communication site",
                Description = "A clean site",
            };

            template.Lists.Add(new ListInstance
            {
                Title = "Documents",
                Url = "Shared Documents",
                TemplateType = 101,
            });

            template.Lists.Add(new ListInstance
            {
                Title = "Site Pages",
                Url = "SitePages",
                TemplateType = 119,
            });

            AssertValid(template);
        }

        #endregion

        #region Breadth - one of everything the handlers emit

        [TestMethod]
        public void ATemplateCarryingEveryHandlersOutputValidates()
        {
            ProvisioningTemplate template = NewTemplate();

            template.SiteFields.Add(new FieldModel
            {
                SchemaXml = "<Field ID=\"{b1a2c3d4-1111-2222-3333-444455556666}\" Type=\"Text\" " +
                    "Name=\"Sample\" StaticName=\"Sample\" DisplayName=\"Sample\" Group=\"Samples\" />",
            });

            var contentType = new ContentType
            {
                Id = "0x0100AABBCCDDEEFF00112233445566778899",
                Name = "Sample content type",
                Group = "Samples",
            };

            contentType.FieldRefs.Add(new FieldRef("Sample")
            {
                Id = Guid.Parse("{b1a2c3d4-1111-2222-3333-444455556666}"),
            });

            template.ContentTypes.Add(contentType);

            var list = new ListInstance
            {
                Title = "Sample list",
                Url = "Lists/Sample",
                TemplateType = 100,
            };

            list.FieldRefs.Add(new FieldRef("Sample")
            {
                Id = Guid.Parse("{b1a2c3d4-1111-2222-3333-444455556666}"),
                DisplayName = "Sample",
            });

            template.Lists.Add(list);

            template.PropertyBagEntries.Add(new PropertyBagEntry
            {
                Key = "Sample",
                Value = "Value",
                Indexed = true,
            });

            template.CustomActions.WebCustomActions.Add(new CustomAction
            {
                Name = "SampleAction",
                Title = "Sample action",
                Location = "ClientSideExtension.ApplicationCustomizer",
                ClientSideComponentId = Guid.NewGuid(),
                Sequence = 100,
                Enabled = true,
            });

            template.Security.SiteGroups.Add(new SiteGroup { Title = "Sample group" });

            template.ComposedLook = new ComposedLookModel
            {
                Name = "Office",
                ColorFile = string.Empty,
                FontFile = string.Empty,
                BackgroundFile = string.Empty,
            };

            AssertValid(template);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// A template shaped the way the engine's own extract produces one.
        /// </summary>
        private static ProvisioningTemplate NewTemplate()
        {
            return new ProvisioningTemplate
            {
                Id = $"TEMPLATE-{Guid.NewGuid():N}".ToUpperInvariant(),
            };
        }

        /// <summary>
        /// Serialises a template through the real formatter and validates the result.
        /// </summary>
        private static ValidationResult Validate(ProvisioningTemplate template)
        {
            var formatter = new XMLPnPSchemaFormatter();

            using (Stream serialized = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
            using (var copy = new MemoryStream())
            {
                serialized.CopyTo(copy);
                copy.Position = 0;

                return formatter.GetValidationResults(copy);
            }
        }

        private static void AssertValid(ProvisioningTemplate template)
        {
            ValidationResult result = Validate(template);

            if (result.IsValid)
            {
                return;
            }

            string detail = string.Join(
                Environment.NewLine + "  ",
                result.Exceptions?.Select(e => e.Message) ?? Enumerable.Empty<string>());

            Assert.Fail($"The serialized template does not validate against the schema:" +
                $"{Environment.NewLine}  {detail}{Environment.NewLine}{Environment.NewLine}" +
                $"{Serialize(template)}");
        }

        private static string Serialize(ProvisioningTemplate template)
        {
            using (Stream stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        #endregion
    }
}
