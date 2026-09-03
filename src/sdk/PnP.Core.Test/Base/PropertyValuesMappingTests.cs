using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class PropertyValuesMappingTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // All these tests are offline by design - no real SharePoint calls are made.
        }

        [TestMethod]
        public async Task PropertyValuesWithCustomIdEntry_DoesNotThrow_Issue1698()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var propertyValues = new PropertyValues();
                ((IDataModelWithContext)propertyValues).PnPContext = context;

                // Simulated SP REST response shape for a folder/file property bag containing
                // a custom property literally named "ID" with a non-Guid value.
                using var jsonDoc = JsonDocument.Parse(
                    "{\"ID\":\"467327\",\"vti_isbrowsable\":\"true\",\"vti_level\":1}");

                var apiCall = new ApiCall("_api/dummy", ApiType.SPORest);
                var apiResponse = new ApiResponse(apiCall, jsonDoc.RootElement, Guid.NewGuid());

                // Before the fix this throws FormatException trying to Guid.Parse("467327").
                await ((IDataModelProcess)propertyValues).ProcessResponseAsync(apiResponse);

                // The custom "ID" property bag entry must end up in the overflow dictionary,
                // not be coerced into the synthetic Id property.
                Assert.IsTrue(propertyValues.Values.ContainsKey("ID"));
                Assert.AreEqual("467327", propertyValues.Values["ID"].ToString());

                // The synthetic Id stays unloaded - it has no real server-side counterpart
                // for SP.PropertyValues, so it must NOT be populated from the user property bag.
                Assert.IsFalse(propertyValues.HasValue(nameof(PropertyValues.Id)));

                // Other property bag entries are mapped as before.
                Assert.IsTrue(propertyValues.Values.ContainsKey("vti_isbrowsable"));
                Assert.AreEqual("true", propertyValues.Values["vti_isbrowsable"].ToString());
                Assert.IsTrue(propertyValues.Values.ContainsKey("vti_level"));
            }
        }

        [TestMethod]
        public async Task FieldStringValuesWithCustomIdEntry_DoesNotThrow_Issue1698()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var fieldStringValues = new FieldStringValues();
                ((IDataModelWithContext)fieldStringValues).PnPContext = context;

                using var jsonDoc = JsonDocument.Parse(
                    "{\"ID\":\"not-an-int\",\"Title\":\"hello\"}");

                var apiCall = new ApiCall("_api/dummy", ApiType.SPORest);
                var apiResponse = new ApiResponse(apiCall, jsonDoc.RootElement, Guid.NewGuid());

                // Before the fix this throws FormatException trying to int.Parse("not-an-int").
                await ((IDataModelProcess)fieldStringValues).ProcessResponseAsync(apiResponse);

                Assert.IsTrue(fieldStringValues.Values.ContainsKey("ID"));
                Assert.AreEqual("not-an-int", fieldStringValues.Values["ID"].ToString());
                Assert.IsFalse(fieldStringValues.HasValue(nameof(FieldStringValues.Id)));
                Assert.IsTrue(fieldStringValues.Values.ContainsKey("Title"));
                Assert.AreEqual("hello", fieldStringValues.Values["Title"].ToString());
            }
        }
    }
}
