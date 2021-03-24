using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on Graph vs Rest interoperabilty
    /// </summary>
    [TestClass]
    public class GraphRestTransitionTests
    {
        //[TestMethod]
        //public async Task GetLists()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var webViaGraph = await context.Web.GetAsync(p => p.Lists);

        //        Assert.IsTrue(webViaGraph.Requested);
        //        Assert.IsTrue(webViaGraph.Lists.Requested);
        //        Assert.IsTrue(webViaGraph.Lists.Length > 0);

        //        // Grab the "Documents" library and verify it has the needed Graph+Rest metadata loaded
        //        var documentsViaGraph = webViaGraph.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");

        //        Assert.IsTrue(documentsViaGraph != null);
        //        // Graph id must be populated
        //        Assert.IsTrue((documentsViaGraph as List).Metadata.ContainsKey(PnPConstants.MetaDataGraphId));
        //        Assert.IsTrue((documentsViaGraph as List).Metadata[PnPConstants.MetaDataGraphId] == documentsViaGraph.Id.ToString());
        //        // Given a list also supports rest the rest id must be populated as well
        //        Assert.IsTrue((documentsViaGraph as List).Metadata.ContainsKey(PnPConstants.MetaDataRestId));
        //        Assert.IsTrue((documentsViaGraph as List).Metadata[PnPConstants.MetaDataRestId] == (documentsViaGraph as List).Metadata[PnPConstants.MetaDataGraphId]);
        //        // Rest entity type must be filled
        //        Assert.IsTrue((documentsViaGraph as List).Metadata.ContainsKey(PnPConstants.MetaDataRestEntityTypeName));
        //        Assert.IsTrue(!string.IsNullOrEmpty((documentsViaGraph as List).Metadata[PnPConstants.MetaDataRestEntityTypeName]));
        //        // Rest type type must be filled
        //        Assert.IsTrue((documentsViaGraph as List).Metadata.ContainsKey(PnPConstants.MetaDataType));
        //        Assert.IsTrue(!string.IsNullOrEmpty((documentsViaGraph as List).Metadata[PnPConstants.MetaDataType]));

        //        using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
        //        {
        //            context2.GraphFirst = false;

        //            var webViaRest = await context2.Web.GetAsync(p => p.Lists);

        //            Assert.IsTrue(webViaRest.Requested);
        //            Assert.IsTrue(webViaRest.Lists.Requested);
        //            Assert.IsTrue(webViaRest.Lists.Length > 0);

        //            // We should have loaded the same set of lists
        //            Assert.IsTrue(webViaGraph.Lists.Length == webViaRest.Lists.Length);

        //            // Grab the "Documents" library and verify it has the needed Graph+Rest metadata loaded
        //            var documentsViaRest = webViaRest.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
        //            Assert.IsTrue(documentsViaGraph != null);
        //            // Rest id must be populated
        //            Assert.IsTrue((documentsViaRest as List).Metadata.ContainsKey(PnPConstants.MetaDataRestId));
        //            Assert.IsTrue((documentsViaRest as List).Metadata[PnPConstants.MetaDataRestId] == documentsViaRest.Id.ToString());

        //            // Rest entity type was automatically filled since this was a rest call
        //            Assert.IsTrue((documentsViaRest as List).Metadata.ContainsKey(PnPConstants.MetaDataRestEntityTypeName));
        //            Assert.IsTrue(!string.IsNullOrEmpty((documentsViaRest as List).Metadata[PnPConstants.MetaDataRestEntityTypeName]));
        //            // Rest type type was automatically filled since this was a rest call
        //            Assert.IsTrue((documentsViaRest as List).Metadata.ContainsKey(PnPConstants.MetaDataType));
        //            Assert.IsTrue(!string.IsNullOrEmpty((documentsViaRest as List).Metadata[PnPConstants.MetaDataType]));

        //            Assert.IsTrue((documentsViaGraph as List).Metadata[PnPConstants.MetaDataType] == (documentsViaRest as List).Metadata[PnPConstants.MetaDataType]);

        //            // Verify the "calculated" Rest entity type when loading via Graph matches the one retrieved via a rest call
        //            foreach (var graphList in webViaGraph.Lists.AsRequested())
        //            {
        //                var restList = webViaRest.Lists.AsRequested().FirstOrDefault(p => p.Id == graphList.Id);
        //                Assert.IsTrue(restList != null);
        //                Assert.IsTrue((restList as List).Metadata[PnPConstants.MetaDataRestEntityTypeName] == (graphList as List).Metadata[PnPConstants.MetaDataRestEntityTypeName]);
        //            }
        //        }
        //    }
        //}
    }
}
