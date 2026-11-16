using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class StrikethroughTests
    {
        [DataTestMethod]
        [DataRow("h1", "h2")]
        [DataRow("h2", "h3")]
        [DataRow("h3", "h4")]
        [DataRow("h4", "div")]
        [DataRow("h5", "div")]
        [DataRow("h6", "div")]
        public void Transform_PreservesStruckHeadings(string sourceTag, string targetTag)
        {
            var html = $"<{sourceTag} id='legacy' style='text-decoration:line-through'>" +
                "Retired <a href='/guide'><em>setup</em></a></" + sourceTag + "><p>Current setup</p>";
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var heading = result.QuerySelector(targetTag);
                Assert.IsNotNull(heading);
                Assert.AreEqual("Retired setup", heading.TextContent);
                AssertDecoration(heading, "s", true);
                AssertDecoration(result.QuerySelector("p"), "s", false);
                Assert.AreEqual(1, result.QuerySelectorAll("#legacy").Length);
                Assert.AreEqual("/guide", result.QuerySelector("a").GetAttribute("href"));
                Assert.AreEqual("setup", result.QuerySelector("em").TextContent);
            }
        }

        [DataTestMethod]
        [DataRow("ul", "<li>Old first step</li><li>Old <a href='/guide'>second step</a><ol><li>Nested step</li></ol></li>")]
        [DataRow("ol", "<li>Old first step</li><li><p>Old second step</p><ul><li>Nested step</li></ul></li>")]
        [DataRow("div", "<p>Retired instructions</p><ul><li>Old step</li></ul>Trailing text")]
        [DataRow("h2", "<p>Retired instructions</p>Trailing text")]
        [DataRow("p", "Retired <strong>instructions</strong>")]
        [DataRow("blockquote", "<p>Retired instructions</p><ul><li>Old step</li></ul>")]
        [DataRow("table", "<tbody><tr><td>Old cell</td><td><p>Old paragraph</p></td></tr></tbody>")]
        public void Transform_PreservesDecoratedContainersWithoutChangingStructure(string tag, string content)
        {
            var html = $"<{tag} style='text-decoration:line-through'>{content}</{tag}>" +
                "<p id='current'>Current instructions</p>";
            using (var source = new HtmlParser().ParseDocument(html))
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var container = result.QuerySelector(tag == "h2" ? "h3" : tag);
                Assert.IsNotNull(container);
                AssertDecoration(container, "s", true);
                AssertDecoration(result.QuerySelector("#current"), "s", false);
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                CollectionAssert.AreEqual(
                    source.QuerySelectorAll("ul, ol, li, p, a").Select(element => element.TagName + ":" + element.TextContent).ToArray(),
                    result.QuerySelectorAll("ul, ol, li, p, a").Select(element => element.TagName + ":" + element.TextContent).ToArray());
                Assert.AreEqual(0, result.QuerySelectorAll("s ul, s ol, s li, s p, s h3, s table").Length);
            }
        }

        [DataTestMethod]
        [DataRow("text-decoration:line-through", false)]
        [DataRow("text-decoration-line:line-through", false)]
        [DataRow("TEXT-DECORATION:LINE-THROUGH", false)]
        [DataRow("text-decoration:underline line-through", true)]
        [DataRow("text-decoration:line-through underline", true)]
        [DataRow("text-decoration-line:underline line-through", true)]
        [DataRow("text-decoration:line-through solid red", false)]
        [DataRow("text-decoration:underline line-through solid red", true)]
        [DataRow("text-decoration:underline; text-decoration-line:line-through", false)]
        public void Transform_PreservesCombinedDecorationTokens(string style, bool underlined)
        {
            foreach (var tag in new[] { "h2", "li", "span", "a", "strong", "em", "sup", "sub" })
            {
                var content = $"<{tag} style='{style}'>Retired <span>instructions</span></{tag}>";
                var html = tag == "li" ? "<ul>" + content + "</ul>" : content;
                using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
                {
                    Assert.AreEqual("Retired instructions", result.Body.TextContent);
                    AssertDecoration(result.Body, "s", true);
                    AssertDecoration(result.Body, "u", underlined);
                }
            }
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("text-decoration:none")]
        [DataRow("text-decoration:underline")]
        [DataRow("text-decoration:overline")]
        [DataRow("text-decoration:line-through; text-decoration-line:none")]
        [DataRow("text-decoration-line:line-through; text-decoration:none")]
        public void Transform_DoesNotStrikeUndecoratedText(string style)
        {
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(
                $"<div style='{style}'><h2>Current heading</h2><ul><li>Current step</li></ul></div>", false)))
            {
                Assert.AreEqual(0, result.QuerySelectorAll("s").Length);
                Assert.AreEqual("Current headingCurrent step", result.Body.TextContent);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_PreservesNestedAndExistingStrikesWithoutLeakingToSiblings(bool usePlaceHolder)
        {
            const string html = "<h2>Current heading</h2><ul><li>Current step</li>" +
                "<li style='text-decoration:line-through'>Retired &lt;step&gt; &amp; " +
                "<a href='/guide?q=1&amp;x=2'>linked <em>guidance</em></a>" +
                "<ul style='text-decoration:line-through'><li style='text-decoration:none'>Still retired</li></ul>" +
                "<s>Existing strike</s><del>Deleted text</del><strike>Legacy strike</strike>" +
                "<span style='text-decoration:line-through'>Inline strike</span></li><li>Current last step</li></ul>";
            using (var source = new HtmlParser().ParseDocument(html))
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, usePlaceHolder)))
            {
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                AssertDecoration(result.QuerySelector("h3"), "s, del, strike", false);
                var items = result.QuerySelectorAll("body > ul > li");
                Assert.AreEqual(3, items.Length);
                AssertDecoration(items[0], "s, del, strike", false);
                AssertDecoration(items[1], "s, del, strike", true);
                AssertDecoration(items[2], "s, del, strike", false);
                Assert.AreEqual(0, result.QuerySelectorAll("s s, del s, strike s").Length);
                Assert.AreEqual("/guide?q=1&x=2", result.QuerySelector("a").GetAttribute("href"));
            }
        }

        [TestMethod]
        public void Transform_PreservesMixedDecorationAndNestedIndentation()
        {
            const string html = "<blockquote style='margin:0px 0px 0px 40px;text-decoration:underline line-through'>" +
                "<blockquote style='margin:0px 0px 0px 40px'>Retired answer <a href='/guide'>details</a></blockquote></blockquote>";
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                Assert.AreEqual("Retired answer details", result.Body.TextContent);
                Assert.AreEqual(0, result.QuerySelectorAll("blockquote").Length);
                Assert.IsTrue(result.QuerySelectorAll("p").All(element =>
                    element.GetAttribute("style").Contains("margin-left: 80px")));
                AssertDecoration(result.Body, "s", true);
                AssertDecoration(result.Body, "u", true);
            }
        }

        [TestMethod]
        public void Transform_PreservesStrikeAlongsideClassicEmphasisAndColors()
        {
            const string html = "<span class='ms-rteStyle-IntenseEmphasis ms-rteForeColor-6' " +
                "style='text-decoration:line-through'>Retired instructions</span>";
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                AssertDecoration(result.Body, "s", true);
                AssertDecoration(result.Body, "em", true);
                AssertDecoration(result.Body, "u", true);
                Assert.IsNotNull(result.QuerySelector(".fontColorGreen"));
            }
        }

        [TestMethod]
        public void Transform_DoesNotInsertFormattingIntoRawTextElements()
        {
            const string html = "<div style='text-decoration:line-through'>Retired<!-- note --><br>" +
                "<script type='application/json'>{\"value\":\"<raw>\"}</script>" +
                "<style>.sample { color: red; }</style><textarea>Raw &lt;text&gt;</textarea></div>";
            using (var source = new HtmlParser().ParseDocument(html))
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                CollectionAssert.AreEqual(
                    source.QuerySelectorAll("script, style, textarea").Select(element => element.InnerHtml).ToArray(),
                    result.QuerySelectorAll("script, style, textarea").Select(element => element.InnerHtml).ToArray());
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                Assert.AreEqual(1, result.QuerySelectorAll("br").Length);
                Assert.AreEqual("Retired", result.QuerySelector("s").TextContent);
            }
        }

        [DataTestMethod]
        [DataRow(false, "false")]
        [DataRow(false, "true")]
        [DataRow(true, "false")]
        [DataRow(true, "true")]
        public void TextCleanup_PreservesStrikeForWikiAndPublishingPages(bool publishing, string usePlaceHolders)
        {
            const string html = "<h2 style='text-decoration:line-through'>Retired heading</h2>" +
                "<ul style='text-decoration-line:underline line-through'><li>Retired step</li></ul>" +
                "<p>Current guidance</p>";
            const string siteUrl = "https://example.org/sites/source";
            var provider = new MockEntryResponseProvider();
            foreach (var property in new[] { "Site", "Web" })
            {
                provider.ResponseEntries.Add(new MockResponseEntry<object>
                {
                    Url = siteUrl,
                    PropertyName = property,
                    ReturnValue = new Dictionary<string, object>
                    {
                        ["_ObjectType_"] = "SP." + property,
                        ["Url"] = siteUrl,
                        ["ServerRelativeUrl"] = "/sites/source"
                    }
                });
            }
            using (var context = new ClientContext(siteUrl)
            {
                WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider)
            })
            {
                BaseTransformationInformation information = publishing
                    ? new PublishingPageTransformationInformation(null)
                    : new PageTransformationInformation(null);
                information.SkipUrlRewrite = true;
                var builtIn = new BuiltIn(information, context);
                context.ExecutingWebRequest += (_, __) =>
                    throw new AssertFailedException("HTML cleanup must not access a tenant.");
                using (var result = new HtmlParser().ParseDocument(builtIn.TextCleanup(html, usePlaceHolders)))
                {
                    AssertDecoration(result.QuerySelector("h3"), "s", true);
                    AssertDecoration(result.QuerySelector("li"), "s", true);
                    AssertDecoration(result.QuerySelector("li"), "u", true);
                    AssertDecoration(result.QuerySelector("p"), "s", false);
                    Assert.AreEqual("Retired headingRetired stepCurrent guidance", result.Body.TextContent);
                }
            }
        }

        private static void AssertDecoration(INode node, string selector, bool expected)
        {
            foreach (var text in TextNodes(node).Where(text => !string.IsNullOrWhiteSpace(text.TextContent)))
            {
                Assert.AreEqual(expected, text.ParentElement.Closest(selector) != null,
                    $"Unexpected {selector} decoration for '{text.TextContent}' in {node.TextContent}");
            }
        }

        private static IEnumerable<IText> TextNodes(INode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child is IText text)
                {
                    yield return text;
                }
                else
                {
                    foreach (var descendant in TextNodes(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
}
