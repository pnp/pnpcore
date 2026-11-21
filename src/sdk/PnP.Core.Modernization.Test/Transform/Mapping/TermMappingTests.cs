using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Utilities;

namespace PnP.Core.Modernization.Tests.Transform.Mapping
{
    [TestClass]
    [TestCategory("Integration")]
    public class TermMappingTests
    {
        [TestMethod]
        public void TermMappingFileLoadTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadTermMappingFile(@"..\..\Transform\Mapping\term_mapping_sample.csv");

            Assert.IsTrue(mapping.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TermMappingFileNotFoundTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadTermMappingFile(@"..\..\Transform\Mapping\idontexist_sample.csv");
        }

        [TestMethod]
        public void BasicOnlineWikiPage_TermMappingTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    //pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPagesFromList("Site Pages", pageNameStartsWith: "Common-WikiPageTest");

                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            SkipUserMapping = true,
                            SkipDefaultUrlRewrite = true,
                            SkipUrlRewrite = true,

                            // Term store mapping
                            TermMappingFile = @"..\..\Transform\Mapping\term_mapping_sample.csv",

                            //Should process default mapping
                            SkipTermStoreMapping = true,

                            CopyPageMetadata = true

                        };

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();

                }
            }
        }

        [TestMethod]
        public void BasicOnlineWikiPage_DefaultTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    //pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPagesFromList("Site Pages", pageNameStartsWith: "Common-WikiPageTest");

                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            TargetPagePrefix = "DefaultMapping-",

                            SkipUserMapping = true,
                            SkipDefaultUrlRewrite = true,
                            SkipUrlRewrite = true,

                            //Should process default mapping
                            SkipTermStoreMapping = false,

                            CopyPageMetadata = true

                        };

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();

                }
            }
        }

        [TestMethod]
        public void BasicOnlinePublishingPage_TermDefaultTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    var pageTransformator = new PublishingPageTransformator(sourceClientContext, targetClientContext, @"C:\temp\spo-mapping-all-test.xml");
                    //pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPagesFromList("Pages", pageNameStartsWith: "Article-PnP-Example");

                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            // Term store mapping
                            TermMappingFile = string.Empty,

                            //Should process default mapping
                            SkipTermStoreMapping = false

                        };

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();

                }
            }
        }

        [TestMethod]
        public void BasicOnlinePublishingPage_TermMappingTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    var pageTransformator = new PublishingPageTransformator(sourceClientContext, targetClientContext, @"C:\temp\spo-mapping-all-test.xml");
                    //pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());


                    var pages = sourceClientContext.Web.GetPagesFromList("Pages",  pageNameStartsWith: "Article-PnP-Example");

                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,
                            
                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            // Term store mapping
                            TermMappingFile = @"..\..\Transform\Mapping\term_mapping_sample.csv",

                            SkipTermStoreMapping = true

                        };

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();

                }
            }
        }

        [TestMethod]
        public void CacheTermStoreSiteCollectionByIdTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    // Need to have the term store populated values
                    var result = CacheManager.Instance.GetTransformTermCacheTermById(sourceClientContext, new Guid("ac625b0a-0459-4d23-bc96-0970abd1029d"));
                    var expectedLabel = "Announcements";

                    Assert.AreEqual(expectedLabel, result.TermLabel);
                }
            }
        }

        [TestMethod]
        public void CacheTermStoreSiteCollectionByNameTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    // Need to have the term store populated values
                    var expectedLabel = "Announcements";
                    var result = CacheManager.Instance.GetTransformTermCacheTermByName(sourceClientContext, expectedLabel);

                    Assert.IsTrue(result.Count > 0);

                    result.ForEach(o => Console.WriteLine("Cached Term: {0} {1} ", o.TermSetId, o.TermPath, o.TermLabel));
                                       
                }
            }
        }

        [TestMethod]
        public void CacheTermStoreTenantStoreTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    // Need to have the term store populated values
                    var result = CacheManager.Instance.GetTransformTermCacheTermById(sourceClientContext, new Guid("c9cbb11b-77ed-4890-ae24-ee103002c46b"));
                    var expectedLabel = "PnPTransform";

                    Assert.AreEqual(expectedLabel, result.TermLabel);
                }
            }
        }

        [TestMethod]
        public void GetTermSetPathsTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    var results = TermTransformator.GetAllTermsFromTermSet(new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"), sourceClientContext);
                    foreach(var result in results)
                    {
                        Console.WriteLine($"ID: {result.Key} {result.Value.TermPath}");
                    }

                    Assert.IsTrue(results.Count > 0); //Super simple
                }
            }
        }


        [TestMethod]
        public void ValidateTermById_PositiveTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    // Need to have the term store populated values
                    var result = termTransformator.ResolveTermInCache(sourceClientContext, new Guid("ac625b0a-0459-4d23-bc96-0970abd1029d"));
                    var expectedLabel = "Announcements";

                    Assert.AreEqual(expectedLabel, result.TermLabel);
                }
            }
        }

        [TestMethod]
        public void ValidateTermById_NegativeTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    TermTransformator termTransformator = new TermTransformator(null, sourceClientContext, targetClientContext);

                    // Target - Katchup - dc2cca62-cf9a-4a61-b672-4aa7d0b6a179
                    // Source - Categories - e757dcf5-f443-42e9-98c6-5842861099cb (site collection term set)
                    termTransformator.CacheTermsFromTermStore(
                        new Guid("e757dcf5-f443-42e9-98c6-5842861099cb"),
                        new Guid("dc2cca62-cf9a-4a61-b672-4aa7d0b6a179"), Guid.Empty, false);

                    // Need to have the term store populated values
                    // Announcements
                    var result = termTransformator.ResolveTermInCache(sourceClientContext, new Guid("11111111-2222-3333-4444-0970abd1029d"));
                    
                    Assert.IsTrue(result == default);
                }
            }
        }

        [TestMethod]
        public void ExtractTermSetIdFromSchemaTest()
        {
            var inputSchema = "<Field Type=\"TaxonomyFieldTypeMulti\" DisplayName=\"PnPCategory\" List=\"{4c44ff30-1049-433b-8b46-3f5e1d03622d}\" WebId=\"c665bf3c-0512-4973-8bb0-7e12839b520b\" ShowField=\"Term1033\" Required=\"FALSE\" EnforceUniqueValues=\"FALSE\" Mult=\"TRUE\" Sortable=\"FALSE\" ID=\"{e8f72832-999e-4bbe-8b14-0b1b5de424aa}\" SourceID=\"{07ffec92-8050-42dd-a31c-127a254e76e2}\" StaticName=\"PnPCategory\" Name=\"PnPCategory\" ColName=\"int1\" RowOrdinal=\"0\" Version=\"1\"><Default /><Customization><ArrayOfProperty><Property><Name>SspId</Name><Value xmlns:q1=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q1:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">bf53dc87-0092-47bf-8463-ee69cde66b93</Value></Property><Property><Name>GroupId</Name></Property><Property><Name>TermSetId</Name><Value xmlns:q2=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q2:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">5e8a3614-8777-4eb0-9890-e3a6ac466396</Value></Property><Property><Name>AnchorId</Name><Value xmlns:q3=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q3:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">00000000-0000-0000-0000-000000000000</Value></Property><Property><Name>UserCreated</Name><Value xmlns:q4=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q4:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>Open</Name><Value xmlns:q5=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q5:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>TextField</Name><Value xmlns:q6=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q6:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">{2fa0ab86-9de6-407c-8279-2784fd894587}</Value></Property><Property><Name>IsPathRendered</Name><Value xmlns:q7=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q7:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>IsKeyword</Name><Value xmlns:q8=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q8:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>TargetTemplate</Name></Property><Property><Name>CreateValuesInEditForm</Name><Value xmlns:q9=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q9:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>FilterAssemblyStrongName</Name><Value xmlns:q10=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q10:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Value></Property><Property><Name>FilterClassName</Name><Value xmlns:q11=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q11:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">Microsoft.SharePoint.Taxonomy.TaxonomyField</Value></Property><Property><Name>FilterMethodName</Name><Value xmlns:q12=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q12:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">GetFilteringHtml</Value></Property><Property><Name>FilterJavascriptProperty</Name><Value xmlns:q13=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q13:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">FilteringJavascript</Value></Property></ArrayOfProperty></Customization></Field>";

            var result = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(inputSchema, false); 
            var expected = "5e8a3614-8777-4eb0-9890-e3a6ac466396";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ExtractSsdIdFromSchemaTest()
        {
            var inputSchema = "<Field Type=\"TaxonomyFieldTypeMulti\" DisplayName=\"PnPCategory\" List=\"{4c44ff30-1049-433b-8b46-3f5e1d03622d}\" WebId=\"c665bf3c-0512-4973-8bb0-7e12839b520b\" ShowField=\"Term1033\" Required=\"FALSE\" EnforceUniqueValues=\"FALSE\" Mult=\"TRUE\" Sortable=\"FALSE\" ID=\"{e8f72832-999e-4bbe-8b14-0b1b5de424aa}\" SourceID=\"{07ffec92-8050-42dd-a31c-127a254e76e2}\" StaticName=\"PnPCategory\" Name=\"PnPCategory\" ColName=\"int1\" RowOrdinal=\"0\" Version=\"1\"><Default /><Customization><ArrayOfProperty><Property><Name>SspId</Name><Value xmlns:q1=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q1:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">bf53dc87-0092-47bf-8463-ee69cde66b93</Value></Property><Property><Name>GroupId</Name></Property><Property><Name>TermSetId</Name><Value xmlns:q2=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q2:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">5e8a3614-8777-4eb0-9890-e3a6ac466396</Value></Property><Property><Name>AnchorId</Name><Value xmlns:q3=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q3:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">00000000-0000-0000-0000-000000000000</Value></Property><Property><Name>UserCreated</Name><Value xmlns:q4=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q4:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>Open</Name><Value xmlns:q5=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q5:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>TextField</Name><Value xmlns:q6=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q6:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">{2fa0ab86-9de6-407c-8279-2784fd894587}</Value></Property><Property><Name>IsPathRendered</Name><Value xmlns:q7=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q7:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>IsKeyword</Name><Value xmlns:q8=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q8:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>TargetTemplate</Name></Property><Property><Name>CreateValuesInEditForm</Name><Value xmlns:q9=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q9:boolean\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">false</Value></Property><Property><Name>FilterAssemblyStrongName</Name><Value xmlns:q10=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q10:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Value></Property><Property><Name>FilterClassName</Name><Value xmlns:q11=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q11:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">Microsoft.SharePoint.Taxonomy.TaxonomyField</Value></Property><Property><Name>FilterMethodName</Name><Value xmlns:q12=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q12:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">GetFilteringHtml</Value></Property><Property><Name>FilterJavascriptProperty</Name><Value xmlns:q13=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q13:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">FilteringJavascript</Value></Property></ArrayOfProperty></Customization></Field>";

            var result = TermTransformator.ExtractTermSetIdOrSspIdFromXmlSchema(inputSchema, true);
            var expected = "bf53dc87-0092-47bf-8463-ee69cde66b93";

            Assert.AreEqual(expected, result);
        }

    }
}
