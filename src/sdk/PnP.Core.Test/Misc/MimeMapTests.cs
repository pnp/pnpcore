using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Utilities;
using System;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class MimeMapTests
    {
        [TestMethod()]
        [DataRow("image.pnz")]
        [DataRow("image.png")]
        [DataRow("image.jpe")]
        [DataRow("image.jpeg")]
        [DataRow("image.jpg")]
        [DataRow("image.gif")]
        [DataRow("image.bmp")]
        [DataRow("image.dib")]
        [DataRow("image.tif")]
        [DataRow("image.tiff")]
        [DataRow("image.tiff?urlParam=x")]
        [DataRow("something/image.tiff")]
        public void GetMimeMapSuccess(string fileName)
        {
            MimeTypeMap.TryGetMimeType(fileName, out string mimeMap);
            Assert.IsTrue(!string.IsNullOrEmpty(mimeMap));
        }

        [TestMethod()]
        [DataRow("image.exe")]
        public void GetMimeMapNoMatch(string fileName)
        {
            MimeTypeMap.TryGetMimeType(fileName, out string mimeMap);
            Assert.IsNull(mimeMap);
        }

        [TestMethod()]
        [DataRow(null)]
        public void GetMimeMapError(string fileName)
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                MimeTypeMap.TryGetMimeType(fileName, out string mimeMap);
            });            
        }
    }
}
