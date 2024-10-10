using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CanvasColumn : ICanvasColumn
    {
        #region variables
        internal const string CanvasControlAttribute = "data-sp-canvascontrol";
        internal const string CanvasDataVersionAttribute = "data-sp-canvasdataversion";
        internal const string ControlDataAttribute = "data-sp-controldata";
        private int? zoneEmphasis;
        private readonly string DataVersion = "1.0";
        #endregion

        // internal constructors as we don't want users to manually create sections
        #region construction
        internal CanvasColumn(CanvasSection section)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            ColumnFactor = 12;
            Order = 0;
            LayoutIndex = 1;
        }

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Order = order;
            ColumnFactor = sectionFactor ?? 12;
            LayoutIndex = 1;
        }

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor, int? layoutIndex)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Order = order;
            ColumnFactor = sectionFactor ?? 12;
            LayoutIndex = layoutIndex ?? 1;
        }
        #endregion

        #region Properties
        public int Order { get; set; }

        /// <summary>
        /// <see cref="ICanvasSection"/> this section belongs to
        /// </summary>
        public ICanvasSection Section { get; private set; }

        /// <summary>
        /// Column size factor. Max value is 12 (= one column), other options are 8,6,4 or 0
        /// </summary>
        public int ColumnFactor { get; private set; }

        /// <summary>
        /// Returns the layout index. Defaults to 1, except for the vertical section column this is 2
        /// </summary>
        public int LayoutIndex { get; }

        /// <summary>
        /// List of <see cref="ICanvasControl"/> instances that are hosted in this section
        /// </summary>
        public List<ICanvasControl> Controls
        {
            get
            {
                return Section.Page.Controls.Where(p => p.Section == Section && p.Column == this).ToList();
            }
        }

        /// <summary>
        /// Is this a vertical section column?
        /// </summary>
        public bool IsVerticalSectionColumn
        {
            get
            {
                return LayoutIndex == 2;
            }
        }

        /// <summary>
        /// Color emphasis of the column (used for the vertical section column) 
        /// </summary>
        public int? VerticalSectionEmphasis
        {
            get
            {
                if (LayoutIndex == 2)
                {
                    return zoneEmphasis;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (LayoutIndex == 2)
                {
                    if (value < 0 || value > 3)
                    {
                        throw new ArgumentException(PnPCoreResources.Exception_Page_InvalidZoneEmphasis);
                    }

                    zoneEmphasis = value;
                }
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Renders a HTML presentation of this section
        /// </summary>
        /// <returns>The HTML presentation of this section</returns>
        internal string ToHtml()
        {
            StringBuilder html = new StringBuilder();
            bool controlWrittenToSection = false;
            int controlIndex = 0;
            foreach (var control in Section.Page.Controls.Where(p => p.Section == Section && p.Column == this).OrderBy(z => z.Order))
            {
                controlIndex++;
                html.Append((control as CanvasControl).ToHtml(controlIndex));
                controlWrittenToSection = true;
            }

            // if a section does not contain a control we still need to render it, otherwise it get's "lost"
            if (!controlWrittenToSection)
            {
                // Obtain the json data
                var clientSideCanvasPosition = new CanvasData()
                {
                    Position = new CanvasPosition()
                    {
                        ZoneIndex = Section.Order,
                        SectionIndex = Order,
                        SectionFactor = ColumnFactor,
                        LayoutIndex = LayoutIndex,
                    },

                    Emphasis = new SectionEmphasis()
                    {
                        ZoneEmphasis = VerticalSectionEmphasis ?? Section.ZoneEmphasis,
                    }
                };

                if (Section.Collapsible)
                {
                    clientSideCanvasPosition.ZoneGroupMetadata = new SectionZoneGroupMetadata()
                    {
                        IsExpanded = Section.IsExpanded,
                        DisplayName = Section.DisplayName,
                        ShowDividerLine = Section.ShowDividerLine,
                        IconAlignment = Section.IconAlignment == IconAlignment.Right ? "right" : "left",
                        Type = 1,
                    };
                }

                var jsonControlData = JsonSerializer.Serialize(clientSideCanvasPosition);

                html.Append($@"<div {CanvasControlAttribute}="""" {CanvasDataVersionAttribute}=""{DataVersion}"" {ControlDataAttribute}=""{jsonControlData.Replace("\"", "&quot;")}""></div>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Resets the column, used in scenarios where a section is changed from type (e.g. from 3 column to 2 column)
        /// </summary>
        /// <param name="order">Column order to set</param>
        /// <param name="columnFactor">Column factor to set</param>
        public void ResetColumn(int order, int columnFactor)
        {
            Order = order;
            ColumnFactor = columnFactor;
        }

        #region Internal and helper methods
        internal void MoveTo(CanvasSection section)
        {
            Section = section;
        }
        #endregion

        #endregion
    }
}
