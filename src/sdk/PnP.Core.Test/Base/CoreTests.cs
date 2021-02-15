using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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

        [TestMethod]
        public void ShellBashTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Simple test
                var result = Shell.Bash("whoami");
                Assert.IsTrue(result.Any());
            }
        }

        [TestMethod]
        public void StreamCopyAsStringTest()
        {
            string test = "Testing 1-2-3";

            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(test);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                var streamPos = stream.Position;
                var copiedString = stream.CopyAsString();
                Assert.AreEqual(copiedString, test);
                Assert.IsTrue(stream.Position == streamPos);
            }
        }

        [TestMethod]
        public void ContainsTest()
        {
            Assert.IsTrue(StringExtensions.Contains("ThisIsAString", "IsA", System.StringComparison.InvariantCulture));
            Assert.IsFalse(StringExtensions.Contains("ThisIsAString", "isa", System.StringComparison.InvariantCulture));
            Assert.IsTrue(StringExtensions.Contains("ThisIsAString", "isa", System.StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
