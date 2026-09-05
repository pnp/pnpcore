using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins the rule that turns a list content type's id into the site content type a template can
    /// bind to.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class ParentContentTypeIdTests
    {
        [TestMethod]
        public void ASiteContentTypeAddedToAListStripsTheListSuffix()
        {
            Assert.AreEqual("0x01",
                ObjectListInstance.ParentContentTypeId("0x010009B9C2A7D0F14B4E9F7C1E2B3A4D5E6F"));

            Assert.AreEqual("0x0120",
                ObjectListInstance.ParentContentTypeId("0x012000A1B2C3D4E5F60718293A4B5C6D7E8F90"));

            Assert.AreEqual("0x0120D520",
                ObjectListInstance.ParentContentTypeId("0x0120D52000FEDCBA98765432100123456789ABCDEF"));
        }

        [TestMethod]
        public void AContentTypeDeclaredByAListDefinitionYieldsItsHierarchyParent()
        {
            Assert.AreEqual("0x01", ObjectListInstance.ParentContentTypeId("0x01FD"));

            Assert.AreEqual("0x01", ObjectListInstance.ParentContentTypeId("0x0101"));

            Assert.AreEqual("0x0101", ObjectListInstance.ParentContentTypeId("0x010108"));
        }

        /// <summary>
        /// The walk that turns a list content type into something a site actually offers.
        /// </summary>
        [TestMethod]
        public void TheAncestryIsWalkedUntilTheSiteOffersTheContentType()
        {
            var siteContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "0x01", "0x0101", "0x0120",
            };

            Assert.AreEqual("0x01", ObjectListInstance.BindableContentTypeId(
                "0x01FD0034F0F0F1F2F3F4F5F6F7F8F9FA0B1C2D", siteContentTypes));

            Assert.AreEqual("0x0101", ObjectListInstance.BindableContentTypeId(
                "0x010100A1B2C3D4E5F60718293A4B5C6D7E8F90", siteContentTypes));

            Assert.AreEqual(BuiltInContentTypeId.System, ObjectListInstance.BindableContentTypeId(
                "0x0900", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "0x0101" }));
        }

        [TestMethod]
        public void WithNoSiteContentTypesItDegradesToTheImmediateParent()
        {
            Assert.AreEqual("0x01FD", ObjectListInstance.BindableContentTypeId(
                "0x01FD0034F0F0F1F2F3F4F5F6F7F8F9FA0B1C2D", null));
        }

        [TestMethod]
        public void AnIdWithNoParentReadsAsTheSystemContentType()
        {
            Assert.AreEqual(BuiltInContentTypeId.System, ObjectListInstance.ParentContentTypeId("0x01"));
            Assert.AreEqual(BuiltInContentTypeId.System, ObjectListInstance.ParentContentTypeId("0x"));
            Assert.AreEqual(BuiltInContentTypeId.System, ObjectListInstance.ParentContentTypeId(""));
            Assert.AreEqual(BuiltInContentTypeId.System, ObjectListInstance.ParentContentTypeId(null));
        }
    }
}
