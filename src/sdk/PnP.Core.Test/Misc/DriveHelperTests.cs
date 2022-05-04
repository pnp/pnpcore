using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class DriveHelperTests
    {
        [TestMethod()]
        public void EncodeIdTest()
        {
            // File
            Assert.AreEqual("01A7SUVGCLOOP3433QVRE2Z34DBGNLTPUC", DriveHelper.EncodeId(new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"), 
                                                                                       new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"), 
                                                                                       new Guid("be9f734b-706f-49ac-acef-83099ab9be82")));
            // Folder
            Assert.AreEqual("01A7SUVGC7VA7ME76V55C3GRGEXYA7QF3X", DriveHelper.EncodeId(new Guid("b56adf79-ff6a-4964-a63a-ff1fa23be9f8"), 
                                                                                       new Guid("8c8e101c-1b0d-4253-85e7-c30039bf46e2"), 
                                                                                       new Guid("c23ea85f-d57f-45ef-b344-c4be01f81777")));
        }

        [TestMethod()]
        public void DecodeIdTest()
        {
            // File
            Assert.AreEqual(new Guid("be9f734b-706f-49ac-acef-83099ab9be82"), DriveHelper.DecodeId("01A7SUVGCLOOP3433QVRE2Z34DBGNLTPUC"));
            
            // Folder
            Assert.AreEqual(new Guid("c23ea85f-d57f-45ef-b344-c4be01f81777"), DriveHelper.DecodeId("01A7SUVGC7VA7ME76V55C3GRGEXYA7QF3X"));
        }

    }
}
