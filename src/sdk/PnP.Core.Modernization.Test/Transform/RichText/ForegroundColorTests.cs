using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class ForegroundColorTests
    {
        [TestMethod]
        public void Transform_PreservesColorsAndAutomaticTableWidths()
        {
            const string html = @"<table class='ms-rteTable-default'><tr>
                <td><span id='blue' class='ms-rteThemeForeColor-6-0'>Blue field</span></td>
                <td><font id='green' class='ms-rteForeColor-6'>Green field</font></td>
                <td></td>
            </tr></table>";
            using (var result = Parse(new HtmlTransformator(null, _ => "#00485b").Transform(html, false)))
            {
                var cells = result.QuerySelectorAll("table td");
                Assert.AreEqual(3, cells.Length);
                Assert.IsTrue(cells.All(cell => cell.GetStyle().GetWidth() == "auto"));
                AssertColor(result, "#blue", "#00485b");
                AssertColor(result, "#green", "green");
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_PreservesSchemaLegendAndFieldColors(bool usePlaceHolder)
        {
            const string html = @"<p>Fields in <span id='blue' class='ms-rteThemeForeColor-6-0 ms-rteFontSize-3'>blue</span> apply to all scenarios.</p>
                <p>Fields in <span id='green' class='ms-rteForeColor-6 ms-rteFontSize-3'>green</span> apply only to DCAT.</p>
                <table class='ms-rteTable-default'><tr><td><h3><span id='version' class='ms-rteThemeForeColor-6-0'>Version</span></h3></td></tr>
                <tr><td><h3><span id='context' class='ms-rteThemeForeColor-6-0'>ContextData</span></h3></td></tr>
                <tr><td><h3><font id='destination' class='ms-rteForeColor-6'>DestinationLocation</font></h3></td></tr>
                <tr><td><font id='explicit' color='#008000'>DestinationHash</font></td></tr></table>";
            using (var source = Parse(html))
            using (var result = Parse(new HtmlTransformator(null, shade =>
            {
                Assert.AreEqual("ContentAccent2", shade);
                return "#00485b";
            }).Transform(html, usePlaceHolder)))
            {
                AssertColor(result, "#blue", "#00485b");
                AssertColor(result, "#version", "#00485b");
                AssertColor(result, "#context", "#00485b");
                AssertColor(result, "#green", "green");
                AssertColor(result, "#destination", "green");
                AssertColor(result, "#explicit", "green");
                Assert.AreEqual(0, result.QuerySelectorAll("[class*='ms-rteForeColor'],[class*='ms-rteThemeForeColor']").Length);
                CollectionAssert.AreEqual(source.QuerySelectorAll("p, td").Select(element => element.TextContent).ToArray(),
                    result.QuerySelectorAll("p, td").Select(element => element.TextContent).ToArray());
                Assert.AreEqual(3, result.QuerySelectorAll("h4").Length);
            }
        }

        [DataTestMethod]
        [DataRow("1-0", "BackgroundOverlay")]
        [DataRow("2-0", "BodyText")]
        [DataRow("3-0", "SubtleEmphasisText")]
        [DataRow("4-0", "SubtleEmphasisBackground")]
        [DataRow("5-0", "ContentAccent1")]
        [DataRow("6-0", "ContentAccent2")]
        [DataRow("7-0", "ContentAccent3")]
        [DataRow("8-0", "ContentAccent4")]
        [DataRow("9-0", "ContentAccent5")]
        [DataRow("10-0", "ContentAccent6")]
        [DataRow("6-1", "ContentAccent2-Lightest")]
        [DataRow("6-2", "ContentAccent2-Lighter")]
        [DataRow("6-3", "ContentAccent2-Medium")]
        [DataRow("6-4", "ContentAccent2-Darker")]
        [DataRow("6-5", "ContentAccent2-Darkest")]
        public void Transform_UsesBothThemeSlotAndShade(string code, string expectedShade)
        {
            using (var result = Parse(new HtmlTransformator(null, shade =>
            {
                Assert.AreEqual(expectedShade, shade);
                return "#123456";
            }).Transform($"<span id='field' class='MS-RTETHEMEFORECOLOR-{code}'>Field</span>", false)))
            {
                AssertColor(result, "#field", "#123456");
            }
        }

        [DataTestMethod]
        [DataRow("font", "class='ms-rteForeColor-6'", "green")]
        [DataRow("font", "color='#008000'", "green")]
        [DataRow("font", "color='red' class='ms-rteForeColor-6'", "green")]
        [DataRow("font", "color='red' class='ms-rteForeColor-6' style='color:blue'", "blue")]
        [DataRow("span", "class='ms-rteForeColor-6' style='color:blue'", "blue")]
        [DataRow("span", "class='ms-rteStyle-IntenseEmphasis ms-rteForeColor-6' style='color:blue'", "blue")]
        [DataRow("span", "class='ms-rteForeColor-6' style='text-decoration:underline'", "green")]
        [DataRow("span", "class='ms-rteForeColor-6' style='text-decoration:line-through'", "green")]
        [DataRow("strong", "class='ms-rteForeColor-6' style='text-decoration:underline'", "green")]
        [DataRow("em", "class='ms-rteForeColor-6' style='text-decoration:line-through'", "green")]
        [DataRow("sup", "class='ms-rteForeColor-6'", "green")]
        [DataRow("sub", "class='ms-rteForeColor-6'", "green")]
        [DataRow("p", "class='ms-rteForeColor-6'", "green")]
        [DataRow("h3", "class='ms-rteForeColor-6'", "green")]
        [DataRow("h4", "style='color:green'", "green")]
        public void Transform_PreservesColorThroughFormatting(string tag, string attributes, string expected)
        {
            var html = $"<{tag} {attributes}><a id='field' href='/details'>Field</a></{tag}>";
            using (var result = Parse(new HtmlTransformator().Transform(html, false)))
            {
                AssertColor(result, "#field", expected);
                Assert.AreEqual("/details", result.QuerySelector("#field").GetAttribute("href"));
                Assert.AreEqual("Field", result.Body.TextContent);
            }
        }

        [TestMethod]
        public void Transform_PreservesNestedColorsAndExplicitThemeOverride()
        {
            const string html = @"<span class='ms-rteThemeForeColor-6-0' style='color:blue;text-decoration:underline'>
                <a id='blue' href='/details'>Blue</a><font class='ms-rteForeColor-6'><strong id='green'>Green</strong></font></span>";
            using (var result = Parse(new HtmlTransformator(null, _ =>
                throw new AssertFailedException("An inline override must not fetch a theme color.")).Transform(html, false)))
            {
                AssertColor(result, "#blue", "blue");
                AssertColor(result, "#green", "green");
                Assert.IsNotNull(result.QuerySelector("u"));
            }
        }

        [TestMethod]
        public void Transform_PreservesNestedFontColors()
        {
            const string html = @"<font class='ms-rteForeColor-8'><a id='blue'>Blue</a>
                <font class='ms-rteForeColor-6'><a id='green'>Green</a></font></font>";
            using (var result = Parse(new HtmlTransformator().Transform(html, false)))
            {
                AssertColor(result, "#blue", "blue");
                AssertColor(result, "#green", "green");
                Assert.AreEqual(0, result.QuerySelectorAll("[class*='ms-rteForeColor']").Length);
            }
        }

        [TestMethod]
        public void Transform_UnresolvedThemeClassIsRetainedWithWarning()
        {
            var observer = new Mock<ILogObserver>();
            using (var result = Parse(new HtmlTransformator(new[] { observer.Object })
                .Transform("<span class='ms-rteThemeForeColor-6-0'>Field</span>", false)))
            {
                Assert.IsNotNull(result.QuerySelector(".ms-rteThemeForeColor-6-0"));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("ms-rteThemeForeColor-6-0"))), Times.Once);
            }
        }

        [TestMethod]
        public void Transform_InvalidResolvedColorFailsExplicitly()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new HtmlTransformator(null, _ => "invalid")
                .Transform("<span class='ms-rteThemeForeColor-6-0'>Field</span>", false));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TextCleanup_ResolvesAndCachesSourceThemeInsteadOfTargetTheme(bool sameWeb)
        {
            const string sourceUrl = "https://contoso.sharepoint.com/sites/source";
            const string targetUrl = "https://contoso.sharepoint.com/sites/target";
            using (var source = CoreTestContext.Create(sourceUrl, (request, _) =>
            {
                StringAssert.Contains(request.RequestUri.AbsoluteUri.ToLowerInvariant(), "getthemeshadebyname");
                return CoreTestContext.Json(new { value = "FF00485B" });
            }))
            using (var target = CoreTestContext.Create(targetUrl))
            {
                var builtIn = new BuiltIn(new PageTransformationInformation(null) { SkipUrlRewrite = true },
                    sameWeb ? source : target, sameWeb ? null : source);
                using (var result = Parse(builtIn.TextCleanup("<span id='field' class='ms-rteThemeForeColor-6-0 ms-rteThemeBackColor-6-0'>Field</span>", "false")))
                {
                    AssertColor(result, "#field", "#00485b");
                    Assert.AreEqual(result.QuerySelector("#field").GetStyle().GetColor(),
                        result.QuerySelector("#field").GetStyle().GetBackgroundColor());
                }

                CoreTestContext.SetResponse(source, (_, _) =>
                    throw new AssertFailedException("The resolved theme shade must be cached."));
                using (var result = Parse(builtIn.TextCleanup("<font id='field' class='ms-rteThemeForeColor-6-0 ms-rteThemeBackColor-6-0'>Field</font>", "false")))
                {
                    AssertColor(result, "#field", "#00485b");
                    Assert.AreEqual(result.QuerySelector("#field").GetStyle().GetColor(),
                        result.QuerySelector("#field").GetStyle().GetBackgroundColor());
                }
            }
        }

        private static IHtmlDocument Parse(string html)
        {
            return new HtmlParser(new HtmlParserOptions { IsEmbedded = true },
                BrowsingContext.New(Configuration.Default.WithCss())).ParseDocument(html);
        }

        private static void AssertColor(IHtmlDocument document, string selector, string color)
        {
            var expected = document.CreateElement("span");
            expected.GetStyle().SetColor(color);
            var element = document.QuerySelector(selector);
            Assert.IsNotNull(element, selector);
            while (element != null && string.IsNullOrEmpty(element.GetStyle().GetColor()))
            {
                element = element.ParentElement;
            }
            Assert.IsNotNull(element, $"No foreground color for {selector}");
            Assert.AreEqual(expected.GetStyle().GetColor(), element.GetStyle().GetColor(), selector);
        }
    }
}
