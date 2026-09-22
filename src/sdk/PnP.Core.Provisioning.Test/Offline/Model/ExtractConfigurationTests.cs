using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.BaseTemplates;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;

namespace PnP.Core.Provisioning.Test.Offline.Model
{
    /// <summary>
    /// Pins how an extraction's base template reaches the handlers through
    /// <see cref="ExtractConfiguration.ToCreationInformation"/>.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class ExtractConfigurationTests
    {
        private static ProvisioningTemplate CommunicationSiteBaseTemplate()
        {
            ProvisioningTemplate baseTemplate = BaseTemplateManager.GetBaseTemplate(null, "SITEPAGEPUBLISHING", 0);
            Assert.IsNotNull(baseTemplate, "The SITEPAGEPUBLISHING#0 base template did not load.");
            return baseTemplate;
        }

        [TestMethod]
        public void CompareWithBaseTemplate_IsOnByDefault()
        {
            Assert.IsTrue(new ExtractConfiguration().CompareWithBaseTemplate);
            Assert.IsTrue(ExtractConfiguration.FromString("{}").CompareWithBaseTemplate,
                "A JSON configuration cannot turn the comparison off, so it must come back on.");
        }

        [TestMethod]
        public void ToCreationInformation_HandsTheHandlersTheBaseTemplateOfTheRun()
        {
            ProvisioningTemplate baseTemplate = CommunicationSiteBaseTemplate();
            var configuration = new ExtractConfiguration { BaseTemplate = baseTemplate };

            ProvisioningTemplateCreationInformation creationInformation = configuration.ToCreationInformation();

            Assert.AreSame(baseTemplate, creationInformation.BaseTemplate);
            Assert.AreSame(creationInformation, configuration.ToCreationInformation(),
                "Every handler of one run must share one creation information - it collects the resource tokens.");
        }

        [TestMethod]
        public void ToCreationInformation_LeavesTheBaseTemplateOutWhenTheComparisonIsOff()
        {
            var configuration = new ExtractConfiguration
            {
                BaseTemplate = CommunicationSiteBaseTemplate(),
                CompareWithBaseTemplate = false,
            };

            Assert.IsNull(configuration.ToCreationInformation().BaseTemplate);
        }

        [TestMethod]
        public void ToCreationInformation_AnExplicitBaseTemplateWins()
        {
            ProvisioningTemplate explicitBaseTemplate = new ProvisioningTemplate();
            var configuration = new ExtractConfiguration { BaseTemplate = CommunicationSiteBaseTemplate() };

            Assert.AreSame(explicitBaseTemplate, configuration.ToCreationInformation(explicitBaseTemplate).BaseTemplate);
        }

        [TestMethod]
        public void ResetCreationInformation_StartsTheNextRunFromScratch()
        {
            var configuration = new ExtractConfiguration();

            ProvisioningTemplateCreationInformation first = configuration.ToCreationInformation();
            first.ResourceTokens.Add(new System.Tuple<string, int, string>("Field_Title_DisplayName", 1033, "Title"));

            configuration.ResetCreationInformation();
            ProvisioningTemplateCreationInformation second = configuration.ToCreationInformation();

            Assert.AreNotSame(first, second);
            Assert.AreEqual(0, second.ResourceTokens.Count,
                "A second extraction with the same configuration inherited the resource tokens of the first.");
        }

        [TestMethod]
        public void FromCreationInformation_KeepsTheCallersBaseTemplate()
        {
            ProvisioningTemplate baseTemplate = CommunicationSiteBaseTemplate();

            ExtractConfiguration configuration = ExtractConfiguration.FromCreationInformation(
                new ProvisioningTemplateCreationInformation { BaseTemplate = baseTemplate });

            Assert.AreSame(baseTemplate, configuration.ToCreationInformation().BaseTemplate);
        }
    }
}
