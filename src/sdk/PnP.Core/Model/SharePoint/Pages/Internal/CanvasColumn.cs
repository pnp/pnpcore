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
        private bool? isLayoutReflowOnTop;
        private ZoneReflowStrategy? zoneReflowStrategy;
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

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor, string zoneId = null)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Order = order;
            ColumnFactor = sectionFactor ?? 12;
            LayoutIndex = 1;
            ZoneId = !string.IsNullOrEmpty(zoneId) ? zoneId : null;
        }

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor, int? layoutIndex, string zoneId = null)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Order = order;
            ColumnFactor = sectionFactor ?? 12;
            LayoutIndex = layoutIndex ?? 1;
            ZoneId = !string.IsNullOrEmpty(zoneId) ? zoneId : null;
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

        public string ZoneId { get; private set; }

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

        public bool? IsLayoutReflowOnTop
        {
            get
            {
                if (LayoutIndex == 2)
                {
                    return isLayoutReflowOnTop;
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
                    isLayoutReflowOnTop = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the strategy used to reflow content within a zone.
        /// </summary>
        public ZoneReflowStrategy? ZoneReflowStrategy
        {
            get
            {
                if (Section.Type == CanvasSectionTemplate.FlexibleLayoutSection || Section.Type == CanvasSectionTemplate.FlexibleLayoutVerticalSection)
                    return zoneReflowStrategy;
                else
                    return null;
            }

            set
            {
                zoneReflowStrategy = value;
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
                var emptySection = new EmptySection(Section, this);
                controlIndex++;
                html.Append(emptySection.ToHtml(controlIndex));
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

        /// <summary>
        /// Sets the zone id
        /// </summary>
        /// <param name="zoneId"></param>
        public void SetZoneId(string zoneId)
        {
            ZoneId = zoneId;
        }

        /// <summary>
        /// Set IsLayoutReflowOnTop
        /// </summary>
        /// <param name="layoutReflowOnTop"></param>
        public void SetIsLayoutReflowOnTop(bool layoutReflowOnTop)
        {
            IsLayoutReflowOnTop = layoutReflowOnTop;
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
