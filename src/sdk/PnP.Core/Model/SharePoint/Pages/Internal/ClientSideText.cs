using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Controls of type 4 ( = text control)
    /// </summary>
    internal class ClientSideText : CanvasControl, IClientSideText
    {
        #region variables
        internal const string TextRteAttribute = "data-sp-rte";
        #endregion

        #region construction
        /// <summary>
        /// Creates a <see cref="ClientSideText"/> instance
        /// </summary>
        internal ClientSideText() : base()
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
        /// Type of the control (= <see cref="ClientSideText"/>)
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(ClientSideText);
            }
        }

        /// <summary>
        /// Deserialized value of the "data-sp-controldata" attribute
        /// </summary>
        internal ClientSideTextControlData SpControlData { get; private set; }
        #endregion

        #region public methods
        /// <summary>
        /// Converts this <see cref="ClientSideText"/> control to it's html representation
        /// </summary>
        /// <param name="controlIndex">The sequence of the control inside the section</param>
        /// <returns>Html representation of this <see cref="ClientSideText"/> control</returns>
        public override string ToHtml(float controlIndex)
        {
            // Can this control be hosted in this section type?
            if (Section.Type == CanvasSectionTemplate.OneColumnFullWidth)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Page_ControlNotAllowedInFullWidthSection);
            }

            // Obtain the json data
            ClientSideTextControlData controlData = new ClientSideTextControlData()
            {
                ControlType = ControlType,
                Id = InstanceId.ToString("D"),
                Position = new ClientSideCanvasControlPosition()
                {
                    ZoneIndex = Section.Order,
                    SectionIndex = Column.Order,
                    SectionFactor = Column.ColumnFactor,
                    LayoutIndex = Column.LayoutIndex,
                    ControlIndex = controlIndex,
                },
                Emphasis = new ClientSideSectionEmphasis()
                {
                    ZoneEmphasis = Column.VerticalSectionEmphasis.HasValue ? Column.VerticalSectionEmphasis.Value : Section.ZoneEmphasis,
                },
                EditorType = "CKEditor"
            };


            if (section.Type == CanvasSectionTemplate.OneColumnVerticalSection)
            {
                if (section.Columns.First().Equals(Column))
                {
                    controlData.Position.SectionFactor = 12;
                }
            }

            jsonControlData = JsonSerializer.Serialize(controlData);

            try
            {
                var nodeList = new HtmlParser().ParseFragment(Text, null);
                PreviewText = string.Concat(nodeList.Select(x => x.Text()));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch { }
#pragma warning restore CA1031 // Do not catch general exception types

            StringBuilder html = new StringBuilder(100);
            html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{ DataVersion}""  {ControlDataAttribute}=""{jsonControlData.Replace("\"", "&quot;")}"">");
            html.Append($@"<div {TextRteAttribute}=""{Rte}"">");
            if (Text.Trim().StartsWith("<p>", StringComparison.InvariantCultureIgnoreCase))
            {
                html.Append(Text);
            }
            else
            {
                html.Append($@"<p>{Text}</p>");
            }
            html.Append("</div>");
            html.Append("</div>");
            return html.ToString();
        }
        #endregion

        #region Internal and private methods
        internal override void FromHtml(IElement element)
        {
            base.FromHtml(element);

            var div = element.GetElementsByTagName("div").Where(a => a.HasAttribute(TextRteAttribute)).FirstOrDefault();

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

            SpControlData = JsonSerializer.Deserialize<ClientSideTextControlData>(element.GetAttribute(CanvasControl.ControlDataAttribute), new JsonSerializerOptions() { IgnoreNullValues = true });
            controlType = SpControlData.ControlType;
        }
        #endregion
    }
}
