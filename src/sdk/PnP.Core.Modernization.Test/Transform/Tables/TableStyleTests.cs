using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Linq;

namespace PnP.Core.Modernization.Tests.Transform.Tables
{
    [TestClass]
    [TestCategory("Offline")]
    public class TableStyleTests
    {
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(10)]
        public void Transform_HeaderlessTableDoesNotFillFirstContentRow(int styleCode)
        {
            var html = $@"<table class='ms-rteTable-{styleCode}' cellspacing='0'>
                <tr class='ms-rteTableEvenRow-{styleCode}'>
                    <td class='ms-rteTableEvenCol-{styleCode}'>
                        <h2><span class='ms-rteForeColor-2'>Security requirements</span></h2>
                        <p>Read the <a href='/details'>instructions</a>.</p>
                    </td>
                </tr>
            </table>";

            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var table = document.QuerySelector("table");
                Assert.AreEqual("simpleTableStyleNeutral", table.ClassName);
                Assert.AreEqual(1, table.QuerySelectorAll("tr").Length);
                Assert.AreEqual("Security requirements", table.QuerySelector("h3 .fontColorRed").TextContent);
                Assert.AreEqual("/details", table.QuerySelector("a").GetAttribute("href"));
                Assert.AreEqual("Read the instructions.", table.QuerySelector("p").TextContent);
            }
        }

        [DataTestMethod]
        [DataRow(1, "bandedRowTableStyleNeutral")]
        [DataRow(2, "filledHeaderTableStyleNeutral")]
        [DataRow(4, "filledHeaderTableStyleTheme")]
        [DataRow(5, "filledHeaderTableStyleTheme")]
        [DataRow(6, "bandedRowTableStyleTheme")]
        [DataRow(7, "filledHeaderTableStyleTheme")]
        [DataRow(8, "filledHeaderTableStyleTheme")]
        [DataRow(9, "filledHeaderTableStyleTheme")]
        [DataRow(10, "filledHeaderTableStyleTheme")]
        public void Transform_PreservesStyleForActualHeader(int styleCode, string expectedStyle)
        {
            var html = $@"<table class='ms-rteTable-{styleCode}'>
                <thead><tr><th>Heading</th><th>Details</th></tr></thead>
                <tbody><tr><td>First</td><td>Second</td></tr></tbody>
            </table>";

            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var table = document.QuerySelector("table");
                Assert.AreEqual(expectedStyle, table.ClassName);
                Assert.AreEqual(2, table.QuerySelectorAll("tr").Length);
                CollectionAssert.AreEqual(new[] { "Heading", "Details" },
                    table.QuerySelectorAll("tr:first-child strong").Select(element => element.TextContent).ToArray());
                CollectionAssert.AreEqual(new[] { "First", "Second" },
                    table.QuerySelectorAll("tr:last-child td").Select(element => element.TextContent).ToArray());
            }
        }

        [DataTestMethod]
        [DataRow("<tr><td>First</td><td>Second</td></tr><tr><td>Third</td><td>Fourth</td></tr>")]
        [DataRow("<tr><th>First</th><td>Second</td></tr><tr><th>Third</th><td>Fourth</td></tr>")]
        [DataRow("<tr><td rowspan='2'>First</td><td>Second</td></tr><tr><td>Third</td></tr>")]
        [DataRow("<tr><td colspan='2'>First</td></tr><tr><td>Second</td><td>Third</td></tr>")]
        public void Transform_HeaderlessMultirowTableKeepsNeutralGrid(string rows)
        {
            foreach (var styleCode in new[] { 1, 6, 9 })
            {
                var html = $"<table class='custom ms-rteTable-{styleCode}' style='background-color:white;border:1px solid red'>{rows}</table>";

                using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
                using (var source = new HtmlParser().ParseDocument(html))
                {
                    var table = document.QuerySelector("table");
                    Assert.AreEqual("simpleTableStyleNeutral", table.ClassName);
                    Assert.AreEqual(2, table.QuerySelectorAll("tr").Length);
                    Assert.AreEqual(source.QuerySelectorAll("td, th").Length, table.QuerySelectorAll("td").Length);
                    StringAssert.Contains(table.TextContent, "First");
                    StringAssert.Contains(table.TextContent, "Second");
                    StringAssert.Contains(table.TextContent, "Third");
                }
            }
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(7)]
        public void Transform_NestedHeaderDoesNotStyleOuterContentAsAHeader(int styleCode)
        {
            var html = $@"<table class='ms-rteTable-{styleCode}'><tr><td>Outer content
                <table class='ms-rteTable-9'>
                    <tr><th>Nested heading</th></tr><tr><td>Nested content</td></tr>
                </table>
            </td></tr></table>";

            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var tables = document.QuerySelectorAll("table");
                Assert.AreEqual(2, tables.Length);
                Assert.AreEqual("simpleTableStyleNeutral", tables[0].ClassName);
                Assert.AreEqual("filledHeaderTableStyleTheme", tables[1].ClassName);
                StringAssert.Contains(tables[0].TextContent, "Outer content");
                Assert.AreEqual("Nested heading", tables[1].QuerySelector("strong").TextContent);
            }
        }

        [DataTestMethod]
        [DataRow("", "noBorderTableStyleNeutral", "noBorderTableStyleNeutral")]
        [DataRow("class='ms-rteTable-0'", "noBorderTableStyleNeutral", "noBorderTableStyleNeutral")]
        [DataRow("class='ms-rteTable-3'", "simpleTableStyleNeutral", "simpleTableStyleNeutral")]
        [DataRow("class='ms-rteTable-default'", "simpleTableStyleNeutral", "borderHeaderTableStyleNeutral")]
        [DataRow("class='custom'", "simpleTableStyleNeutral", "borderHeaderTableStyleNeutral")]
        [DataRow("class='ms-rteTable-0' border='1'", "simpleTableStyleNeutral", "borderHeaderTableStyleNeutral")]
        [DataRow("style='border:1px solid red'", "simpleTableStyleNeutral", "borderHeaderTableStyleNeutral")]
        [DataRow("class='custom' border='0'", "noBorderTableStyleNeutral", "noBorderTableStyleNeutral")]
        public void Transform_PreservesBordersWithoutAddingHeaderEmphasis(string attributes, string bodyStyle, string headerStyle)
        {
            foreach (var tag in new[] { "td", "th" })
            {
                var html = $"<table {attributes}><tr><{tag}>Content</{tag}></tr></table>";

                using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
                {
                    Assert.AreEqual(tag == "th" ? headerStyle : bodyStyle, document.QuerySelector("table").ClassName);
                    Assert.AreEqual("Content", document.QuerySelector("td").TextContent);
                    Assert.AreEqual(tag == "th" ? 1 : 0, document.QuerySelectorAll("strong").Length);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_SeverityTableDoesNotApplyHeaderStylingToContent(bool usePlaceHolder)
        {
            const string html = @"<table class='ms-rteTable-6'>
                <tr class='ms-rteTableEvenRow-6'>
                    <td class='ms-rteTableEvenCol-6' style='width:50%'>
                        <h2><strong>Severity 0</strong><br></h2><h2>[Major Disaster]</h2>
                    </td>
                    <td class='ms-rteTableOddCol-6' style='width:50%'>
                        <p><strong>SLA Impact</strong></p><p>Recovery guidance</p>
                        <ul><li>Read the <a href='/details'>instructions</a>.</li></ul>
                    </td>
                </tr>
                <tr class='ms-rteTableOddRow-6'><td><h2>Severity 1</h2></td><td>Details</td></tr>
            </table>";

            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, usePlaceHolder)))
            {
                var table = document.QuerySelector("figure.canvasRteResponsiveTable > table");
                Assert.AreEqual("simpleTableStyleNeutral", table.ClassName);
                Assert.AreEqual(2, table.QuerySelectorAll("tr").Length);
                Assert.AreEqual(4, table.QuerySelectorAll("td").Length);
                CollectionAssert.AreEqual(new[] { "Severity 0", "SLA Impact" },
                    table.QuerySelectorAll("strong").Select(element => element.TextContent).ToArray());
                CollectionAssert.AreEqual(new[] { "Severity 0", "[Major Disaster]", "Severity 1" },
                    table.QuerySelectorAll("h3").Select(element => element.TextContent).ToArray());
                CollectionAssert.AreEqual(new[] { "SLA Impact", "Recovery guidance" },
                    table.QuerySelectorAll("p").Select(element => element.TextContent).ToArray());
                Assert.AreEqual("/details", table.QuerySelector("a").GetAttribute("href"));
                Assert.IsTrue(table.QuerySelectorAll("tr:first-child > td")
                    .All(cell => cell.GetAttribute("style").Contains("width: 50%")));
            }
        }

        [DataTestMethod]
        [DataRow(1, "BackgroundOverlay-Lighter", "#d8d8d8")]
        [DataRow(6, "ContentAccent1-Lightest", "#cbfcfe")]
        public void Transform_HeaderlessBandedTableKeepsAllFiveRowFills(int styleCode, string bandShade, string expectedBand)
        {
            var html = $"<table class='ms-rteTable-{styleCode}'>" +
                string.Concat(Enumerable.Range(0, 5).Select(index =>
                    $"<tr><td><h2>Severity {index}</h2></td><td>Plain text <strong>Important</strong></td></tr>")) + "</table>";
            var transformator = new HtmlTransformator(null, shade =>
            {
                if (shade == "BackgroundOverlay") return "#ffffff";
                Assert.AreEqual(bandShade, shade);
                return styleCode == 6 ? "#b4fbfd" : "#d8d8d8";
            });
            using (var document = new HtmlParser(new HtmlParserOptions(),
                BrowsingContext.New(Configuration.Default.WithCss())).ParseDocument(transformator.Transform(html, false)))
            {
                Assert.IsNotNull(document.QuerySelector("figure.canvasRteResponsiveTable"));
                Assert.AreEqual("simpleTableStyleNeutral", document.QuerySelector("table").ClassName);
                var rows = document.QuerySelectorAll("tr");
                Assert.AreEqual(5, rows.Length);
                for (int index = 0; index < rows.Length; index++)
                {
                    var expected = document.CreateElement("td");
                    expected.GetStyle().SetBackgroundColor(index % 2 == 0 ? "#ffffff" : expectedBand);
                    foreach (var cell in rows[index].Children)
                    {
                        Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cell.GetStyle().GetBackgroundColor());
                        Assert.IsFalse(cell.GetAttribute("style").Contains("font-weight"));
                    }
                }
                Assert.AreEqual(5, document.QuerySelectorAll("strong").Length);
                Assert.AreEqual(5, document.QuerySelectorAll("h3").Length);
            }
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        public void Transform_BandingRespectsExplicitRowClassesAndNestedTables(int styleCode)
        {
            var html = $@"<table class='ms-rteTable-{styleCode}'>
                <tr class='ms-rteTableOddRow-{styleCode}'><td>First</td></tr>
                <tr class='ms-rteTableEvenRow-{styleCode}'><td>Second
                    <table class='ms-rteTable-6'><tr><td>Nested first</td></tr><tr><td>Nested second</td></tr></table>
                </td></tr></table>";
            using (var document = new HtmlParser(new HtmlParserOptions(),
                BrowsingContext.New(Configuration.Default.WithCss()))
                .ParseDocument(new HtmlTransformator(null, shade => shade == "BackgroundOverlay" ? "#ffffff" : "#b4fbfd").Transform(html, false)))
            {
                var tables = document.QuerySelectorAll("table");
                var outerRows = tables[0].Children.Last().Children;
                var nestedRows = tables[1].Children.Last().Children;
                Assert.AreNotEqual(outerRows[1].Children[0].GetStyle().GetBackgroundColor(),
                    outerRows[0].Children[0].GetStyle().GetBackgroundColor());
                Assert.AreEqual(nestedRows[0].Children[0].GetStyle().GetBackgroundColor(),
                    outerRows[1].Children[0].GetStyle().GetBackgroundColor());
                Assert.AreNotEqual(nestedRows[0].Children[0].GetStyle().GetBackgroundColor(),
                    nestedRows[1].Children[0].GetStyle().GetBackgroundColor());
            }
        }

        [TestMethod]
        public void Transform_BandingUsesClassicDefaultsWithWarningWhenThemeIsUnavailable()
        {
            var observer = new Mock<ILogObserver>();
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator(new[] { observer.Object })
                .Transform("<table class='ms-rteTable-6'><tr><td>First</td></tr><tr><td>Second</td></tr></table>", false)))
            {
                var cells = document.QuerySelectorAll("td");
                Assert.AreNotEqual(cells[0].GetAttribute("style"), cells[1].GetAttribute("style"));
                Assert.IsTrue(cells.All(cell => cell.GetAttribute("style").Contains("background-color")));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("classic default palette"))), Times.Once);
            }
        }

        [TestMethod]
        public void Transform_InvalidBandingThemeColorFailsExplicitly()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new HtmlTransformator(null, _ => "invalid")
                .Transform("<table class='ms-rteTable-6'><tr><td>First</td></tr></table>", false));
        }

        [TestMethod]
        public void Transform_ActualBandedHeaderRetainsNativeThemeStyling()
        {
            var html = "<table class='ms-rteTable-6'><tr><th>Heading</th></tr><tr><td>Body</td></tr></table>";
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator(null,
                _ => throw new AssertFailedException("A native header table must not resolve custom banding colors.")).Transform(html, false)))
            {
                Assert.AreEqual("bandedRowTableStyleTheme", document.QuerySelector("table").ClassName);
                Assert.IsTrue(document.QuerySelectorAll("td").All(cell => !cell.GetAttribute("style").Contains("background-color")));
            }
        }

        [DataTestMethod]
        [DataRow("td", "background:#9cc2e5")]
        [DataRow("td", "background-color:#9cc2e5")]
        [DataRow("td", "background-color:rgb(156,194,229)")]
        [DataRow("th", "background:#9cc2e5")]
        [DataRow("th", "background-color:#9cc2e5")]
        public void Transform_PreservesAuthoredCellBackgrounds(string tag, string style)
        {
            var html = $"<table><tr><{tag} style='{style};color:#123456;text-align:center'>" +
                $"<strong>New Method Name</strong></{tag}><{tag} style='{style}'>Description</{tag}></tr>" +
                "<tr><td>GetFileDetails</td><td>Returns file details</td></tr></table>";
            using (var document = new HtmlParser(new HtmlParserOptions(),
                BrowsingContext.New(Configuration.Default.WithCss())).ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                Assert.IsNotNull(document.QuerySelector("figure.canvasRteResponsiveTable"));
                var cells = document.QuerySelectorAll("td");
                var expected = document.CreateElement("td");
                expected.GetStyle().SetBackgroundColor("#9cc2e5");
                expected.GetStyle().SetColor("#123456");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[0].GetStyle().GetBackgroundColor());
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[1].GetStyle().GetBackgroundColor());
                Assert.AreEqual(expected.GetStyle().GetColor(), cells[0].GetStyle().GetColor());
                Assert.AreEqual("center", cells[0].GetStyle().GetTextAlign());
                Assert.IsTrue(string.IsNullOrEmpty(cells[2].GetStyle().GetBackgroundColor()));
                Assert.IsTrue(string.IsNullOrEmpty(cells[3].GetStyle().GetBackgroundColor()));
                Assert.AreEqual("New Method Name", cells[0].QuerySelector("strong").TextContent);
            }
        }

        [TestMethod]
        public void Transform_AuthoredCellFillOverridesBandingButTransparentCellsKeepTheBand()
        {
            const string html = @"<table class='ms-rteTable-6'>
                <tr><td>First</td><td>Second</td></tr>
                <tr><td style='background:#9cc2e5'>Custom</td><td style='background:transparent'>Banded</td></tr>
                <tr><td>Last</td><td>Last</td></tr></table>";
            using (var document = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss()))
                .ParseDocument(new HtmlTransformator(null, shade => shade == "BackgroundOverlay" ? "#ffffff" : "#b4fbfd").Transform(html, false)))
            {
                var cells = document.QuerySelectorAll("td");
                var expected = document.CreateElement("td");
                expected.GetStyle().SetBackgroundColor("#9cc2e5");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[2].GetStyle().GetBackgroundColor());
                expected.GetStyle().SetBackgroundColor("#cbfcfe");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[3].GetStyle().GetBackgroundColor());
                Assert.AreEqual(cells[0].GetStyle().GetBackgroundColor(), cells[4].GetStyle().GetBackgroundColor());
            }
        }

        [TestMethod]
        public void Transform_PreservesAuthoredFillsInNestedAndMergedCells()
        {
            const string html = @"<table><tr><td rowspan='2' style='background:navy;color:white'>Outer
                <table><tr><td style='background:yellow;color:black'>Nested</td></tr></table>
                </td><td>First</td></tr><tr><td>Second</td></tr>
                <tr><td colspan='2' style='background:green'>Merged</td></tr></table>";
            using (var document = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss()))
                .ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var outer = document.QuerySelector("td[rowspan='2']");
                var nested = outer.QuerySelector("td");
                var merged = document.QuerySelector("td[colspan='2']");
                var expected = document.CreateElement("td");
                expected.GetStyle().SetBackgroundColor("navy");
                expected.GetStyle().SetColor("white");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), outer.GetStyle().GetBackgroundColor());
                Assert.AreEqual(expected.GetStyle().GetColor(), outer.GetStyle().GetColor());
                expected.GetStyle().SetBackgroundColor("yellow");
                expected.GetStyle().SetColor("black");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), nested.GetStyle().GetBackgroundColor());
                Assert.AreEqual(expected.GetStyle().GetColor(), nested.GetStyle().GetColor());
                expected.GetStyle().SetBackgroundColor("green");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), merged.GetStyle().GetBackgroundColor());
            }
        }

        [TestMethod]
        public void Transform_PreservesTextMaskInsideAuthoredAndBandedCells()
        {
            const string html = @"<table class='ms-rteTable-6'>
                <tr><td style='background:#9cc2e5'><span id='mask'
                    class='ms-rteThemeForeColor-2-5 ms-rteThemeBackColor-2-5'>Obsolete instructions</span></td></tr>
                <tr><td>Current instructions</td></tr></table>";
            var transformator = new HtmlTransformator(null, shade => shade switch
            {
                "BackgroundOverlay" => "#ffffff",
                "ContentAccent1-Lightest" => "#b4fbfd",
                "BodyText-Darkest" => "#1a1a1a",
                _ => throw new AssertFailedException($"Unexpected theme shade: {shade}")
            });
            using (var document = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss()))
                .ParseDocument(transformator.Transform(html, false)))
            {
                var cells = document.QuerySelectorAll("td");
                var expected = document.CreateElement("span");
                expected.GetStyle().SetBackgroundColor("#9cc2e5");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[0].GetStyle().GetBackgroundColor());
                expected.GetStyle().SetBackgroundColor("#cbfcfe");
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), cells[1].GetStyle().GetBackgroundColor());
                expected.GetStyle().SetBackgroundColor("#1a1a1a");
                expected.GetStyle().SetColor("#1a1a1a");
                var mask = document.QuerySelector("#mask");
                Assert.AreEqual(expected.GetStyle().GetColor(), mask.GetStyle().GetColor());
                Assert.AreEqual(expected.GetStyle().GetBackgroundColor(), mask.GetStyle().GetBackgroundColor());
                Assert.AreEqual("Obsolete instructions", mask.TextContent);
            }
        }
    }
}
