using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.BaseTemplates;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins which web property bag entries survive the comparison with the base template.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class PropertyBagBaseTemplateTests
    {
        private static List<string> KeysKept(ProvisioningTemplateCreationInformation creationInformation, params string[] keys)
        {
            var template = new ProvisioningTemplate();
            foreach (string key in keys)
            {
                template.PropertyBagEntries.Add(new PropertyBagEntry { Key = key, Value = "value" });
            }

            ObjectPropertyBagEntry.RemoveBaseTemplateEntries(template, creationInformation);

            return template.PropertyBagEntries.Select(e => e.Key).ToList();
        }

        private static ProvisioningTemplateCreationInformation CommunicationSite()
        {
            ProvisioningTemplate baseTemplate = BaseTemplateManager.GetBaseTemplate(null, "SITEPAGEPUBLISHING", 0);
            Assert.IsNotNull(baseTemplate, "The SITEPAGEPUBLISHING#0 base template did not load.");

            return new ProvisioningTemplateCreationInformation { BaseTemplate = baseTemplate };
        }

        [TestMethod]
        public void WhatEveryCommunicationSiteHasIsLeftOut()
        {
            // As read from a communication site: SharePoint's own entries, the base template's, and one added by hand.
            List<string> kept = KeysKept(CommunicationSite(),
                "vti_associategroups", "vti_indexedpropertykeys", "vti_defaultlanguage", "profileschemaversion",
                "taxonomyhiddenlist", "FollowLinkEnabled", "ThemePrimary");

            CollectionAssert.AreEquivalent(new[] { "ThemePrimary" }, kept);
        }

        [TestMethod]
        public void TheEnginesOwnEntriesAreKept()
        {
            List<string> kept = KeysKept(CommunicationSite(),
                "_PnP_ProvisioningTemplateId", "_PnP_ProvisioningTemplateInfo", "_SomethingSharePointOwns");

            CollectionAssert.AreEquivalent(new[] { "_PnP_ProvisioningTemplateId", "_PnP_ProvisioningTemplateInfo" }, kept,
                "The template id and info are read back out of the extracted entries, so they must survive.");
        }

        [TestMethod]
        public void AnEntryAskedToBePreservedIsKept()
        {
            ProvisioningTemplateCreationInformation creationInformation = CommunicationSite();
            creationInformation.PropertyBagPropertiesToPreserve.Add("DesignPreviewThemedCssFolderUrl");

            CollectionAssert.AreEquivalent(new[] { "DesignPreviewThemedCssFolderUrl" },
                KeysKept(creationInformation, "DesignPreviewThemedCssFolderUrl", "DesignPreviewLayoutUrl"));
        }
    }
}
