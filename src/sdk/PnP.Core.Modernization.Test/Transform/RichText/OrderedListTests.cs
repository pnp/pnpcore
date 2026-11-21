using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using System.Linq;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class OrderedListTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_PreservesDecimalStepsInClassicNestedLists(bool usePlaceHolder)
        {
            const string html = "<ul><ul><ul><ol>" +
                "<li>First <strong>step</strong></li><li>Second step</li><li>Third step</li>" +
                "<ol><li>Nested step</li><li><a href='#details'>Details</a><br>Next line</li>" +
                "<ol><li>Deep step</li></ol><ol><li>Restarted step</li></ol></ol>" +
                "</ol></ul></ul></ul>";

            using (var source = Parse(html))
            using (var result = Parse(new HtmlTransformator().Transform(html, usePlaceHolder)))
            {
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                CollectionAssert.AreEqual(
                    source.QuerySelectorAll("ol, ul, li").Select(element => element.TagName).ToArray(),
                    result.QuerySelectorAll("ol, ul, li").Select(element => element.TagName).ToArray());
                Assert.IsTrue(result.QuerySelectorAll("ol, ul").All(list => list.ClassList.Contains("customListStyle")));
                Assert.IsTrue(result.QuerySelectorAll("ol").All(list =>
                    list.GetStyle().GetPropertyValue("list-style-type") == "decimal"));
                Assert.AreEqual("step", result.QuerySelector("strong").TextContent);
                Assert.AreEqual("#details", result.QuerySelector("a").GetAttribute("href"));
                Assert.AreEqual(1, result.QuerySelectorAll("br").Length);
            }
        }

        [DataTestMethod]
        [DataRow("", "decimal")]
        [DataRow("type='1'", "decimal")]
        [DataRow("type='a'", "lower-alpha")]
        [DataRow("type='A'", "upper-alpha")]
        [DataRow("type='i'", "lower-roman")]
        [DataRow("type='I'", "upper-roman")]
        [DataRow("type='invalid'", "decimal")]
        [DataRow("style='list-style-type:lower-roman'", "lower-roman")]
        [DataRow("type='A' style='list-style-type:decimal'", "decimal")]
        [DataRow("type='a' style='list-style:upper-roman inside'", "upper-roman")]
        [DataRow("style='list-style-type:none'", "none")]
        public void Transform_PreservesAuthoredOrderedListStyles(string attributes, string marker)
        {
            using (var result = Parse(new HtmlTransformator().Transform(
                $"<ol {attributes}><li>First<ol><li>Nested</li></ol></li><li>Second</li></ol>", false)))
            {
                var lists = result.QuerySelectorAll("ol");
                Assert.AreEqual(marker, lists[0].GetStyle().GetPropertyValue("list-style-type"));
                Assert.AreEqual("decimal", lists[1].GetStyle().GetPropertyValue("list-style-type"));
                Assert.IsTrue(lists.All(list => list.ClassList.Contains("customListStyle")));
            }
        }

        [TestMethod]
        public void Transform_PreservesItemOverridesAndNumberingAttributes()
        {
            const string html = "<ol type='A' start='7' reversed class='source-list' " +
                "style='margin-left:40px;text-align:right;position:absolute'>" +
                "<li value='10' type='i'>Roman</li>" +
                "<li type='A' style='list-style-type:decimal;color:red'>Decimal</li>" +
                "<li style='list-style:lower-alpha inside'>Alpha</li><li>Inherited</li></ol>";
            using (var result = Parse(new HtmlTransformator().Transform(html, false)))
            {
                var list = result.QuerySelector("ol");
                Assert.AreEqual("7", list.GetAttribute("start"));
                Assert.IsTrue(list.HasAttribute("reversed"));
                Assert.IsTrue(list.ClassList.Contains("source-list"));
                Assert.IsTrue(list.ClassList.Contains("customListStyle"));
                Assert.AreEqual("40px", list.GetStyle().GetMarginLeft());
                Assert.AreEqual("right", list.GetStyle().GetTextAlign());
                Assert.AreEqual("", list.GetStyle().GetPropertyValue("position"));
                var items = list.Children;
                Assert.AreEqual("10", items[0].GetAttribute("value"));
                Assert.AreEqual("lower-roman", items[0].GetStyle().GetPropertyValue("list-style-type"));
                Assert.AreEqual("decimal", items[1].GetStyle().GetPropertyValue("list-style-type"));
                Assert.IsFalse(string.IsNullOrEmpty(items[1].GetStyle().GetColor()));
                Assert.AreEqual("lower-alpha", items[2].GetStyle().GetPropertyValue("list-style-type"));
                Assert.AreEqual("", items[3].GetStyle().GetPropertyValue("list-style-type"));
            }
        }

        [TestMethod]
        public void Transform_PreservesMixedListsInsideTablesAndIndentation()
        {
            const string html = "<blockquote style='margin:0px 0px 0px 40px;'>" +
                "<table><tr><td><ul style='list-style-type:square'><li>Bullet" +
                "<ol><li>Number<ul><li>Nested bullet</li></ul></li></ol>" +
                "</li></ul></td></tr></table></blockquote>";
            var transformator = new HtmlTransformator();
            using (var result = Parse(transformator.Transform(html, false)))
            {
                Assert.AreEqual("square", result.QuerySelector("ul").GetStyle().GetPropertyValue("list-style-type"));
                Assert.AreEqual("decimal", result.QuerySelector("ol").GetStyle().GetPropertyValue("list-style-type"));
                Assert.IsTrue(result.QuerySelector("ol").ClassList.Contains("customListStyle"));
                Assert.IsTrue(result.QuerySelector("ul").ClassList.Contains("customListStyle"));
                Assert.IsFalse(result.QuerySelector("ol ul").ClassList.Contains("customListStyle"));
                Assert.AreEqual(2, result.QuerySelectorAll("ul").Length);
                Assert.AreEqual(3, result.QuerySelectorAll("li").Length);
                var once = result.QuerySelector("ul").OuterHtml;
                Assert.AreEqual(once, transformator.Transform(once, false));
            }
        }

        [TestMethod]
        public void Transform_DoesNotChangeBulletOnlyListMarkup()
        {
            const string html = "<ul><li>First<ul><li>Nested</li></ul></li><li>Second</li></ul>";
            Assert.AreEqual(html, new HtmlTransformator().Transform(html, false));
        }

        private static IDocument Parse(string html)
        {
            return new HtmlParser(new HtmlParserOptions(),
                BrowsingContext.New(Configuration.Default.WithCss())).ParseDocument(html);
        }
    }
}
