using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class TestTests
    {

        [TestMethod]
        public async Task PropertySavingTest()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string property2Value = @"This is value 2 /\ ><:!";
                Dictionary<string, string> myTestProperties = new Dictionary<string, string>
                {
                    { "Prop1", "Value1" },
                    { "Property 2", property2Value }
                };

                TestManager.SaveProperties(context, myTestProperties);

                var loadedProperties = TestManager.GetProperties(context);

                var prop1 = loadedProperties["Prop1"];
                Assert.IsTrue(prop1 == "Value1");

                var prop2 = loadedProperties["Property 2"];
                Assert.IsTrue(prop2 == property2Value);

                TestManager.DeleteProperties(context);
                
                bool exceptionThrown = false;
                try
                {
                    var loadedProperties2 = TestManager.GetProperties(context);
                }
                catch(Exception)
                {
                    exceptionThrown = true;
                }

                Assert.IsTrue(exceptionThrown);
            }
        }

    }
}
