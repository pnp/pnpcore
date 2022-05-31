using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Utilities;
using System;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class DriveHelperTests
    {
        [TestMethod()]
        public void EncodeDriveItemIdTest()
        {
            // File
            Assert.AreEqual("01A7SUVGCLOOP3433QVRE2Z34DBGNLTPUC", DriveHelper.EncodeDriveItemId(new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"), 
                                                                                       new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"), 
                                                                                       new Guid("be9f734b-706f-49ac-acef-83099ab9be82")));
            // Folder
            Assert.AreEqual("01A7SUVGC7VA7ME76V55C3GRGEXYA7QF3X", DriveHelper.EncodeDriveItemId(new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"), 
                                                                                       new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"), 
                                                                                       new Guid("c23ea85f-d57f-45ef-b344-c4be01f81777")));
        }

        [TestMethod()]
        public void DecodeDriveItemIdTest()
        {
            // File
            Assert.AreEqual(new Guid("be9f734b-706f-49ac-acef-83099ab9be82"), DriveHelper.DecodeDriveItemId("01A7SUVGCLOOP3433QVRE2Z34DBGNLTPUC"));
            
            // Folder
            Assert.AreEqual(new Guid("c23ea85f-d57f-45ef-b344-c4be01f81777"), DriveHelper.DecodeDriveItemId("01A7SUVGC7VA7ME76V55C3GRGEXYA7QF3X"));
        }

        [TestMethod()]
        public void EncodeDriveIdTest()
        {
            Assert.AreEqual("b!ed9qtWr_ZEmmOv8fojvp-BwQjowNG1NChefDADm_RuJF2trmBsH2R4cpCCQhPmLA", 
                             DriveHelper.EncodeDriveId(new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"),
                                                       new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"),
                                                       new Guid("e6dada45-c106-47f6-8729-0824213e62c0")));
        }

        [TestMethod()]
        public void DecodeDriveIdTest()
        {
            (Guid siteId, Guid webId, Guid docLibId) = DriveHelper.DecodeDriveId("b!ed9qtWr_ZEmmOv8fojvp-BwQjowNG1NChefDADm_RuJF2trmBsH2R4cpCCQhPmLA");

            Assert.AreEqual(siteId, new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"));
            Assert.AreEqual(webId, new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"));
            Assert.AreEqual(docLibId, new Guid("e6dada45-c106-47f6-8729-0824213e62c0"));
        }

        [TestMethod()]
        public void EncodeSharingUrlTest()
        {
            Assert.AreEqual("u!aHR0cHM6Ly9iZXJ0b25saW5lLnNoYXJlcG9pbnQuY29tLzp3Oi9zL3Byb3YtMS9FZDdhU0Zrc1gzRkZwaGE1UUV2Ry1QZ0JiZkdCX0tJOU8wNFNhaFVtOWNDcHB3P2U9Q0hOeUdZ",
                             DriveHelper.EncodeSharingUrl("https://bertonline.sharepoint.com/:w:/s/prov-1/Ed7aSFksX3FFpha5QEvG-PgBbfGB_KI9O04SahUm9cCppw?e=CHNyGY"));
        }
    }
}
