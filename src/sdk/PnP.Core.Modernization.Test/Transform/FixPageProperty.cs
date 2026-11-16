using System;
using System.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class Provisioning_FixPageProperty
    {
        [TestMethod]
        public void FixPageProperty_NotReallyATest()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(sourceClientContext);

                var pages = sourceClientContext.Web.GetPages("WPP_Image-Asset-MultipleImages");

                var pageItem = pages.FirstOrDefault();

                if (pageItem != default(ListItem))
                {

                    sourceClientContext.Load(pageItem.File, p => p.ServerRelativeUrl);
                    sourceClientContext.ExecuteQueryRetry();

                    var pageFile = sourceClientContext.Web.GetFileByServerRelativeUrl(pageItem.File.ServerRelativeUrl);
                    sourceClientContext.Load(pageFile, p => p.Properties);
                    pageFile.Properties["vti_setuppath"] = "1033\\STS\\doctemp\\smartpgs\\spstd2.aspx";
                    pageFile.Properties["fixing_layout"] = "1033\\STS\\doctemp\\smartpgs\\spstd2.aspx";
                    pageFile.Update();
                    sourceClientContext.ExecuteQueryRetry();

                    // Try both
                    //page.Properties["vti_setuppath"] = @"1033\STS\doctemp\smartpgs\spstd2.aspx";
                    //page.Update();
                    //sourceClientContext.ExecuteQueryRetry();



                    Assert.IsTrue(true, "File Updated");
                }
                else
                {
                    Assert.Fail("File not found");
                }
            }
        }
    }
}
