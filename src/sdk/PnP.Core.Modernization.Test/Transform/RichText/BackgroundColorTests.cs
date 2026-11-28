using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class BackgroundColorTests
    {
        private const string MaskClasses = "ms-rteThemeBackColor-2-5 ms-rteThemeForeColor-2-5";

        [DataTestMethod]
        [DataRow("span", "")]
        [DataRow("span", "style='text-decoration:underline'")]
        [DataRow("span", "style='text-decoration:line-through'")]
        [DataRow("font", "")]
        [DataRow("font", "style='text-decoration:underline'")]
        [DataRow("strong", "")]
        [DataRow("strong", "style='text-decoration:underline'")]
        [DataRow("em", "style='text-decoration:line-through'")]
        [DataRow("sup", "style='text-decoration:underline'")]
        [DataRow("sub", "style='text-decoration:line-through'")]
        [DataRow("p", "")]
        [DataRow("p", "style='text-decoration:underline'")]
        [DataRow("h1", "")]
        [DataRow("h2", "")]
        [DataRow("h3", "")]
        [DataRow("h4", "")]
        [DataRow("h5", "")]
        [DataRow("h6", "")]
        public void Transform_PreservesMaskThroughFormatting(string tag, string attributes)
        {
            var html = $"<{tag} class='{MaskClasses}' {attributes}><a id='content' href='/details'>Obsolete content</a></{tag}>";
            using (var result = Parse(CreateTransformator().Transform(html, false)))
            {
                AssertColors(result, "#content", "#1a1a1a", "#1a1a1a");
                Assert.AreEqual("/details", result.QuerySelector("#content").GetAttribute("href"));
                Assert.AreEqual("Obsolete content", result.Body.TextContent);
                Assert.AreEqual(0, result.QuerySelectorAll("[class*='ms-rteTheme'],[data-pnp-theme-background-color]").Length);
                if (attributes.Contains("underline"))
                {
                    Assert.IsNotNull(result.QuerySelector("u"));
                }
                if (attributes.Contains("line-through"))
                {
                    Assert.IsNotNull(result.QuerySelector("s"));
                }
            }
        }

        [DataTestMethod]
        [DataRow("Quote")]
        [DataRow("IntenseQuote")]
        [DataRow("Emphasis")]
        [DataRow("IntenseEmphasis")]
        public void Transform_PreservesMaskThroughClassicTextStyles(string style)
        {
            var html = $"<span class='{MaskClasses} ms-rteStyle-{style}'><a id='content'>Obsolete</a></span>";
            using (var result = Parse(CreateTransformator().Transform(html, false)))
            {
                AssertColors(result, "#content", "#1a1a1a", "#1a1a1a");
                Assert.IsNotNull(result.QuerySelector("em"));
            }
        }

        [DataTestMethod]
        [DataRow("1-0", "BackgroundOverlay")]
        [DataRow("2-5", "BodyText-Darkest")]
        [DataRow("3-0", "SubtleEmphasisText")]
        [DataRow("4-0", "SubtleEmphasisBackground")]
        [DataRow("5-0", "ContentAccent1")]
        [DataRow("6-1", "ContentAccent2-Lightest")]
        [DataRow("7-2", "ContentAccent3-Lighter")]
        [DataRow("8-3", "ContentAccent4-Medium")]
        [DataRow("9-4", "ContentAccent5-Darker")]
        [DataRow("10-0", "ContentAccent6")]
        public void Transform_ResolvesBackgroundSlotAndShade(string code, string expectedShade)
        {
            var transformator = new HtmlTransformator(null, shade =>
            {
                Assert.AreEqual(expectedShade, shade);
                return "#123456";
            });
            using (var result = Parse(transformator.Transform($"<span class='MS-RTETHEMEBACKCOLOR-{code}'>Highlight</span>", false)))
            {
                AssertColors(result, "span", null, "#123456");
            }
        }

        [DataTestMethod]
        [DataRow("Highlight")]
        [DataRow("<strong>Highlight</strong>")]
        [DataRow("<span>Highlight</span>")]
        public void Transform_KeepsBackgroundOnlySpans(string content)
        {
            using (var result = Parse(CreateTransformator().Transform(
                $"<span class='ms-rteThemeBackColor-2-5'>{content}</span>", false)))
            {
                AssertColors(result, "span", null, "#1a1a1a");
                Assert.AreEqual("Highlight", result.Body.TextContent);
                Assert.AreEqual(0, result.QuerySelectorAll("[data-pnp-theme-background-color]").Length);
            }
        }

        [DataTestMethod]
        [DataRow("span", "color:red;background-color:red", "red")]
        [DataRow("span", "color:red;background:red", "red")]
        [DataRow("h1", "color:red;background-color:red", "red")]
        [DataRow("font", "color:red;background-color:red;text-decoration:underline", "red")]
        [DataRow("strong", "color:red;background-color:red;text-decoration:line-through", "red")]
        [DataRow("span", "color:red;background-color:transparent", "transparent")]
        public void Transform_InlineBackgroundOverridesThemeWithoutResolving(string tag, string style, string background)
        {
            var transformator = new HtmlTransformator(null, _ =>
                throw new AssertFailedException("Inline overrides must not resolve the theme."));
            using (var result = Parse(transformator.Transform(
                $"<{tag} class='{MaskClasses}' style='{style}'><a id='content'>Content</a></{tag}>", false)))
            {
                AssertColors(result, "#content", "red", background);
            }
        }

        [DataTestMethod]
        [DataRow("span")]
        [DataRow("font")]
        [DataRow("p")]
        [DataRow("h1")]
        public void Transform_PreservesInlineMaskWithoutThemeClasses(string tag)
        {
            using (var result = Parse(new HtmlTransformator().Transform(
                $"<{tag} style='color:#1a1a1a;background-color:#1a1a1a'><a id='content'>Obsolete</a></{tag}>", false)))
            {
                AssertColors(result, "#content", "#1a1a1a", "#1a1a1a");
            }
        }

        [TestMethod]
        public void Transform_PreservesNestedBackgroundOverrides()
        {
            var html = $"<span class='{MaskClasses}'><a id='outer'>Masked</a>" +
                "<span style='background-color:yellow;color:red'><a id='inner'>Visible</a></span></span>";
            using (var result = Parse(CreateTransformator().Transform(html, false)))
            {
                AssertColors(result, "#outer", "#1a1a1a", "#1a1a1a");
                AssertColors(result, "#inner", "red", "yellow");
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_PreservesMaskInHeadingsTablesAndIndentation(bool usePlaceHolder)
        {
            var html = $"<h1><span class='{MaskClasses}'><a id='heading'>Introduction</a></span></h1>" +
                $"<table><tr><td><span class='{MaskClasses}'><a id='cell'>Obsolete instructions</a></span></td></tr></table>" +
                $"<blockquote style='margin:0px 0px 0px 40px;'><span class='{MaskClasses}'><a id='indent'>Obsolete answer</a></span></blockquote>";
            using (var result = Parse(CreateTransformator().Transform(html, usePlaceHolder)))
            {
                foreach (var id in new[] { "heading", "cell", "indent" })
                {
                    AssertColors(result, "#" + id, "#1a1a1a", "#1a1a1a");
                }
                Assert.IsNotNull(result.QuerySelector("h2"));
                Assert.IsNotNull(result.QuerySelector("table td"));
                Assert.AreEqual(0, result.QuerySelectorAll("blockquote").Length);
            }
        }

        [DataTestMethod]
        [DataRow("ms-rteThemeBackColor-2-5")]
        [DataRow("ms-rteThemeBackColor-11-7")]
        public void Transform_UnresolvedBackgroundRetainsClassAndWarns(string className)
        {
            var observer = new Mock<ILogObserver>();
            using (var result = Parse(new HtmlTransformator(new[] { observer.Object }).Transform(
                $"<span class='{className}'>Highlight</span>", false)))
            {
                Assert.IsNotNull(result.QuerySelector("." + className));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains(className))), Times.Once);
            }
        }

        [TestMethod]
        public void Transform_InvalidResolvedBackgroundFailsExplicitly()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new HtmlTransformator(null, _ => "invalid")
                .Transform("<span class='ms-rteThemeBackColor-2-5'>Highlight</span>", false));
        }

        private static HtmlTransformator CreateTransformator()
        {
            return new HtmlTransformator(null, _ => "#1a1a1a");
        }

        private static IHtmlDocument Parse(string html)
        {
            return new HtmlParser(new HtmlParserOptions { IsEmbedded = true },
                BrowsingContext.New(Configuration.Default.WithCss())).ParseDocument(html);
        }

        private static void AssertColors(IHtmlDocument document, string selector, string foreground, string background)
        {
            var expected = document.CreateElement("span");
            expected.GetStyle().SetColor(foreground);
            expected.GetStyle().SetBackgroundColor(background);
            var element = document.QuerySelector(selector);
            Assert.IsNotNull(element, selector);
            if (foreground != null)
            {
                Assert.AreEqual(expected.GetStyle().GetColor(), FindColor(element, false), selector + " foreground");
            }
            Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), FindColor(element, true), selector + " background");
        }

        private static string FindColor(IElement element, bool background)
        {
            while (element != null)
            {
                var color = background ? element.GetStyle().GetBackgroundColor() : element.GetStyle().GetColor();
                if (!string.IsNullOrEmpty(color))
                {
                    return color;
                }
                element = element.ParentElement;
            }
            return null;
        }
    }
}
