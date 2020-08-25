using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class CoreTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            
        }
        
        [TestMethod]
        public void OperatingSystemTests()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.IsTrue(OperatingSystem.IsWindows());
                Assert.IsFalse(OperatingSystem.IsMacOS());
                Assert.IsFalse(OperatingSystem.IsLinux());
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.IsFalse(OperatingSystem.IsWindows());
                Assert.IsFalse(OperatingSystem.IsMacOS());
                Assert.IsTrue(OperatingSystem.IsLinux());
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.IsFalse(OperatingSystem.IsWindows());
                Assert.IsTrue(OperatingSystem.IsMacOS());
                Assert.IsFalse(OperatingSystem.IsLinux());
            }

        }

        [TestMethod]
        public void ShellBatTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Simple test
                var result = Shell.Bat("whoami");
                Assert.IsTrue(result.Any());
            }   
        }
    }
}
