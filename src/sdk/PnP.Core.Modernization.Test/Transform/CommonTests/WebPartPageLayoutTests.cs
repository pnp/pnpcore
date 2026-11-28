using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Tests.Transform.CommonTests
{
    [TestClass]
    [TestCategory("Offline")]
    public class WebPartPageLayoutTests
    {
        private static PnPContext CreateContext() => CoreTestContext.WithFile(
            "https://contoso.sharepoint.com/sites/source", new
            {
                UniqueId = Guid.NewGuid(), ServerRelativeUrl = "/sites/source/default.aspx",
                Properties = new Dictionary<string, object>()
            });

        public static IEnumerable<object[]> SetupPaths
        {
            get
            {
                yield return new object[] { @"SiteTemplates\STS\defaultdws.aspx", PageLayout.WebPart_DocumentWorkspace };
                yield return new object[] { @"SiteTemplates\ProjectSite\default.aspx", PageLayout.WebPart_ProjectSite };
                yield return new object[] { @"Features\BDR\default.aspx", PageLayout.WebPart_DocumentCenter };
                yield return new object[] { @"1033\STS\doctemp\blankpgs\bpstd.aspx", PageLayout.WebPart_FullPageVertical };
                yield return new object[] { @"SiteTemplates\STS\default.aspx", PageLayout.WebPart_2010_TwoColumnsLeft };
                var layouts = new[]
                {
                    PageLayout.WebPart_FullPageVertical,
                    PageLayout.WebPart_HeaderFooterThreeColumns,
                    PageLayout.WebPart_HeaderLeftColumnBody,
                    PageLayout.WebPart_HeaderRightColumnBody,
                    PageLayout.WebPart_HeaderFooter2Columns4Rows,
                    PageLayout.WebPart_HeaderFooter4ColumnsTopRow,
                    PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns,
                    PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns
                };
                for (var index = 0; index < layouts.Length; index++)
                {
                    yield return new object[] { $@"1033\STS\doctemp\smartpgs\spstd{index + 1}.aspx", layouts[index] };
                }
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(SetupPaths))]
        public void SetupPathsMatchInOnlineAnalyzerWithCasingAndPrefixes(string setupPath, PageLayout expected)
        {
            using (var context = CreateContext())
            {
                var file = context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx");
                var analyzer = new WebPartPage(null, file, new PageTransformation());
                foreach (var path in new[]
                {
                    setupPath, setupPath.ToUpperInvariant(), "\\" + setupPath,
                    @"C:\SharePoint\TEMPLATE\" + setupPath, setupPath.Replace('\\', '/')
                })
                {
                    file.Properties["vti_setuppath"] = path;
                    Assert.AreEqual(expected, analyzer.GetLayout(file.Properties), path);
                }
            }
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(@"SiteTemplates\Custom\default.aspx")]
        [DataRow(@"NotSiteTemplates\STS\defaultdws.aspx")]
        [DataRow(@"Features\NotBDR\default.aspx")]
        [DataRow(@"SiteTemplates\ProjectSite\default.aspx.bak")]
        [DataRow(@"custombpstd.aspx")]
        public void UnknownSetupPathsRemainCustom(string path)
        {
            Assert.AreEqual(PageLayout.WebPart_Custom, WebPartPage.GetLayoutFromSetupPath(path));
        }

        [TestMethod]
        public void MissingSetupPathDoesNotInferLayoutFromPageContent()
        {
            using (var context = CreateContext())
            {
                var file = context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx");
                var analyzer = new WebPartPage(null, file, new PageTransformation());
                Assert.AreEqual(PageLayout.WebPart_Custom, analyzer.GetLayout(file.Properties));
                file.Properties["vti_setuppath"] = null;
                Assert.AreEqual(PageLayout.WebPart_Custom, analyzer.GetLayout(file.Properties));
            }
            Assert.AreEqual(PageLayout.WebPart_FullPageVertical, WebPartPage.GetLayoutFromSetupPath("bpstd.aspx"));
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_DocumentWorkspace, "Top", 1, 1)]
        [DataRow(PageLayout.WebPart_DocumentWorkspace, "Left", 2, 1)]
        [DataRow(PageLayout.WebPart_DocumentWorkspace, "Right", 2, 2)]
        [DataRow(PageLayout.WebPart_ProjectSite, "Top", 1, 1)]
        [DataRow(PageLayout.WebPart_ProjectSite, "Left", 2, 1)]
        [DataRow(PageLayout.WebPart_ProjectSite, "Right", 2, 2)]
        [DataRow(PageLayout.WebPart_DocumentCenter, "Top_Left", 1, 1)]
        [DataRow(PageLayout.WebPart_DocumentCenter, "Top_Right", 1, 2)]
        [DataRow(PageLayout.WebPart_DocumentCenter, "Left", 2, 1)]
        [DataRow(PageLayout.WebPart_DocumentCenter, "Right", 2, 2)]
        [DataRow(PageLayout.WebPart_FullPageVertical, "FullPage", 1, 1)]
        [DataRow(PageLayout.WebPart_HeaderFooterThreeColumns, "Header", 1, 1)]
        [DataRow(PageLayout.WebPart_HeaderFooterThreeColumns, "MiddleColumn", 2, 2)]
        [DataRow(PageLayout.WebPart_HeaderFooterThreeColumns, "RightColumn", 2, 3)]
        [DataRow(PageLayout.WebPart_HeaderFooterThreeColumns, "Footer", 3, 1)]
        [DataRow(PageLayout.WebPart_HeaderLeftColumnBody, "Body", 2, 2)]
        [DataRow(PageLayout.WebPart_HeaderRightColumnBody, "Body", 2, 1)]
        [DataRow(PageLayout.WebPart_HeaderRightColumnBody, "RightColumn", 2, 2)]
        [DataRow(PageLayout.WebPart_HeaderFooter2Columns4Rows, "Row4", 2, 2)]
        [DataRow(PageLayout.WebPart_HeaderFooter4ColumnsTopRow, "CenterRightColumn", 2, 2)]
        [DataRow(PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns, "CenterRightColumn", 3, 3)]
        [DataRow(PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns, "TopRow", 2, 1)]
        [DataRow(PageLayout.WebPart_2010_TwoColumnsLeft, "Right", 1, 2)]
        [DataRow(PageLayout.WebPart_Custom, "Right", 1, 1)]
        public void ZonesHaveStableCoordinates(PageLayout layout, string zone, int row, int column)
        {
            using (var context = CreateContext())
            {
                var file = context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx");
                foreach (var analyzer in new WebPartPage[]
                {
                    new WebPartPage(null, file, new PageTransformation())
                })
                {
                    foreach (var id in new[] { zone, zone.ToLowerInvariant(), zone.ToUpperInvariant() })
                    {
                        Assert.AreEqual(row, analyzer.GetRow(id, layout));
                        Assert.AreEqual(column, analyzer.GetColumn(id, layout));
                    }
                    Assert.AreEqual(1, analyzer.GetRow("Unknown", layout));
                    Assert.AreEqual(1, analyzer.GetColumn("Unknown", layout));
                }
            }
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_DocumentWorkspace, CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.TwoColumnLeft)]
        [DataRow(PageLayout.WebPart_ProjectSite, CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.TwoColumn)]
        [DataRow(PageLayout.WebPart_DocumentCenter, CanvasSectionTemplate.TwoColumnLeft, CanvasSectionTemplate.TwoColumn)]
        [DataRow(PageLayout.WebPart_HeaderLeftColumnBody, CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.TwoColumnRight)]
        [DataRow(PageLayout.WebPart_HeaderRightColumnBody, CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.TwoColumnLeft)]
        public void TwoRowLayoutsPreserveSectionsEvenWithSparseContent(PageLayout layout, CanvasSectionTemplate first, CanvasSectionTemplate second)
        {
            foreach (var parts in new[]
            {
                new List<WebPartEntity>(),
                new List<WebPartEntity> { new WebPartEntity { Row = 2, Column = 2, Order = 7 } }
            })
            {
                var sections = TransformSections(layout, parts);
                CollectionAssert.AreEqual(new[] { (first, 1f), (second, 2f) }, sections);
                if (parts.Any())
                {
                    Assert.AreEqual(2, parts[0].Row);
                    Assert.AreEqual(2, parts[0].Column);
                    Assert.AreEqual(7, parts[0].Order);
                }
            }
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_FullPageVertical, CanvasSectionTemplate.OneColumn)]
        [DataRow(PageLayout.WebPart_Custom, CanvasSectionTemplate.OneColumn)]
        [DataRow(PageLayout.WebPart_2010_TwoColumnsLeft, CanvasSectionTemplate.TwoColumnLeft)]
        public void SingleRowLayoutsAreUnchanged(PageLayout layout, CanvasSectionTemplate section)
        {
            CollectionAssert.AreEqual(new[] { (section, 1f) }, TransformSections(layout, new List<WebPartEntity>()));
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_HeaderFooterThreeColumns)]
        [DataRow(PageLayout.WebPart_HeaderFooter2Columns4Rows)]
        [DataRow(PageLayout.WebPart_HeaderFooter4ColumnsTopRow)]
        public void LegacyThreeRowSectionsAreUnchanged(PageLayout layout)
        {
            CollectionAssert.AreEqual(new[]
            {
                (CanvasSectionTemplate.OneColumn, 1f), (CanvasSectionTemplate.ThreeColumn, 2f), (CanvasSectionTemplate.OneColumn, 3f)
            }, TransformSections(layout, new List<WebPartEntity>()));
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns)]
        [DataRow(PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns)]
        public void LegacyFourRowSectionsAreUnchanged(PageLayout layout)
        {
            CollectionAssert.AreEqual(new[]
            {
                (CanvasSectionTemplate.OneColumn, 1f), (CanvasSectionTemplate.OneColumn, 2f),
                (CanvasSectionTemplate.ThreeColumn, 3f), (CanvasSectionTemplate.OneColumn, 4f)
            }, TransformSections(layout, new List<WebPartEntity>()));
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_DocumentWorkspace)]
        [DataRow(PageLayout.WebPart_ProjectSite)]
        public void TemplateDescriptionIsEncodedAndPrecedesTopWithoutChangingZoneOrder(PageLayout layout)
        {
            using (var context = CreateContext())
            {
                context.Web.Description = "Research & planning <script>alert('x')</script>";
                var analyzer = new WebPartPage(null, context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx"), new PageTransformation());
                var parts = new List<WebPartEntity>
                {
                    new WebPartEntity { Row = 1, Column = 1, Order = 4, ZoneId = "Top", ZoneIndex = 4 },
                    new WebPartEntity { Row = 2, Column = 2, Order = 0, ZoneId = "Right", ZoneIndex = 0 },
                    new WebPartEntity { Row = 1, Column = 1, Order = 0, ZoneId = "Top", ZoneIndex = 0 }
                };
                analyzer.AddTemplateContent(layout, parts);
                Assert.AreEqual(4, parts.Count);
                Assert.AreEqual(WebParts.WikiText, parts[0].Type);
                Assert.AreEqual(1, parts[0].Row);
                Assert.AreEqual(1, parts[0].Column);
                Assert.AreEqual(0, parts[0].Order);
                Assert.AreEqual("<p>Research &amp; planning &lt;script&gt;alert(&#39;x&#39;)&lt;/script&gt;</p>", parts[0].Properties["Text"]);
                CollectionAssert.AreEqual(new[] { 0, 5, 0, 1 }, parts.Select(p => p.Order).ToArray());
                CollectionAssert.AreEqual(new uint[] { 4, 0, 0 }, parts.Skip(1).Select(p => p.ZoneIndex).ToArray());
                Assert.AreEqual("Right", parts[2].ZoneId);
            }
        }

        [DataTestMethod]
        [DataRow(PageLayout.WebPart_DocumentWorkspace)]
        [DataRow(PageLayout.WebPart_ProjectSite)]
        public void DescriptionsWithoutWebPartsWorkWithFileOnlyAnalyzers(PageLayout layout)
        {
            using (var context = CreateContext())
            {
                context.Web.Description = "Description only";
                var file = context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx");
                foreach (var analyzer in new WebPartPage[]
                {
                    new WebPartPage(null, file, new PageTransformation())
                })
                {
                    var parts = new List<WebPartEntity>();
                    analyzer.AddTemplateContent(layout, parts);
                    Assert.AreEqual(1, parts.Count);
                    Assert.AreEqual("<p>Description only</p>", parts[0].Properties["Text"]);
                    Assert.AreEqual(0, parts[0].Order);
                }
            }
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" \r\n ")]
        public void EmptyTemplateDescriptionsDoNotShiftContent(string description)
        {
            using (var context = CreateContext())
            {
                context.Web.Description = description;
                var analyzer = new WebPartPage(null, context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx"), new PageTransformation());
                foreach (var layout in new[] { PageLayout.WebPart_DocumentWorkspace, PageLayout.WebPart_ProjectSite })
                {
                    var parts = new List<WebPartEntity> { new WebPartEntity { Row = 1, Column = 1, Order = 0 } };
                    analyzer.AddTemplateContent(layout, parts);
                    Assert.AreEqual(1, parts.Count);
                    Assert.AreEqual(0, parts[0].Order);
                }
            }
        }

        [TestMethod]
        public void OtherTemplatesDoNotLoadOrInsertDescriptions()
        {
            using (var context = CreateContext())
            {
                var analyzer = new WebPartPage(null, context.Web.GetFileByServerRelativeUrl("/sites/source/default.aspx"), new PageTransformation());
                foreach (var layout in Enum.GetValues(typeof(PageLayout)).Cast<PageLayout>()
                    .Except(new[] { PageLayout.WebPart_DocumentWorkspace, PageLayout.WebPart_ProjectSite }))
                {
                    var parts = new List<WebPartEntity>();
                    analyzer.AddTemplateContent(layout, parts);
                    Assert.AreEqual(0, parts.Count);
                }
            }
        }

        private static (CanvasSectionTemplate, float)[] TransformSections(PageLayout layout, List<WebPartEntity> parts)
        {
            var page = new Mock<IPage>(MockBehavior.Strict);
            var sections = new List<(CanvasSectionTemplate, float)>();
            page.Setup(p => p.AddSection(It.IsAny<CanvasSectionTemplate>(), It.IsAny<float>(), It.IsAny<ZoneReflowStrategy?>()))
                .Callback<CanvasSectionTemplate, float, ZoneReflowStrategy?>((template, order, reflow) => sections.Add((template, order)));
            new LayoutTransformator(page.Object).Transform(Tuple.Create(layout, parts));
            return sections.ToArray();
        }
    }
}
