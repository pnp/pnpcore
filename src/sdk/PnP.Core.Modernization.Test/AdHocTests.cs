using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using System.Linq;
using System.IO;
using AngleSharp.Html.Parser;

namespace PnP.Core.Modernization.Tests
{
    [TestClass]
    [TestCategory("Integration")]
    public class AdHocTests
    {
/**
        [TestMethod]
        public void Workflowtest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/temp2"))
            {
                cc.Load(cc.Web, p => p.WorkflowTemplates, p => p.WorkflowAssociations, p => p.WorkflowTemplates);
                cc.ExecuteQueryRetry();

                var wf = cc.Web.WorkflowTemplates.Where(p => p.Name.StartsWith("demo")).FirstOrDefault();
                if (wf != null)
                {
                    var workflowList = cc.Web.GetListByTitle("Workflows");

                    if (workflowList != null)
                    {
                        workflowList.EnsureProperty(p => p.RootFolder);

                        var file = workflowList.RootFolder.GetFile($"{wf.Name}/{wf.Name}.xoml");

                        ClientResult<System.IO.Stream> xomlFile = file.OpenBinaryStream();

                        cc.Load(file);
                        cc.ExecuteQueryRetry();

                        string text;
                        using (Stream sourceFileStream = xomlFile.Value)
                        {
                            StreamReader reader = new StreamReader(sourceFileStream);
                            text = reader.ReadToEnd();
                        }




                    }
                }


                //var wfAssoc = cc.Web.WorkflowAssociations.FirstOrDefault();
                //if (wfAssoc != null)
                //{

                //}


            }
        }
    **/

        [TestMethod]
        public void TestMethod1()
        {
            using (var cc = TestCommon.CreateClientContext())
            {
                var pages = WebExtensions.GetPages(cc.Web,"Header");

                Assert.IsTrue(pages.AsRequested().Any());

            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var cc = TestCommon.CreateClientContext())
            {
                ContentByQuerySearchTransformator cqs = new ContentByQuerySearchTransformator(cc);
                var res = cqs.HighlightedContentProperties();
            }
        }


        [TestMethod]
        public void TestMethod3()
        {
            using (var cc = TestCommon.CreateClientContext())
            {
                // sample config from web part filtering data from document library
                ContentByQuery cbq = new ContentByQuery()
                {
                    WebUrl = "~sitecollection",
                    ListGuid = "c3f3860d-53c9-4118-b985-8701af1505b0", // done
                    ListName = "Documents", // done
                    ServerTemplate = "101", 
                    ContentTypeBeginsWithId = "0x0101",

                    
                    // Done for Title
                    FilterField1 = "Title",
                    FilterField1Value = "value1",
                    FilterOperator1 = FilterFieldQueryOperator.BeginsWith,
                    Filter1ChainingOperator = FilterChainingOperator.And,
                    FilterField2 = "FileLeafRef",
                    FilterField2Value = "value2",
                    FilterOperator2 = FilterFieldQueryOperator.Eq,
                    Filter2ChainingOperator = FilterChainingOperator.And,
                    FilterField3 = "Title",
                    FilterField3Value = "value3",
                    FilterOperator3 = FilterFieldQueryOperator.Contains,

                    SortBy = "FileLeafRef", // Done, not usable unless managed property
                    SortByDirection = SortDirection.Asc, // Done, not usable unless managed property
                    GroupBy = "", // Done, not usable today
                    GroupByDirection = SortDirection.Desc, // Done, not usable today

                    ItemLimit = 15, // done
                    DisplayColumns = 1, // done 

                    DataMappings = "Description:|ImageUrl:|Title:{fa564e0f-0c70-4ab9-b863-0177e6ddd247},Title,Text;|LinkUrl:{94f89715-e097-4e8b-ba79-ea02aa8b7adb},FileRef,Lookup;|"
                };
                
                ContentByQuerySearchTransformator cqs = new ContentByQuerySearchTransformator(cc);
                var res = cqs.TransformContentByQueryWebPartToHighlightedContent(cbq);
                
            }
        }

        [TestMethod]
        public void WikiSplitTest()
        {
            string text = File.ReadAllText(@"C:\temp\htmlsplittest.html");

            string[] split = text.Split(new string[] { "<span class=\"split\"></span>" }, StringSplitOptions.RemoveEmptyEntries);
            HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });

            foreach(var part in split)
            {
                using (var document = parser.ParseDocument(part))
                {
                    if (document.DocumentElement.Children.Count() > 1)
                    {
                        string updatedText = document.DocumentElement.Children[1].InnerHtml;
                    }
                }
            }
        }


    }
}
