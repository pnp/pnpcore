using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Publishing
{
    internal static class EnterpriseWikiSidebar
    {
        internal const string ComponentId = "2b190945-b52f-4130-9deb-6d949f08660c";
        internal const string ControlKind = "publishing.enterpriseWikiSidebar";

        internal static void Ensure(PnPCore.IPage page)
        {
            var existing = page.Controls.OfType<PnPCore.IPageWebPart>().Where(IsSidebar).ToList();
            if (existing.Count > 1)
            {
                throw new InvalidOperationException("The target page already contains multiple Enterprise Wiki sidebars.");
            }
            var column = EnsureVerticalColumn(page);
            var order = column.Controls.Count == 0 ? 1 : column.Controls.Max(c => c.Order) + 1;
            if (existing.Count == 1)
            {
                if (existing[0].Column != column)
                {
                    existing[0].Move(column, order);
                }
                return;
            }

            // This hidden native host is not advertised by AvailablePageComponents.
            // Its rendering support must already be present in the destination environment.
            var properties = new JObject
            {
                ["controlKind"] = ControlKind,
                ["contractVersion"] = 1,
                ["settings"] = new JObject { ["showRating"] = true, ["showCategories"] = true }
            };
            var component = new PnPCore.PageComponent
            {
                Id = ComponentId,
                Name = "Modernized control host",
                Manifest = new JObject
                {
                    ["preconfiguredEntries"] = new JArray(new JObject
                    {
                        ["title"] = new JObject { ["default"] = "Enterprise Wiki sidebar" },
                        ["properties"] = properties
                    })
                }.ToString(Newtonsoft.Json.Formatting.None)
            };
            var sidebar = page.NewWebPart(component);
            sidebar.InstanceId = Guid.NewGuid();
            sidebar.DataVersion = "1.0";
            page.AddControl(sidebar, column, order);
        }

        internal static bool IsSidebar(PnPCore.IPageWebPart part)
        {
            return string.Equals(part.WebPartId, ComponentId, StringComparison.OrdinalIgnoreCase) &&
                part.Properties.ValueKind == System.Text.Json.JsonValueKind.Object &&
                part.Properties.TryGetProperty("controlKind", out var kind) && kind.GetString() == ControlKind;
        }

        private static PnPCore.ICanvasColumn EnsureVerticalColumn(PnPCore.IPage page)
        {
            var column = page.Sections.SelectMany(s => s.Columns).FirstOrDefault(c => c.IsVerticalSectionColumn);
            if (column != null)
            {
                return column;
            }
            var original = page.Sections.OrderBy(s => s.Order).FirstOrDefault(s => s.Type != PnPCore.CanvasSectionTemplate.OneColumnFullWidth);
            if (original == null)
            {
                var order = page.Sections.Count == 0 ? 1 : page.Sections.Max(s => s.Order) + 1;
                page.AddSection(PnPCore.CanvasSectionTemplate.OneColumnVerticalSection, order);
                return page.Sections.Last().VerticalSectionColumn;
            }

            var template = GetVerticalTemplate(original.Type);
            page.AddSection(template, original.Order);
            var replacement = page.Sections.Last();
            replacement.ZoneEmphasis = original.ZoneEmphasis;
            replacement.Collapsible = original.Collapsible;
            replacement.IsExpanded = original.IsExpanded;
            replacement.DisplayName = original.DisplayName;
            replacement.HeadingLevel = original.HeadingLevel;
            replacement.IconAlignment = original.IconAlignment;
            replacement.ShowDividerLine = original.ShowDividerLine;
            var columns = replacement.Columns.Where(c => !c.IsVerticalSectionColumn).ToList();
            if (original.Columns.Count != columns.Count)
            {
                throw new InvalidOperationException("Cannot add the Enterprise Wiki sidebar without changing the existing column layout.");
            }
            for (var index = 0; index < original.Columns.Count; index++)
            {
                foreach (var control in original.Columns[index].Controls.ToList())
                {
                    control.Move(columns[index], control.Order);
                }
            }
            page.Sections.Remove(original);
            return replacement.VerticalSectionColumn;
        }

        internal static PnPCore.CanvasSectionTemplate GetVerticalTemplate(PnPCore.CanvasSectionTemplate template)
        {
            switch (template)
            {
                case PnPCore.CanvasSectionTemplate.OneColumn: return PnPCore.CanvasSectionTemplate.OneColumnVerticalSection;
                case PnPCore.CanvasSectionTemplate.TwoColumn: return PnPCore.CanvasSectionTemplate.TwoColumnVerticalSection;
                case PnPCore.CanvasSectionTemplate.TwoColumnLeft: return PnPCore.CanvasSectionTemplate.TwoColumnLeftVerticalSection;
                case PnPCore.CanvasSectionTemplate.TwoColumnRight: return PnPCore.CanvasSectionTemplate.TwoColumnRightVerticalSection;
                case PnPCore.CanvasSectionTemplate.ThreeColumn: return PnPCore.CanvasSectionTemplate.ThreeColumnVerticalSection;
                default: throw new InvalidOperationException($"The layout '{template}' cannot be converted to an Enterprise Wiki sidebar layout.");
            }
        }
    }
}
