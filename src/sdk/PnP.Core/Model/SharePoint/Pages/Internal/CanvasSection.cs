using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a section on the canvas
    /// </summary>
    internal sealed class CanvasSection : ICanvasSection
    {

        #region variables
        private int zoneEmphasis;
        #endregion

        #region construction
        internal CanvasSection(IPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            Page = page;
            zoneEmphasis = 0;
            Order = 0;
        }

        /// <summary>
        /// Creates a new canvas section
        /// </summary>
        /// <param name="page"><see cref="IPage"/> instance that holds this section</param>
        /// <param name="canvasSectionTemplate">Type of section to create</param>
        /// <param name="order">Order of this section in the collection of sections on the page</param>
        internal CanvasSection(IPage page, CanvasSectionTemplate canvasSectionTemplate, float order)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            Page = page;
            zoneEmphasis = 0;
            Type = canvasSectionTemplate;
            Order = order;

            switch (canvasSectionTemplate)
            {
                case CanvasSectionTemplate.OneColumn:
                    goto default;
                case CanvasSectionTemplate.OneColumnFullWidth:
                    Columns.Add(new CanvasColumn(this, 1, 0));
                    break;
                case CanvasSectionTemplate.OneColumnVerticalSection:
                    Columns.Add(new CanvasColumn(this, 1, 12, 1));
                    Columns.Add(new CanvasColumn(this, 1, 12, 2));
                    break;
                case CanvasSectionTemplate.TwoColumn:
                    Columns.Add(new CanvasColumn(this, 1, 6));
                    Columns.Add(new CanvasColumn(this, 2, 6));
                    break;
                case CanvasSectionTemplate.TwoColumnVerticalSection:
                    Columns.Add(new CanvasColumn(this, 1, 6, 1));
                    Columns.Add(new CanvasColumn(this, 2, 6, 1));
                    Columns.Add(new CanvasColumn(this, 1, 12, 2));
                    break;
                case CanvasSectionTemplate.ThreeColumn:
                    Columns.Add(new CanvasColumn(this, 1, 4));
                    Columns.Add(new CanvasColumn(this, 2, 4));
                    Columns.Add(new CanvasColumn(this, 3, 4));
                    break;
                case CanvasSectionTemplate.ThreeColumnVerticalSection:
                    Columns.Add(new CanvasColumn(this, 1, 4, 1));
                    Columns.Add(new CanvasColumn(this, 2, 4, 1));
                    Columns.Add(new CanvasColumn(this, 3, 4, 1));
                    Columns.Add(new CanvasColumn(this, 1, 12, 2));
                    break;
                case CanvasSectionTemplate.TwoColumnLeft:
                    Columns.Add(new CanvasColumn(this, 1, 8));
                    Columns.Add(new CanvasColumn(this, 2, 4));
                    break;
                case CanvasSectionTemplate.TwoColumnLeftVerticalSection:
                    Columns.Add(new CanvasColumn(this, 1, 8, 1));
                    Columns.Add(new CanvasColumn(this, 2, 4, 1));
                    Columns.Add(new CanvasColumn(this, 1, 12, 2));
                    break;
                case CanvasSectionTemplate.TwoColumnRight:
                    Columns.Add(new CanvasColumn(this, 1, 4));
                    Columns.Add(new CanvasColumn(this, 2, 8));
                    break;
                case CanvasSectionTemplate.TwoColumnRightVerticalSection:
                    Columns.Add(new CanvasColumn(this, 1, 4, 1));
                    Columns.Add(new CanvasColumn(this, 2, 8, 1));
                    Columns.Add(new CanvasColumn(this, 1, 12, 2));
                    break;
                default:
                    Columns.Add(new CanvasColumn(this, 1, 12));
                    break;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Type of the section
        /// </summary>
        public CanvasSectionTemplate Type { get; set; }

        /// <summary>
        /// Order in which this section is presented on the page
        /// </summary>
        public float Order { get; set; }

        /// <summary>
        /// <see cref="CanvasColumn"/> instances that are part of this section
        /// </summary>
        public List<ICanvasColumn> Columns { get; } = new List<ICanvasColumn>(3);

        /// <summary>
        /// The <see cref="IPage"/> instance holding this section
        /// </summary>
        public IPage Page { get; }

        /// <summary>
        /// Controls hosted in this section
        /// </summary>
        public List<ICanvasControl> Controls
        {
            get
            {
                return Page.Controls.Where(p => p.Section == this).ToList();
            }
        }

        /// <summary>
        /// The default <see cref="CanvasColumn"/> of this section
        /// </summary>
        public ICanvasColumn DefaultColumn
        {
            get
            {
                if (Columns.Count == 0)
                {
                    Columns.Add(new CanvasColumn(this));
                }

                return Columns.First();
            }
        }

        /// <summary>
        /// A page can contain one section that has a vertical section column...use this attribute to get that column
        /// </summary>
        public ICanvasColumn VerticalSectionColumn
        {
            get
            {
                return Columns.FirstOrDefault(p => p.LayoutIndex == 2);
            }
        }

        /// <summary>
        /// Color emphasis of the section 
        /// </summary>
        public int ZoneEmphasis
        {
            get
            {
                return zoneEmphasis;
            }
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new ArgumentException(PnPCoreResources.Exception_Page_InvalidZoneEmphasis);
                }

                zoneEmphasis = value;
            }
        }

        /// <summary>
        /// Is this section collapsible
        /// </summary>
        public bool Collapsible { get; set; }

        /// <summary>
        /// Is the collapsible section expanded?
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// DisplayName of the collapsible section
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Sets the alignment of the collaps icon, left/right or not configured
        /// </summary>
        public IconAlignment? IconAlignment { get; set; }

        /// <summary>
        /// Show a devided line for this collapsible section?
        /// </summary>
        public bool ShowDividerLine { get; set; }

        /// <summary>
        /// Type of this section, not configurable by SDK users
        /// </summary>
        public int SectionType { get; set; }

        #endregion

        #region internal and private methods
        /// <summary>
        /// Renders this section as a HTML fragment
        /// </summary>
        /// <returns>HTML string representing this section</returns>
        internal string ToHtml()
        {
            StringBuilder html = new StringBuilder();
            foreach (var column in Columns.OrderBy(z => z.LayoutIndex).ThenBy(z => z.Order))
            {
                html.Append((column as CanvasColumn).ToHtml());
            }
            return html.ToString();
        }

        internal void AddColumn(CanvasColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            Columns.Add(column);
        }

        internal void MergeVerticalSectionColumn(CanvasColumn column)
        {
            // What was the highest order
            int order = 1;
            var lastColumn = Columns.OrderBy(p => p.Order).FirstOrDefault();
            if (lastColumn != null)
            {
                order = lastColumn.Order + 1;
            }

            // Add the column to this section, first ensure it's connected to the new section and it's order has been updated for insertion in the new section
            column.MoveTo(this);
            column.Order = order;

            AddColumn(column);
        }

        #endregion

    }
}
