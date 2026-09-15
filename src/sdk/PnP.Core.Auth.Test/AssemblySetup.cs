using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Common.Utilities;

namespace PnP.Core.Auth.Test
{
    [TestClass]
    public class AssemblySetup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            TestCommonBase.UseTestTelemetryInstance();
        }
    }
}
