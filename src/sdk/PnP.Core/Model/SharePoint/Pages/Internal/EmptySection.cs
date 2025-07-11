using AngleSharp.Dom;
using System;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// This class is used to instantiate controls of type 1 (= emptySection).
    /// </summary>
    internal sealed class EmptySection : CanvasControl, IEmptySection
    {
        public EmptySection() : base()
        {
            var emptyJson = JsonSerializer.Deserialize<JsonElement>("{}");

            controlType = 1; // emptySection
        }

        /// <summary>
        /// Return <see cref="Type"/> of the client side web part
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(EmptySection);
            }
        }

        /// <summary>
        /// Deserialized value of the "data-sp-controldata" attribute
        /// </summary>
        internal CanvasControlData SpControlData { get; private set; }

        #region public methods
        public override string ToHtml(float controlIndex)
        {
            CanvasControlData controlData = new CanvasControlData
            {
                ControlType = ControlType,
                Id = "emptySection",//for other webParts this is a Guid but here it seems to be a static value 
                Position = new CanvasControlPosition()
                {
                    ZoneIndex = Section.Order,
                    SectionIndex = Column.Order,
                    SectionFactor = Column.ColumnFactor,
                    LayoutIndex = Column.LayoutIndex,
                    ControlIndex = controlIndex,
                    ZoneId = column.ZoneId,
                    IsLayoutReflowOnTop = Column.IsLayoutReflowOnTop
                },
                Emphasis = new SectionEmphasis()
                {
                    ZoneEmphasis = Column.VerticalSectionEmphasis ?? Section.ZoneEmphasis,
                }
            };

            // Persist the collapsible section settings
            if (Section.Collapsible && !Column.IsVerticalSectionColumn)
            {
                controlData.ZoneGroupMetadata = new SectionZoneGroupMetadata()
                {
                    // Set section type to 1 if it was not set (when new sections are added via code)
                    Type = (Section as CanvasSection).SectionType == 0 ? 1 : (Section as CanvasSection).SectionType,
                    DisplayName = Section.DisplayName,
                    IsExpanded = Section.IsExpanded,
                    ShowDividerLine = Section.ShowDividerLine,
                };

                if (Section.IconAlignment.HasValue)
                {
                    controlData.ZoneGroupMetadata.IconAlignment = Section.IconAlignment.Value.ToString().ToLower();
                }
                else
                {
                    controlData.ZoneGroupMetadata.IconAlignment = "true";
                }
            }

            jsonControlData = JsonSerializer.Serialize(controlData);

            StringBuilder html = new StringBuilder();
            html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{DataVersion}""  {ControlDataAttribute}=""{jsonControlData.Replace("\"", "&quot;")}"">");
            html.Append("</div>");
            return html.ToString();
        }
        #endregion

        #region Internal and private methods
        internal override void FromHtml(IElement element, bool isHeader)
        {
            base.FromHtml(element, isHeader);
            SpControlData = JsonSerializer.Deserialize<TextControlData>(element.GetAttribute(ControlDataAttribute), PnPConstants.JsonSerializer_IgnoreNullValues);
            controlType = SpControlData.ControlType;
        }
        #endregion

    }
}