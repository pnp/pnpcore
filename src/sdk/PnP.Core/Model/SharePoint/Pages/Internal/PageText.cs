using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Controls of type 4 ( = text control)
    /// </summary>
    internal sealed class PageText : CanvasControl, IPageText
    {
        #region variables
        internal const string TextRteAttribute = "data-sp-rte";
        #endregion

        #region construction
        /// <summary>
        /// Creates a <see cref="PageText"/> instance
        /// </summary>
        internal PageText() : base()
        {
            controlType = 4;
            Rte = "";
            PreviewText = "";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Text value of the client side text control
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Value of the "data-sp-rte" attribute
        /// </summary>
        public string Rte { get; private set; }

        /// <summary>
        /// Text used in page preview in news web part
        /// </summary>
        public string PreviewText { get; private set; }

        /// <summary>
        /// Type of the control (= <see cref="PageText"/>)
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(PageText);
            }
        }

        /// <summary>
        /// Deserialized value of the "data-sp-controldata" attribute
        /// </summary>
        internal TextControlData SpControlData { get; private set; }

        /// <summary>
        /// List of web parts to be added when this text control is added
        /// </summary>
        internal List<PageWebPart> InlineWebParts { get; } = new List<PageWebPart>();
        #endregion

        #region public methods
        /// <summary>
        /// Converts this <see cref="PageText"/> control to it's html representation
        /// </summary>
        /// <param name="controlIndex">The sequence of the control inside the section</param>
        /// <returns>Html representation of this <see cref="PageText"/> control</returns>
        public override string ToHtml(float controlIndex)
        {
            // Can this control be hosted in this section type?
            if (Section.Type == CanvasSectionTemplate.OneColumnFullWidth)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Page_ControlNotAllowedInFullWidthSection);
            }

            // Obtain the json data
            TextControlData controlData = new TextControlData()
            {
                ControlType = ControlType,
                Id = InstanceId.ToString("D"),
                Position = new CanvasControlPosition()
                {
                    ZoneIndex = Section.Order,
                    SectionIndex = Column.Order,
                    SectionFactor = Column.ColumnFactor,
                    LayoutIndex = Column.LayoutIndex,
                    ControlIndex = controlIndex,
                },
                Emphasis = new SectionEmphasis()
                {
                    ZoneEmphasis = Column.VerticalSectionEmphasis ?? Section.ZoneEmphasis,
                },
                EditorType = "CKEditor"
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

            try
            {
                var nodeList = new HtmlParser().ParseFragment(Text, null);
                PreviewText = string.Concat(nodeList.Select(x => x.Text()));
            }
            catch { }

            StringBuilder html = new StringBuilder();
            html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{ DataVersion}""  {ControlDataAttribute}=""{jsonControlData.Replace("\"", "&quot;")}"">");
            html.Append($@"<div {TextRteAttribute}=""{Rte}"">");
            if (!string.IsNullOrEmpty(Text))
            {
                if (Text.Trim().StartsWith("<p>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<h1>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<h2>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<h3>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<h4>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<ul>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<blockquote>", StringComparison.InvariantCultureIgnoreCase) ||
                    Text.Trim().StartsWith("<pre>", StringComparison.InvariantCultureIgnoreCase))
                {
                    html.Append(Text);
                }
                else
                {
                    html.Append($@"<p>{Text}</p>");
                }
            }
            html.Append("</div>");
            html.Append("</div>");
            return html.ToString();
        }        
        #endregion

        #region Internal and private methods
        internal override void FromHtml(IElement element, bool isHeader)
        {
            base.FromHtml(element, isHeader);

            var div = element.GetElementsByTagName("div").FirstOrDefault(a => a.HasAttribute(TextRteAttribute));

            if (div != null)
            {
                Rte = div.GetAttribute(TextRteAttribute);
            }
            else
            {
                // supporting updated rendering of Text controls, no nested DIV tag with the data-sp-rte attribute...so HTML content is embedded at the root
                Rte = "";
                div = element;
            }

            // By default simple plain text is wrapped in a Paragraph, need to drop it to avoid getting multiple paragraphs on page edits.
            // Only drop the paragraph tag when there's only one Paragraph element underneath the DIV tag
            if ((div.FirstChild != null && (div.FirstChild as IElement) != null && (div.FirstChild as IElement).TagName.Equals("P", StringComparison.InvariantCultureIgnoreCase)) &&
                (div.ChildElementCount == 1))
            {
                Text = (div.FirstChild as IElement).InnerHtml;
            }
            else
            {
                Text = div.InnerHtml;
            }

            SpControlData = JsonSerializer.Deserialize<TextControlData>(element.GetAttribute(ControlDataAttribute), PnPConstants.JsonSerializer_IgnoreNullValues);
            controlType = SpControlData.ControlType;
        }
        #endregion
    }
}
