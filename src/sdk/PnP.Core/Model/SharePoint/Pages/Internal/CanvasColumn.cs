using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class CanvasColumn :  ICanvasColumn
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
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            this.Section = section;
            this.ColumnFactor = 12;
            this.Order = 0;
            this.LayoutIndex = 1;
        }

        internal CanvasColumn(CanvasSection section, int order)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            this.Section = section;
            this.Order = order;
            this.LayoutIndex = 1;
        }

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            this.Section = section;
            this.Order = order;
            this.ColumnFactor = sectionFactor.HasValue ? sectionFactor.Value : 12;
            this.LayoutIndex = 1;
        }

        internal CanvasColumn(CanvasSection section, int order, int? sectionFactor, int? layoutIndex)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            this.Section = section;
            this.Order = order;
            this.ColumnFactor = sectionFactor.HasValue ? sectionFactor.Value : 12;
            this.LayoutIndex = layoutIndex.HasValue ? layoutIndex.Value : 1;
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
                return this.Section.Page.Controls.Where(p => p.Section == this.Section && p.Column == this).ToList();
            }
        }

        /// <summary>
        /// Is this a vertical section column?
        /// </summary>
        public bool IsVerticalSectionColumn
        {
            get
            {
                return this.LayoutIndex == 2;
            }
        }

        /// <summary>
        /// Color emphasis of the column (used for the vertical section column) 
        /// </summary>
        public int? VerticalSectionEmphasis
        {
            get
            {
                if (this.LayoutIndex == 2)
                {
                    return this.zoneEmphasis;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.LayoutIndex == 2)
                {
                    if (value < 0 || value > 3)
                    {
                        throw new ArgumentException($"The zoneEmphasis value needs to be between 0 and 3. See the Microsoft.SharePoint.Client.SPVariantThemeType values for the why.");
                    }

                    this.zoneEmphasis = value;
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
            StringBuilder html = new StringBuilder(100);
            bool controlWrittenToSection = false;
            int controlIndex = 0;
            foreach (var control in this.Section.Page.Controls.Where(p => p.Section == this.Section && p.Column == this).OrderBy(z => z.Order))
            {
                controlIndex++;
                html.Append((control as CanvasControl).ToHtml(controlIndex));
                controlWrittenToSection = true;
            }

            // if a section does not contain a control we still need to render it, otherwise it get's "lost"
            if (!controlWrittenToSection)
            {
                // Obtain the json data
                var clientSideCanvasPosition = new ClientSideCanvasData()
                {
                    Position = new ClientSideCanvasPosition()
                    {
                        ZoneIndex = this.Section.Order,
                        SectionIndex = this.Order,
                        SectionFactor = this.ColumnFactor,
                        LayoutIndex = this.LayoutIndex,
                    },

                    Emphasis = new ClientSideSectionEmphasis()
                    {
                        ZoneEmphasis = this.Section.ZoneEmphasis,
                    }
                };

                //var jsonControlData = JsonConvert.SerializeObject(clientSideCanvasPosition);
                var jsonControlData = JsonSerializer.Serialize(clientSideCanvasPosition);

                html.Append($@"<div {CanvasControlAttribute}="""" {CanvasDataVersionAttribute}=""{this.DataVersion}"" {ControlDataAttribute}=""{jsonControlData.Replace("\"", "&quot;")}""></div>");
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
            this.Order = order;
            this.ColumnFactor = columnFactor;
        }

        #region Internal and helper methods
        internal void MoveTo(CanvasSection section)
        {
            this.Section = section;
        }
        #endregion

        #endregion
    }
}
