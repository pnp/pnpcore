using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform.Mapping
{
    [TestClass]
    [TestCategory("Integration")]
    public class UserMappingTests
    {

        [TestMethod]
        public void UserMappingFileLoadTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUserMappingFile(@"..\..\Transform\Mapping\usermapping_sample.csv");

            Assert.IsTrue(mapping.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UserMappingFileNotFoundTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUrlMappingFile(@"..\..\Transform\Mapping\idontexist_sample.csv");
        }
    }
}
