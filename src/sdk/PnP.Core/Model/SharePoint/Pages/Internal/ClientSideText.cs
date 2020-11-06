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
            this.controlType = 4;
            this.Rte = "";
            this.PreviewText = "";
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
            if (this.Section.Type == CanvasSectionTemplate.OneColumnFullWidth)
            {
                throw new Exception("You cannot host text controls inside a one column full width section, only an image web part or hero web part are allowed");
            }

            // Obtain the json data
            ClientSideTextControlData controlData = new ClientSideTextControlData()
            {
                ControlType = this.ControlType,
                Id = this.InstanceId.ToString("D"),
                Position = new ClientSideCanvasControlPosition()
                {
                    ZoneIndex = this.Section.Order,
                    SectionIndex = this.Column.Order,
                    SectionFactor = this.Column.ColumnFactor,
                    LayoutIndex = this.Column.LayoutIndex,
                    ControlIndex = controlIndex,
                },
                Emphasis = new ClientSideSectionEmphasis()
                {
                    ZoneEmphasis = this.Column.VerticalSectionEmphasis.HasValue ? this.Column.VerticalSectionEmphasis.Value : this.Section.ZoneEmphasis,
                },
                EditorType = "CKEditor"
            };


            if (this.section.Type == CanvasSectionTemplate.OneColumnVerticalSection)
            {
                if (this.section.Columns.First().Equals(this.Column))
                {
                    controlData.Position.SectionFactor = 12;
                }
            }

            //jsonControlData = JsonConvert.SerializeObject(controlData);
            jsonControlData = JsonSerializer.Serialize(controlData);

            try
            {
                var nodeList = new HtmlParser().ParseFragment(this.Text, null);
                this.PreviewText = string.Concat(nodeList.Select(x => x.Text()));
            }
            catch { }

            StringBuilder html = new StringBuilder(100);
            html.Append($@"<div {CanvasControlAttribute}=""{this.CanvasControlData}"" {CanvasDataVersionAttribute}=""{ this.DataVersion}""  {ControlDataAttribute}=""{this.jsonControlData.Replace("\"", "&quot;")}"">");
            html.Append($@"<div {TextRteAttribute}=""{this.Rte}"">");
            if (this.Text.Trim().StartsWith("<p>", StringComparison.InvariantCultureIgnoreCase))
            {
                html.Append(this.Text);
            }
            else
            {
                html.Append($@"<p>{this.Text}</p>");
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
                this.Rte = div.GetAttribute(TextRteAttribute);
            }
            else
            {
                // supporting updated rendering of Text controls, no nested DIV tag with the data-sp-rte attribute...so HTML content is embedded at the root
                this.Rte = "";
                div = element;
            }

            // By default simple plain text is wrapped in a Paragraph, need to drop it to avoid getting multiple paragraphs on page edits.
            // Only drop the paragraph tag when there's only one Paragraph element underneath the DIV tag
            if ((div.FirstChild != null && (div.FirstChild as IElement) != null && (div.FirstChild as IElement).TagName.Equals("P", StringComparison.InvariantCultureIgnoreCase)) &&
                (div.ChildElementCount == 1))
            {
                this.Text = (div.FirstChild as IElement).InnerHtml;
            }
            else
            {
                this.Text = div.InnerHtml;
            }

            // load data from the data-sp-controldata attribute
            //var jsonSerializerSettings = new JsonSerializerSettings()
            //{
            //    MissingMemberHandling = MissingMemberHandling.Ignore
            //};
            var jsonSerializerSettings = new JsonSerializerOptions() { IgnoreNullValues = true };

            //this.spControlData = JsonConvert.DeserializeObject<ClientSideTextControlData>(element.GetAttribute(CanvasControl.ControlDataAttribute), jsonSerializerSettings);
            this.SpControlData = JsonSerializer.Deserialize<ClientSideTextControlData>(element.GetAttribute(CanvasControl.ControlDataAttribute), jsonSerializerSettings);
            this.controlType = this.SpControlData.ControlType;
        }
        #endregion
    }
}
