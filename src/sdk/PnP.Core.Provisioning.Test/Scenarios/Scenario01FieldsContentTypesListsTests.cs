using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ContentTypeModel = PnP.Core.Provisioning.Model.ContentType;
using FieldModel = PnP.Core.Provisioning.Model.Field;

namespace PnP.Core.Provisioning.Test.Scenarios
{
    /// <summary>
    /// Scenario 1 - fields, content types, lists and a <b>cross-list lookup</b>.
    /// </summary>
    [TestClass]
    public class Scenario01FieldsContentTypesListsTests : ScenarioTestBase
    {
        private const string LookupFieldId = "{4f1d0b7e-3a52-4a1e-9a1c-9a9d1b7c5e01}";
        private const string TextFieldId = "{7a2c5f19-9e64-4d3b-8c2a-1f4b6d8e2a02}";
        private const string ContentTypeId = "0x0100A9E1B4C25D3E4F5A8B7C6D5E4F3A2B1C";

        private static readonly string TargetListUrl = $"Lists/{ScenarioPrefix}Target";
        private static readonly string SourceListUrl = $"Lists/{ScenarioPrefix}Source";

        private const string ScenarioPrefix = "PnPCoreScenario1_";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Scenario")]
        [Timeout(45 * 60 * 1000)]
        public async Task Scenario1_FieldsContentTypesListsAndACrossListLookup()
        {
            await RunScenarioAsync("s1", BuildTemplate(), new[]
            {
                ConfigurationHandler.Fields,
                ConfigurationHandler.ContentTypes,
                ConfigurationHandler.Lists,
            },
            AssertAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// The template. <b>The source list is declared before the list its lookup points at.</b>
        /// </summary>
        private static ProvisioningTemplate BuildTemplate()
        {
            var template = new ProvisioningTemplate { Id = "SCENARIO-1" };

            template.SiteFields.Add(new FieldModel
            {
                SchemaXml = $"<Field ID=\"{TextFieldId}\" Type=\"Text\" Name=\"{ScenarioPrefix}Code\" " +
                    $"StaticName=\"{ScenarioPrefix}Code\" DisplayName=\"Code\" Group=\"{ScenarioPrefix}Group\" />",
            });

            template.SiteFields.Add(new FieldModel
            {
                SchemaXml = $"<Field ID=\"{LookupFieldId}\" Type=\"Lookup\" Name=\"{ScenarioPrefix}Target\" " +
                    $"StaticName=\"{ScenarioPrefix}Target\" DisplayName=\"Target\" " +
                    $"Group=\"{ScenarioPrefix}Group\" List=\"{TargetListUrl}\" ShowField=\"Title\" />",
            });

            var contentType = new ContentTypeModel
            {
                Id = ContentTypeId,
                Name = $"{ScenarioPrefix}Item",
                Description = "Scenario 1 content type",
                Group = $"{ScenarioPrefix}Group",
            };

            contentType.FieldRefs.Add(new FieldRef($"{ScenarioPrefix}Code")
            {
                Id = Guid.Parse(TextFieldId),
            });

            template.ContentTypes.Add(contentType);

            var source = new ListInstance
            {
                Title = $"{ScenarioPrefix}Source",
                Url = SourceListUrl,
                TemplateType = (int)ListTemplateType.GenericList,
                ContentTypesEnabled = true,
            };

            source.ContentTypeBindings.Add(new ContentTypeBinding { ContentTypeId = ContentTypeId, Default = true });

            source.FieldRefs.Add(new FieldRef($"{ScenarioPrefix}Target")
            {
                Id = Guid.Parse(LookupFieldId),
            });

            template.Lists.Add(source);

            template.Lists.Add(new ListInstance
            {
                Title = $"{ScenarioPrefix}Target",
                Url = TargetListUrl,
                TemplateType = (int)ListTemplateType.GenericList,
            });

            return template;
        }

        private static async Task AssertAsync(ProvisioningTemplate extracted, PnPContext site)
        {
            await site.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title))
                .ConfigureAwait(false);

            IList sourceList = site.Web.Lists.AsRequested()
                .FirstOrDefault(l => l.Title == $"{ScenarioPrefix}Source");
            IList targetList = site.Web.Lists.AsRequested()
                .FirstOrDefault(l => l.Title == $"{ScenarioPrefix}Target");

            Assert.IsNotNull(sourceList, "The source list was not created.");
            Assert.IsNotNull(targetList, "The target list was not created.");

            await sourceList.LoadAsync(l => l.Fields.QueryProperties(
                f => f.InternalName, f => f.TypeAsString, f => f.SchemaXml)).ConfigureAwait(false);

            IField lookup = sourceList.Fields.AsRequested()
                .FirstOrDefault(f => f.InternalName == $"{ScenarioPrefix}Target");

            Assert.IsNotNull(lookup, $"The lookup column is not on the source list.");

            string listAttribute = (string)XElement.Parse(lookup.SchemaXml).Attribute("List");

            Console.WriteLine($"Lookup List attribute: {listAttribute}");
            Console.WriteLine($"Target list id:        {targetList.Id}");

            Assert.IsFalse(string.IsNullOrEmpty(listAttribute),
                "The lookup column has no List attribute, so it points at nothing - which is what a " +
                "single-pass apply produces, and SharePoint reports as success.");

            Assert.AreEqual(targetList.Id, Guid.Parse(listAttribute.Trim('{', '}')),
                "The lookup column does not point at the list the template named. This is the " +
                "three-pass ordering failing: the lookup was created before its target list existed.");

            await site.Web.LoadAsync(w => w.ContentTypes.QueryProperties(c => c.StringId, c => c.Name))
                .ConfigureAwait(false);

            Assert.IsTrue(site.Web.ContentTypes.AsRequested().Any(c => c.StringId == ContentTypeId),
                "The content type was not created.");

            Assert.IsTrue(sourceList.Fields.AsRequested().Any(f => f.InternalName == $"{ScenarioPrefix}Code"),
                "The content type's field did not reach the list that binds it.");

            Assert.IsTrue(extracted.Lists.Any(l => l.Title == $"{ScenarioPrefix}Source"),
                "The extract did not report the source list.");
            Assert.IsTrue(extracted.Lists.Any(l => l.Title == $"{ScenarioPrefix}Target"),
                "The extract did not report the target list.");

            List<string> extractedFieldNames = extracted.SiteFields
                .Select(f => (string)XElement.Parse(f.SchemaXml).Attribute("Name"))
                .ToList();

            Assert.IsTrue(extractedFieldNames.Contains($"{ScenarioPrefix}Target"),
                $"The extract did not report the lookup site column. Found: {string.Join(", ", extractedFieldNames)}");
        }
    }
}
