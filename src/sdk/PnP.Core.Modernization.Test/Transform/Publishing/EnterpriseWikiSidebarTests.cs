using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using PnP.Core.Modernization.Publishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class EnterpriseWikiSidebarTests
    {
        [DataTestMethod]
        [DataRow(CanvasSectionTemplate.OneColumn, CanvasSectionTemplate.OneColumnVerticalSection)]
        [DataRow(CanvasSectionTemplate.TwoColumnRight, CanvasSectionTemplate.TwoColumnRightVerticalSection)]
        [DataRow(CanvasSectionTemplate.ThreeColumn, CanvasSectionTemplate.ThreeColumnVerticalSection)]
        public void AddingSidebarPreservesContentColumnOrderAndSectionSettings(CanvasSectionTemplate originalType, CanvasSectionTemplate expected)
        {
            var fixture = new PageFixture();
            var original = fixture.AddSection(originalType, 2);
            original.ZoneEmphasis = 2;
            original.Collapsible = true;
            original.DisplayName = "Existing section";
            var controls = original.Columns.Select((column, index) => fixture.AddText(column, index + 2)).ToArray();

            EnterpriseWikiSidebar.Ensure(fixture.Page.Object);

            var section = fixture.Sections.Single();
            Assert.AreEqual(expected, section.Type);
            Assert.AreEqual(2, section.Order);
            Assert.AreEqual(2, section.ZoneEmphasis);
            Assert.IsTrue(section.Collapsible);
            Assert.AreEqual("Existing section", section.DisplayName);
            for (var index = 0; index < controls.Length; index++)
            {
                Assert.AreSame(section.Columns[index], controls[index].Column);
                Assert.AreEqual(index + 2, controls[index].Order);
                Assert.IsTrue(fixture.Controls.Contains(controls[index]));
            }
            var sidebar = fixture.Controls.OfType<IPageWebPart>().Single();
            Assert.AreSame(section.VerticalSectionColumn, sidebar.Column);
            Assert.AreNotEqual(Guid.Empty, sidebar.InstanceId);
            Assert.AreEqual(EnterpriseWikiSidebar.ControlKind, sidebar.Properties.GetProperty("controlKind").GetString());
            Assert.AreEqual(1, sidebar.Properties.GetProperty("contractVersion").GetInt32());
            Assert.IsTrue(sidebar.Properties.GetProperty("settings").GetProperty("showRating").GetBoolean());
            Assert.IsTrue(sidebar.Properties.GetProperty("settings").GetProperty("showCategories").GetBoolean());
        }

        [TestMethod]
        public void ReusesExistingVerticalColumnAndDoesNotDuplicateSidebar()
        {
            var fixture = new PageFixture();
            var section = fixture.AddSection(CanvasSectionTemplate.OneColumnVerticalSection, 1);
            fixture.AddText(section.VerticalSectionColumn, 4);
            EnterpriseWikiSidebar.Ensure(fixture.Page.Object);
            var sidebar = fixture.Controls.OfType<IPageWebPart>().Single();
            Assert.AreEqual(5, sidebar.Order);
            EnterpriseWikiSidebar.Ensure(fixture.Page.Object);
            Assert.AreEqual(1, fixture.Sections.Count);
            Assert.AreEqual(1, fixture.Controls.OfType<IPageWebPart>().Count());
        }

        [TestMethod]
        public void SharedHostWithAnotherControlKindIsNotTreatedAsASidebar()
        {
            var fixture = new PageFixture();
            var section = fixture.AddSection(CanvasSectionTemplate.OneColumn, 1);
            var component = new Mock<IPageComponent>();
            component.SetupGet(p => p.Id).Returns(EnterpriseWikiSidebar.ComponentId);
            component.SetupGet(p => p.Manifest).Returns("{\"preconfiguredEntries\":[{\"properties\":{\"controlKind\":\"publishing.other\"}}]}");
            var other = fixture.NewWebPart(component.Object);
            fixture.AddControl(other, section.DefaultColumn, 1);
            EnterpriseWikiSidebar.Ensure(fixture.Page.Object);
            Assert.AreEqual(2, fixture.Controls.OfType<IPageWebPart>().Count());
            Assert.IsTrue(fixture.Controls.Contains(other));
            Assert.AreEqual(1, fixture.Controls.OfType<IPageWebPart>().Count(EnterpriseWikiSidebar.IsSidebar));
        }

        private sealed class PageFixture
        {
            internal readonly Mock<IPage> Page = new Mock<IPage>();
            internal readonly List<ICanvasSection> Sections = new List<ICanvasSection>();
            internal readonly List<ICanvasControl> Controls = new List<ICanvasControl>();
            private readonly Dictionary<ICanvasControl, ICanvasColumn> locations = new Dictionary<ICanvasControl, ICanvasColumn>();

            internal PageFixture()
            {
                Page.SetupGet(p => p.Sections).Returns(Sections);
                Page.SetupGet(p => p.Controls).Returns(Controls);
                Page.Setup(p => p.AddSection(It.IsAny<CanvasSectionTemplate>(), It.IsAny<float>(), It.IsAny<ZoneReflowStrategy?>()))
                    .Callback<CanvasSectionTemplate, float, ZoneReflowStrategy?>((template, order, reflow) => AddSection(template, order));
                Page.Setup(p => p.NewWebPart(It.IsAny<IPageComponent>())).Returns<IPageComponent>(NewWebPart);
                Page.Setup(p => p.AddControl(It.IsAny<ICanvasControl>(), It.IsAny<ICanvasColumn>(), It.IsAny<int>(), It.IsAny<ControlFlexLayoutPosition>()))
                    .Callback<ICanvasControl, ICanvasColumn, int, ControlFlexLayoutPosition>((control, column, order, position) => AddControl(control, column, order));
            }

            internal ICanvasSection AddSection(CanvasSectionTemplate template, float order)
            {
                var section = new Mock<ICanvasSection>();
                section.SetupAllProperties();
                section.Object.Type = template;
                section.Object.Order = order;
                var count = template.ToString().StartsWith("Three") ? 3 : template.ToString().StartsWith("Two") ? 2 : 1;
                var columns = new List<ICanvasColumn>();
                var vertical = template.ToString().EndsWith("VerticalSection");
                for (var index = 0; index < count + (vertical ? 1 : 0); index++)
                {
                    var column = new Mock<ICanvasColumn>();
                    var isVertical = vertical && index == count;
                    column.SetupGet(c => c.IsVerticalSectionColumn).Returns(isVertical);
                    column.SetupGet(c => c.Section).Returns(section.Object);
                    column.SetupGet(c => c.Controls).Returns(() => Controls.Where(c => locations[c] == column.Object).ToList());
                    columns.Add(column.Object);
                }
                section.SetupGet(s => s.Columns).Returns(columns);
                section.SetupGet(s => s.DefaultColumn).Returns(columns[0]);
                section.SetupGet(s => s.VerticalSectionColumn).Returns(() => columns.FirstOrDefault(c => c.IsVerticalSectionColumn));
                section.SetupGet(s => s.Controls).Returns(() => Controls.Where(c => locations[c].Section == section.Object).ToList());
                Sections.Add(section.Object);
                return section.Object;
            }

            private void ConfigureControl<T>(Mock<T> control) where T : class, ICanvasControl
            {
                control.SetupAllProperties();
                control.SetupGet(c => c.Column).Returns(() => locations.TryGetValue(control.Object, out var column) ? column : null);
                control.SetupGet(c => c.Section).Returns(() => locations.TryGetValue(control.Object, out var column) ? column.Section : null);
                control.Setup(c => c.Move(It.IsAny<ICanvasColumn>(), It.IsAny<int>()))
                    .Callback<ICanvasColumn, int>((column, order) => { locations[control.Object] = column; control.Object.Order = order; });
            }

            internal IPageWebPart NewWebPart(IPageComponent component)
            {
                var part = new Mock<IPageWebPart>();
                ConfigureControl(part);
                part.SetupGet(p => p.WebPartId).Returns(component.Id);
                using (var manifest = JsonDocument.Parse(component.Manifest))
                {
                    part.Object.PropertiesJson = manifest.RootElement.GetProperty("preconfiguredEntries")[0].GetProperty("properties").GetRawText();
                }
                part.SetupGet(p => p.Properties).Returns(() =>
                {
                    using (var json = JsonDocument.Parse(part.Object.PropertiesJson))
                    {
                        return json.RootElement.Clone();
                    }
                });
                return part.Object;
            }

            internal IPageText AddText(ICanvasColumn column, int order)
            {
                var text = new Mock<IPageText>();
                ConfigureControl(text);
                AddControl(text.Object, column, order);
                return text.Object;
            }

            internal void AddControl(ICanvasControl control, ICanvasColumn column, int order)
            {
                locations[control] = column;
                control.Order = order;
                Controls.Add(control);
            }
        }
    }
}
