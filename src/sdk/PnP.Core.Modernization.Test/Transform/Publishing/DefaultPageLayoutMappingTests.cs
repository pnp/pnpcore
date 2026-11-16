using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Model.SharePoint;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Publishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using PageLayout = PnP.Core.Modernization.Publishing.PageLayout;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class DefaultPageLayoutMappingTests
    {
        [TestMethod]
        public void DefaultMappingConformsToSchemaAndHasUniqueLayoutNames()
        {
            var assembly = typeof(PageLayoutManager).Assembly;
            using (var schema = assembly.GetManifestResourceStream("PnP.Core.Modernization.Publishing.pagelayoutmapping.xsd"))
            using (var mapping = assembly.GetManifestResourceStream("PnP.Core.Modernization.Publishing.pagelayoutmapping.xml"))
            using (var reader = XmlReader.Create(schema))
            {
                var schemas = new XmlSchemaSet();
                schemas.Add(Constants.PageLayoutMappingSchema, reader);
                XDocument.Load(mapping).Validate(schemas, (sender, args) => Assert.Fail(args.Message));
            }

            var layouts = new PageLayoutManager().LoadDefaultPageLayoutMappingFile().PageLayouts;
            Assert.AreEqual(layouts.Length, layouts.Select(layout => layout.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [TestMethod]
        public void ErrorLayoutIncludesBodyWithoutChangingItsHeader()
        {
            var layout = LoadLayout("ErrorLayout");
            var body = layout.WebParts.Single();

            AssertTextField(body, "PublishingPageContent", 1);
            Assert.AreEqual(PageLayoutPageHeader.Custom, layout.PageHeader);
            Assert.AreEqual(HeaderType.FullWidthImage, layout.Header.Type);
            Assert.IsFalse(layout.Header.ShowPublishedDate);
            CollectionAssert.AreEqual(
                new[] { "PublishingRollupImage", "ArticleByLine", "PublishingContact" },
                layout.Header.Field.Select(field => field.Name).ToArray());
        }

        [TestMethod]
        public void ArticleLinksIncludesImageAndCaptionBeforeBodyAndRetainsRelatedLinks()
        {
            var layout = LoadLayout("ArticleLinks");
            Assert.AreEqual(3, layout.WebParts.Length);
            var image = layout.WebParts.Single(field => field.Name == "PublishingPageImage");
            var body = layout.WebParts.Single(field => field.Name == "PublishingPageContent");
            var links = layout.WebParts.Single(field => field.Name == "SummaryLinks");

            Assert.AreEqual(WebParts.WikiImage, image.TargetWebPart);
            Assert.AreEqual(1, image.Row);
            Assert.AreEqual(1, image.Column);
            Assert.AreEqual(1, image.Order);
            Assert.IsTrue(image.OrderSpecified);
            AssertTextField(body, "PublishingPageContent", 1);
            Assert.AreEqual(2, body.Order);
            Assert.IsTrue(body.OrderSpecified);
            Assert.AreEqual(1, links.Row);
            Assert.AreEqual(2, links.Column);
            Assert.AreEqual(PageLayoutPageHeader.Custom, layout.PageHeader);
            Assert.IsTrue(layout.Header.ShowPublishedDate);

            CollectionAssert.AreEqual(
                new[]
                {
                    "ImageUrl=ToImageUrl({PublishingPageImage})",
                    "AlternativeText=ToImageAltText({PublishingPageImage})",
                    "Anchor=ToImageAnchor({PublishingPageImage})",
                    "Caption=ToImageCaption({PublishingImageCaption})"
                },
                image.Property.Select(property => $"{property.Name}={property.Functions}").ToArray());

            var page = new Mock<IPage>();
            var parts = layout.WebParts.Select(ToEntity).ToList();
            new PublishingLayoutTransformator(page.Object, layout).Transform(
                Tuple.Create(Pages.PageLayout.PublishingPage_AutoDetect, parts));
            CollectionAssert.AreEqual(new[] { CanvasSectionTemplate.TwoColumn }, AddedSections(page));
        }

        [DataTestMethod]
        [DataRow("RollupPage", false)]
        [DataRow("CuratedRollupPage", true)]
        public void RollupMappingsPreserveZonesAndInterleaveCuratedContent(string name, bool curated)
        {
            var layout = LoadLayout(name);
            Assert.AreEqual(PageLayoutPageLayoutTemplate.AutoDetect, layout.PageLayoutTemplate);
            CollectionAssert.AreEqual(
                new[]
                {
                    "TopLeft:1:1", "TopRight:1:2", "Body:2:1",
                    $"BottomLeft:{(curated ? 4 : 3)}:1",
                    $"BottomCenter:{(curated ? 4 : 3)}:2",
                    $"BottomRight:{(curated ? 4 : 3)}:3"
                },
                layout.WebPartZones.Select(zone => $"{zone.ZoneId}:{zone.Row}:{zone.Column}").ToArray());
            CollectionAssert.Contains(Constants.OobPublishingPageLayouts, name);

            var parts = layout.WebPartZones.Select(zone => new WebPartEntity
            {
                Type = "ZoneContent",
                ZoneId = zone.ZoneId,
                Row = zone.Row,
                Column = zone.Column,
                Order = 1
            }).ToList();
            if (curated)
            {
                var body = layout.WebParts.Single();
                AssertTextField(body, "PublishingPageContent", 3);
                parts.Add(ToEntity(body));
            }
            else
            {
                Assert.IsTrue(layout.WebParts == null || layout.WebParts.Length == 0);
            }

            var page = new Mock<IPage>();
            new PublishingLayoutTransformator(page.Object, layout).Transform(
                Tuple.Create(Pages.PageLayout.PublishingPage_AutoDetect, parts));
            var expected = new List<CanvasSectionTemplate>
            {
                CanvasSectionTemplate.TwoColumn,
                CanvasSectionTemplate.OneColumn
            };
            if (curated)
            {
                expected.Add(CanvasSectionTemplate.OneColumn);
            }
            expected.Add(CanvasSectionTemplate.ThreeColumn);
            CollectionAssert.AreEqual(expected.ToArray(), AddedSections(page));
        }

        [TestMethod]
        public void CuratedContentRetainsItsPositionWhenTheBodyZoneIsEmpty()
        {
            var layout = LoadLayout("CuratedRollupPage");
            var parts = new List<WebPartEntity>
            {
                new WebPartEntity { Type = "ZoneContent", Row = 1, Column = 2 },
                ToEntity(layout.WebParts.Single()),
                new WebPartEntity { Type = "ZoneContent", Row = 4, Column = 3 }
            };
            var page = new Mock<IPage>();

            new PublishingLayoutTransformator(page.Object, layout).Transform(
                Tuple.Create(Pages.PageLayout.PublishingPage_AutoDetect, parts));

            CollectionAssert.AreEqual(
                new[] { CanvasSectionTemplate.TwoColumn, CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.ThreeColumn },
                AddedSections(page));
            CollectionAssert.AreEqual(
                new[] { 1f, 2f, 3f },
                page.Invocations.Where(call => call.Method.Name == nameof(IPage.AddSection))
                    .Select(call => (float)call.Arguments[1]).ToArray());
        }

        [DataTestMethod]
        [DataRow("ErrorLayout")]
        [DataRow("ArticleLinks")]
        [DataRow("RollupPage")]
        [DataRow("CuratedRollupPage")]
        public void CustomMappingsStillOverrideBuiltInMappings(string name)
        {
            var manager = new PageLayoutManager();
            var defaults = manager.LoadDefaultPageLayoutMappingFile();
            var replacement = new PageLayout { Name = name, PageLayoutTemplate = PageLayoutPageLayoutTemplate.OneColumn };

            var merged = manager.MergePageLayoutMappingFiles(defaults, new PublishingPageTransformation
            {
                PageLayouts = new[] { replacement }
            });

            Assert.AreEqual(defaults.PageLayouts.Length, merged.PageLayouts.Length);
            Assert.AreSame(replacement, merged.PageLayouts.Single(layout => layout.Name == name));
            Assert.AreSame(defaults.PageLayouts.Single(layout => layout.Name == "ArticleRight"),
                merged.PageLayouts.Single(layout => layout.Name == "ArticleRight"));
        }

        private static PageLayout LoadLayout(string name) =>
            new PageLayoutManager().LoadDefaultPageLayoutMappingFile().PageLayouts.Single(layout => layout.Name == name);

        private static void AssertTextField(WebPartField field, string name, int row)
        {
            Assert.AreEqual(name, field.Name);
            Assert.AreEqual(WebParts.WikiText, field.TargetWebPart);
            Assert.AreEqual(row, field.Row);
            Assert.AreEqual(1, field.Column);
            Assert.AreEqual("Text", field.Property.Single().Name);
            Assert.IsTrue(string.IsNullOrEmpty(field.Property.Single().Functions));
        }

        private static WebPartEntity ToEntity(WebPartField field) => new WebPartEntity
        {
            Type = field.TargetWebPart,
            Row = field.Row,
            Column = field.Column,
            Order = field.Order
        };

        private static CanvasSectionTemplate[] AddedSections(Mock<IPage> page) =>
            page.Invocations.Where(call => call.Method.Name == nameof(IPage.AddSection))
                .Select(call => (CanvasSectionTemplate)call.Arguments[0]).ToArray();
    }
}
