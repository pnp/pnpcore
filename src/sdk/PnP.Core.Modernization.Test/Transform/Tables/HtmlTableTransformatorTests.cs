using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System.Globalization;
using System.Linq;

namespace PnP.Core.Modernization.Tests.Transform.Tables
{
    [TestClass]
    [TestCategory("Offline")]
    public class HtmlTableTransformatorTests
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void Transform_UnsizedSingleRowBorderlessTableUsesAutomaticWidths(int contentColumn)
        {
            var cells = Enumerable.Range(0, 3).Select(column => column == contentColumn
                ? "<td><strong>Troubleshooting instructions:</strong><p>Read the guidance.</p></td>"
                : "<td>\u200b<br></td>");
            var html = $"<table><tr>{string.Concat(cells)}</tr></table>";

            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var result = document.QuerySelectorAll("table tr td");
                Assert.AreEqual(3, result.Length);
                for (int i = 0; i < result.Length; i++)
                {
                    Assert.AreEqual("width: auto", result[i].GetAttribute("style").TrimEnd(';'));
                }
                Assert.AreEqual("Troubleshooting instructions:", result[contentColumn].QuerySelector("strong").TextContent);
                Assert.AreEqual("Read the guidance.", result[contentColumn].QuerySelector("p").TextContent);
            }
        }

        [DataTestMethod]
        [DataRow("<table><tr><td>First</td><td>Second</td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td><img src='/image.png'></td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td><hr></td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td><svg></svg></td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td style='background-color:red'></td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td class='custom-cell'></td><td></td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td><span style='width:25px'></span></td><td></td></tr></table>")]
        [DataRow("<table border='1'><tr><td>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table border='0' class='custom'><tr><td>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table><tr><td class='custom'>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table width='auto'><tr><td style='width:auto'>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table><colgroup><col><col><col></colgroup><tr><td>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table><tr><td colspan='2'>First</td><td></td></tr></table>")]
        [DataRow("<table><tr><td rowspan='2'>First</td><td></td><td></td></tr><tr><td>Second</td><td>Third</td></tr></table>")]
        [DataRow("<table><tr><td>First</td><td></td><td></td></tr><tr><td>Second</td><td></td><td></td></tr></table>")]
        [DataRow("<table><tr><th>Heading</th><th></th><th></th></tr><tr><td>First</td><td></td><td></td></tr></table>")]
        [DataRow("<table class='ms-rteTable-default'><tr><td class='ms-rteTableEvenCol-default'>First</td><td class='ms-rteTableOddCol-default'>Second</td><td>Third</td></tr></table>")]
        public void Transform_UnsizedTablesUseAutomaticColumnWidths(string html)
        {
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            using (var source = new HtmlParser().ParseDocument(html))
            {
                var cells = document.QuerySelectorAll("table tr > td");
                Assert.AreEqual(source.QuerySelectorAll("td, th").Length, cells.Length);
                Assert.IsTrue(cells.All(cell => cell.GetAttribute("style").Split(';').Any(style => style.Trim() == "width: auto")));
            }
        }

        [DataTestMethod]
        [DataRow("<table width='600'><tr><td>First</td><td></td><td></td></tr></table>", "width: 100%", "width: auto", "width: auto", "width: auto")]
        [DataRow("<table style='width:90%'><tr><td>First</td><td></td><td></td></tr></table>", "width: 90%", "width: auto", "width: auto", "width: auto")]
        [DataRow("<table><colgroup><col style='width:50%'></colgroup><tr><td>First</td><td></td><td></td></tr></table>", "width: 100%", "width: 50%", "width: 25%", "width: 25%")]
        [DataRow("<table><tr><td style='width:50%'>First</td><td></td><td></td></tr></table>", "width: 100%", "width: 50%", "width: 25%", "width: 25%")]
        [DataRow("<table><tr><td style='min-width:50px'>First</td><td></td><td></td></tr></table>", "width: 100%", "width: auto", "width: auto", "width: auto")]
        [DataRow("<table><tr><td>First</td><td width='30'></td><td></td></tr></table>", "width: 100%", "width: 33.333333%", "width: 33.333333%", "width: 33.333333%")]
        public void Transform_PreservesRelativeSizingWithoutFixedDefaultWidths(string html, string tableStyle, string firstStyle, string secondStyle, string thirdStyle)
        {
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var result = document.QuerySelectorAll("table tr:first-child > td");
                Assert.AreEqual(tableStyle, document.QuerySelector("figure").GetAttribute("style")?.TrimEnd(';'));
                CollectionAssert.AreEqual(new[] { firstStyle, secondStyle, thirdStyle },
                    result.Select(cell => cell.GetAttribute("style").TrimEnd(';')).ToArray());
            }
        }

        [DataTestMethod]
        [DataRow(1, 2)]
        [DataRow(2, 2)]
        [DataRow(1, int.MaxValue)]
        [DataRow(2, int.MaxValue)]
        public void Transform_PreservesNestedTableContentWhenRowSpanExceedsRows(int colSpan, int rowSpan)
        {
            var html = "<div><table><tr><td colspan='3'><div>" +
                "<table class='ms-rteTable-7'><tr><td>" +
                "<h1>Welcome to the onboarding page</h1>" +
                "<h2>Important Security Updates</h2>" +
                "<p>CORP tenant registrations are unsupported.</p>" +
                "<p>Use a supported tenant.</p>" +
                "<ul><li><a href='/access-review'>Keycode access review</a></li></ul>" +
                $"</td><td colspan='{colSpan}' rowspan='{rowSpan}'>" +
                "<table><tr><td><p>Later security guidance</p></td></tr></table>" +
                "</td></tr></table></div></td></tr></table></div>";
            var observer = new Mock<ILogObserver>();
            var result = new HtmlTransformator(new[] { observer.Object }).Transform(html, false);

            using (var document = new HtmlParser().ParseDocument(result))
            {
                var expected = new[]
                {
                    "Welcome to the onboarding page",
                    "Important Security Updates",
                    "CORP tenant registrations are unsupported.",
                    "Use a supported tenant.",
                    "Keycode access review",
                    "Later security guidance"
                };
                CollectionAssert.AreEqual(expected,
                    document.QuerySelectorAll("h2, h3, p, li").Select(element => element.TextContent).ToArray());
                Assert.AreEqual("/access-review", document.QuerySelector("li > a").GetAttribute("href"));
                var introTable = document.QuerySelector("h2").Closest("table");
                Assert.AreEqual(colSpan, ((IHtmlTableCellElement)introTable.QuerySelector("tr > td:last-child")).ColumnSpan);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void Transform_ClipsRowSpanToRemainingRows(int colSpan)
        {
            var html = "<table><tr><td>First row</td></tr>" +
                $"<tr><td colspan='{colSpan}' rowspan='3'>Last row</td></tr></table>";
            var observer = new Mock<ILogObserver>();
            var result = new HtmlTransformator(new[] { observer.Object }).Transform(html, false);

            using (var document = new HtmlParser().ParseDocument(result))
            {
                Assert.AreEqual(1, document.QuerySelectorAll(".canvasRteResponsiveTable").Length);
                Assert.AreEqual(2, document.QuerySelectorAll("tr").Length);
                Assert.AreEqual(2, document.QuerySelectorAll("td").Length);
                Assert.AreEqual("First rowLast row", document.Body.TextContent);
                Assert.AreEqual(0, document.QuerySelectorAll("[rowspan]").Length);
                Assert.AreEqual(colSpan, ((IHtmlTableCellElement)document.QuerySelector("tr:last-child > td")).ColumnSpan);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void Transform_PreservesValidRowAndColumnSpans(int colSpan)
        {
            var html = $"<table><tr><th colspan='{colSpan + 1}'>Header</th></tr>" +
                $"<tr><td colspan='{colSpan}' rowspan='2'>Spanning cell</td><td>First</td></tr>" +
                "<tr><td>Second</td></tr></table>";
            var observer = new Mock<ILogObserver>();
            var result = new HtmlTransformator(new[] { observer.Object }).Transform(html, false);

            using (var document = new HtmlParser().ParseDocument(result))
            {
                var rows = document.QuerySelectorAll("tr");
                Assert.AreEqual(3, rows.Length);
                Assert.AreEqual("Header", rows[0].QuerySelector("td > strong").TextContent);
                Assert.AreEqual("Spanning cell", rows[1].Children[0].TextContent);
                CollectionAssert.AreEqual(new[] { 1, 2, 1 }, rows.Select(row => row.Children.Length).ToArray());
                Assert.AreEqual("First", rows[1].Children[1].TextContent);
                Assert.AreEqual("Second", rows[2].Children[0].TextContent);
                Assert.AreEqual(colSpan + 1, ((IHtmlTableCellElement)rows[0].Children[0]).ColumnSpan);
                Assert.AreEqual(colSpan, ((IHtmlTableCellElement)rows[1].Children[0]).ColumnSpan);
                Assert.AreEqual(2, ((IHtmlTableCellElement)rows[1].Children[0]).RowSpan);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [TestMethod]
        public void Transform_SignableFilesKeepsFiveLogicalColumnsAndRelativeSizing()
        {
            const string html = @"<table class='ms-rteTable-7' style='width:1000px;height:2000px'>
                <tr><th style='width:504px'>File Type/Extension</th><th style='width:65pt'>SHA-1 Support</th>
                <th style='width:65pt'>SHA-2 Support</th><th colspan='2' style='width:118px'>Multiple Signature Support</th></tr>
                <tr><td style='width:504px'>Portable Executable Files</td><td style='width:65pt'></td>
                <td style='width:65pt'></td><td colspan='2' style='width:118px'></td></tr>
                <tr><td style='width:504px'><a href='/exe'>.EXE</a></td><td style='width:65pt'>Yes</td>
                <td style='width:65pt'>Yes</td><td colspan='2' style='width:118px'>Yes</td></tr></table>";
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var figure = document.QuerySelector("figure.table.canvasRteResponsiveTable.tableLeftAlign");
                Assert.AreEqual("width: 100%", figure.GetAttribute("style").TrimEnd(';'));
                Assert.AreEqual("filledHeaderTableStyleTheme", figure.QuerySelector("table").ClassName);
                var widths = figure.QuerySelectorAll("col").Select(col =>
                    double.Parse(col.GetAttribute("style").Replace("width:", "").Trim().TrimEnd(';', '%'), CultureInfo.InvariantCulture)).ToArray();
                Assert.AreEqual(5, widths.Length);
                Assert.AreEqual(100, widths.Sum(), 0.00001);
                Assert.AreEqual(504 / (65 * 96.0 / 72), widths[0] / widths[1], 0.00001);
                Assert.AreEqual(widths[1], widths[2], 0.00001);
                Assert.AreEqual(widths[3], widths[4], 0.00001);
                Assert.AreEqual(504.0 / 118, widths[0] / (widths[3] + widths[4]), 0.00001);
                foreach (var row in figure.QuerySelectorAll("tr").Cast<IHtmlTableRowElement>())
                {
                    Assert.AreEqual(4, row.Cells.Length);
                    Assert.AreEqual(5, row.Cells.Sum(cell => cell.ColumnSpan));
                    Assert.AreEqual(2, row.Cells[3].ColumnSpan);
                    StringAssert.Contains(row.Cells[0].GetAttribute("style"), "%");
                    StringAssert.Contains(row.Cells[3].GetAttribute("style"), "%");
                }
                Assert.AreEqual("/exe", figure.QuerySelector("a").GetAttribute("href"));
            }
        }

        [TestMethod]
        public void Transform_MultirowHeaderUsesTheSameSpanGridAsData()
        {
            const string html = "<table><thead><tr><th rowspan='2'>A</th><th colspan='2'>B</th></tr>" +
                "<tr><th>C</th><th>D</th></tr></thead><tbody><tr><td>E</td><td>F</td><td>G</td></tr></tbody></table>";
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var rows = document.QuerySelectorAll("tr").Cast<IHtmlTableRowElement>().ToArray();
                CollectionAssert.AreEqual(new[] { 2, 2, 3 }, rows.Select(row => row.Cells.Length).ToArray());
                Assert.AreEqual(2, rows[0].Cells[0].RowSpan);
                Assert.AreEqual(2, rows[0].Cells[1].ColumnSpan);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, document.QuerySelectorAll("strong").Select(e => e.TextContent).ToArray());
                Assert.AreEqual("ABCDEFG", document.Body.TextContent);
            }
        }

        [DataTestMethod]
        [DataRow("0")]
        [DataRow("3")]
        [DataRow("2147483647")]
        public void Transform_RowSpanStopsAtItsSourceRowGroup(string span)
        {
            var html = $"<table><tbody><tr><td rowspan='{span}'>A</td><td>B</td></tr><tr><td>C</td></tr></tbody>" +
                "<tbody><tr><td>D</td><td>E</td></tr></tbody></table>";
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                var rows = document.QuerySelectorAll("tr").Cast<IHtmlTableRowElement>().ToArray();
                Assert.AreEqual(2, rows[0].Cells[0].RowSpan);
                CollectionAssert.AreEqual(new[] { 2, 1, 2 }, rows.Select(row => row.Cells.Length).ToArray());
                Assert.AreEqual("ABCDE", document.Body.TextContent);
            }
        }

        [TestMethod]
        public void Transform_RowSpanAddsLogicalColumnsWithoutRectangularPadding()
        {
            const string html = "<table><tr><td rowspan='2'>A</td></tr><tr><td>B</td><td>C</td></tr></table>";
            var observer = new Mock<ILogObserver>();
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator(new[] { observer.Object }).Transform(html, false)))
            {
                Assert.AreEqual(3, document.QuerySelectorAll("td").Length);
                Assert.AreEqual("2", document.QuerySelector("td").GetAttribute("rowspan"));
                Assert.AreEqual("ABC", document.Body.TextContent);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow("-2", 1)]
        [DataRow("0", 1)]
        [DataRow("invalid", 1)]
        [DataRow("\u0662", 1)]
        [DataRow("2junk", 2)]
        [DataRow("2147483647", 1000)]
        [DataRow("9999999999999999999999999999", 1000)]
        public void Transform_BoundsColumnSpansBeforeBuildingTheGrid(string span, int expected)
        {
            var observer = new Mock<ILogObserver>();
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator(new[] { observer.Object })
                .Transform($"<table><tr><td colspan='{span}'>A</td><td>B</td></tr></table>", false)))
            {
                Assert.AreEqual(2, document.QuerySelectorAll("td").Length);
                Assert.AreEqual(expected, ((IHtmlTableCellElement)document.QuerySelector("td")).ColumnSpan);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow("<colgroup><col style='width:75%'><col width='25%'></colgroup>", "<td>A</td><td>B</td>", "width: 75%", "width: 25%")]
        [DataRow("<colgroup span='2' width='50%'></colgroup>", "<td>A</td><td>B</td>", "width: 50%", "width: 50%")]
        [DataRow("<colgroup><col span='2' width='50%'></colgroup>", "<td>A</td><td>B</td>", "width: 50%", "width: 50%")]
        [DataRow("", "<td width='400' colspan='2'>A</td>", "width: 50%", "width: 50%")]
        [DataRow("<colgroup><col width='100'></colgroup>", "<td width='400' colspan='2'>A</td>", "width: 25%", "width: 75%")]
        [DataRow("<colgroup><col width='75%'><col width='100'></colgroup>", "<td>A</td><td>B</td>", "width: 75%", "width: 25%")]
        [DataRow("<colgroup><col style='width:72pt'><col width='96'></colgroup>", "<td>A</td><td>B</td>", "width: 50%", "width: 50%")]
        public void Transform_PreservesLogicalColumnWidths(string columns, string cells, string first, string second)
        {
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator()
                .Transform($"<table>{columns}<tr>{cells}</tr></table>", false)))
            {
                CollectionAssert.AreEqual(new[] { first, second },
                    document.QuerySelectorAll("col").Select(col => col.GetAttribute("style").TrimEnd(';')).ToArray());
            }
        }

        [TestMethod]
        public void Transform_DoesNotMoveLaterHeadersToTheTop()
        {
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator()
                .Transform("<table class='ms-rteTable-7'><tr><td>Intro</td></tr><tr><th>Header</th></tr><tr><td>Body</td></tr></table>", false)))
            {
                Assert.AreEqual("IntroHeaderBody", document.Body.TextContent);
                Assert.AreEqual("simpleTableStyleNeutral", document.QuerySelector("table").ClassName);
            }
        }

        [DataTestMethod]
        [DataRow("1000px", "100%")]
        [DataRow("150%", "100%")]
        [DataRow("80%", "80%")]
        public void Transform_TableWidthFitsCanvas(string width, string expected)
        {
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator()
                .Transform($"<table style='width:{width};min-width:1000px'><tr><td>Content</td></tr></table>", false)))
            {
                Assert.AreEqual("width: " + expected, document.QuerySelector("figure").GetAttribute("style").TrimEnd(';'));
                Assert.IsFalse(document.Body.InnerHtml.Contains("1000px"));
            }
        }

        [TestMethod]
        public void Transform_OverlappingSpansKeepAllContentAndWarn()
        {
            var observer = new Mock<ILogObserver>();
            const string html = "<table><tr><td>A</td><td rowspan='2'>B</td></tr><tr><td colspan='2'>C</td><td>D</td></tr></table>";
            using (var document = new HtmlParser().ParseDocument(new HtmlTransformator(new[] { observer.Object }).Transform(html, false)))
            {
                Assert.AreEqual("ABCD", document.Body.TextContent);
                Assert.AreEqual(4, document.QuerySelectorAll("td").Length);
                Assert.AreEqual(0, document.QuerySelectorAll("[colspan]").Length);
            }
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Once);
        }

        [TestMethod]
        public void Transform_ExplicitColumnsWinOverConflictingCellsAndUseInvariantNumbers()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                const string html = "<table><colgroup><col width='75%'><col width='25%'></colgroup><tr><td width='100'>A</td><td width='900'>B</td></tr></table>";
                using (var document = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
                {
                    CollectionAssert.AreEqual(new[] { "width: 75%", "width: 25%" },
                        document.QuerySelectorAll("col").Select(col => col.GetAttribute("style").TrimEnd(';')).ToArray());
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }
}
