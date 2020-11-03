using AngleSharp.Html.Parser;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ListItem", Uri = "_api/web/lists/getbytitle('Site Pages')/items({Id})", LinqGet = "_api/web/lists/getbytitle('Site Pages')/items")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Page : ListItemBase<IPage>, IPage
    {
        private bool isDefaultDescription;
        private string pageTitle;

        #region Graph/Rest interoperability overrides
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async override Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (IsPropertyAvailable(p => p.Id) && Id > 0)
            {
                if (!Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                {
                    Metadata.Add(PnPConstants.MetaDataRestId, Id.ToString());
                }

                if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                {
                    Metadata.Add(PnPConstants.MetaDataGraphId, Id.ToString());
                }
            }
        }

        #endregion

        #region Page Properties
        /// <summary>
        /// Collection of sections that exist on this client side page
        /// </summary>
        private readonly List<CanvasSection> sections = new List<CanvasSection>(1);
        [SystemProperty]
        public List<ICanvasSection> Sections
        {
            get
            {
                return this.sections.Cast<ICanvasSection>().ToList();
            }
        }

        /// <summary>
        /// Collection of all control that exist on this client side page
        /// </summary>
        private readonly List<ICanvasControl> controls = new List<ICanvasControl>(5);
        [SystemProperty]
        public List<ICanvasControl> Controls
        {
            get
            {
                return this.controls;
            }
        }

        /// <summary>
        /// Collection of all header control that exist on this client side page
        /// </summary>
        private readonly List<ICanvasControl> headerControls = new List<ICanvasControl>();
        [SystemProperty]
        public List<ICanvasControl> HeaderControls
        {
            get
            {
                return this.headerControls;
            }
        }

        /// <summary>
        /// Layout type of the client side page
        /// </summary>
        private PageLayoutType layoutType;
        [SystemProperty]
        public PageLayoutType LayoutType
        {
            get
            {
                return this.layoutType;
            }
            set
            {
                this.layoutType = value;
            }
        }

        private string thumbnailUrl;
        [SystemProperty]
        public string ThumbnailUrl
        {
            get
            {
                return this.thumbnailUrl;
            }
            set
            {
                this.thumbnailUrl = value;
            }
        }

        /// <summary>
        /// When a page of type Home is created you can opt to only keep the default client side web parts by setting this to true. This also is a way to reset your home page back the the stock one.
        /// </summary>
        private bool keepDefaultWebParts;
        [SystemProperty]
        public bool KeepDefaultWebParts
        {
            get
            {
                return this.keepDefaultWebParts;
            }
            set
            {
                this.keepDefaultWebParts = value;
            }
        }

        /// <summary>
        /// The default section of the client side page
        /// </summary>
        [SystemProperty]
        public CanvasSection DefaultSection
        {
            get
            {
                if (!Debugger.IsAttached)
                {
                    // Add a default section if there wasn't one yet created
                    if (this.sections.Count == 0)
                    {
                        this.sections.Add(new CanvasSection(this, CanvasSectionTemplate.OneColumn, 0));
                    }

                    return sections.First();
                }
                else
                {
                    if (this.sections.Count > 0)
                    {
                        return sections.First();
                    }
                    else
                    {
                        if (this.sections.Count == 0)
                        {
                            this.sections.Add(new CanvasSection(this, CanvasSectionTemplate.OneColumn, 0));
                        }

                        return sections.First();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the page header for this page
        /// </summary>
        private ClientSidePageHeader pageHeader;
        [SystemProperty]
        public ClientSidePageHeader PageHeader
        {
            get
            {
                return this.pageHeader;
            }
        }

        /// <summary>
        /// ID value of the page (only available when the page was saved)
        /// </summary>
        private int? pageId;
        [SystemProperty]
        public int? PageId
        {
            get
            {
                return this.pageId;
            }
        }

        /// <summary>
        /// Space content field (JSON) for spaces pages
        /// </summary>
        [SystemProperty]
        public string SpaceContent { get; set; }

        /// <summary>
        /// Entity id field for topic pages
        /// </summary>
        [SystemProperty]
        public string EntityId { get; set; }

        /// <summary>
        /// Entity relations field for topic pages
        /// </summary>
        [SystemProperty]
        public string EntityRelations { get; set; }

        /// <summary>
        /// Entity type field for topic pages
        /// </summary>
        [SystemProperty]
        public string EntityType { get; set; }
        #endregion

        #region Page Methods

        #region Section, column and control methods
        /// <summary>
        /// Clears all control and sections from this page
        /// </summary>
        public void ClearPage()
        {
            foreach (var section in this.sections)
            {
                foreach (var control in section.Controls)
                {
                    (control as CanvasControl).Delete();
                }
            }

            this.sections.Clear();
        }

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="template">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        /// <param name="zoneEmphasis">Zone emphasis (section background)</param>
        /// <param name="verticalSectionZoneEmphasis">Vertical Section Zone emphasis (section background)</param>
        public void AddSection(CanvasSectionTemplate template, float order, VariantThemeType zoneEmphasis, VariantThemeType verticalSectionZoneEmphasis = VariantThemeType.None)
        {
            AddSection(template, order, (int)zoneEmphasis, (int)verticalSectionZoneEmphasis);
        }

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="template">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        /// <param name="zoneEmphasis">Zone emphasis (section background)</param>
        /// <param name="verticalSectionZoneEmphasis">Vertical Section Zone emphasis (section background)</param>
        public void AddSection(CanvasSectionTemplate template, float order, int zoneEmphasis, int? verticalSectionZoneEmphasis = null)
        {
            var section = new CanvasSection(this, template, order)
            {
                ZoneEmphasis = zoneEmphasis,
            };
            if (section.VerticalSectionColumn != null && verticalSectionZoneEmphasis.HasValue)
            {
                (section.VerticalSectionColumn as CanvasColumn).VerticalSectionEmphasis = verticalSectionZoneEmphasis;
            }
            AddSection(section);
        }

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="template">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        public void AddSection(CanvasSectionTemplate template, float order)
        {
            var section = new CanvasSection(this, template, order);
            AddSection(section);
        }

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="section"><see cref="CanvasSection"/> object describing the section to add</param>
        public void AddSection(CanvasSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (CanThisSectionBeAdded(section))
            {
                this.sections.Add(section);
            }
        }

        /// <summary>
        /// Adds a new section to your client side page with a given order
        /// </summary>
        /// <param name="section"><see cref="CanvasSection"/> object describing the section to add</param>
        /// <param name="order">Controls the order of the new section</param>
        public void AddSection(CanvasSection section, float order)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (CanThisSectionBeAdded(section))
            {
                section.Order = order;
                this.sections.Add(section);
            }
        }

        private bool CanThisSectionBeAdded(CanvasSection section)
        {
            if (this.Sections.Count == 0)
            {
                return true;
            }

            if (section.VerticalSectionColumn != null)
            {
                // we're adding a section that has a vertical column. This can not be combined with either another section with a vertical column or a full width section
                if (this.Sections.Where(p => p.VerticalSectionColumn != null).Any())
                {
                    throw new Exception("You can only have one section with a vertical column on a page");
                }

                if (this.Sections.Where(p => p.Type == CanvasSectionTemplate.OneColumnFullWidth).Any())
                {
                    throw new Exception("You already have a full width section on this page, you can't add a section with vertical column");
                }
            }

            if (section.Type == CanvasSectionTemplate.OneColumnFullWidth)
            {
                // we're adding a full width section. This can not be combined with either a vertical column section
                if (this.Sections.Where(p => p.VerticalSectionColumn != null).Any())
                {
                    throw new Exception("You already have a section with vertical column on this page, you can't add a full width section");
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a new control to your client side page using the default <see cref="CanvasSection"/>
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        public void AddControl(CanvasControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            // add to defaultsection and column
            if (control.Section == null)
            {
                control.section = this.DefaultSection;
            }
            if (control.Column == null)
            {
                control.column = this.DefaultSection.DefaultColumn;
            }

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new control to your client side page using the default <see cref="CanvasSection"/> using a given order
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="order">Order of the control in the default section</param>
        public void AddControl(CanvasControl control, int order)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            // add to default section and column
            if (control.Section == null)
            {
                control.section = this.DefaultSection;
            }
            if (control.Column == null)
            {
                control.column = this.DefaultSection.DefaultColumn;
            }
            control.Order = order;

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new control to your client side page in the given section
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="section"><see cref="CanvasSection"/> that will hold the control. Control will end up in the <see cref="CanvasSection.DefaultColumn"/>.</param>
        public void AddControl(CanvasControl control, CanvasSection section)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            control.section = section;
            control.column = section.DefaultColumn;

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new control to your client side page in the given section with a given order
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="section"><see cref="CanvasSection"/> that will hold the control. Control will end up in the <see cref="CanvasSection.DefaultColumn"/>.</param>
        /// <param name="order">Order of the control in the given section</param>
        public void AddControl(CanvasControl control, CanvasSection section, int order)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            control.section = section;
            control.column = section.DefaultColumn;
            control.Order = order;

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new control to your client side page in the given section
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="column"><see cref="CanvasColumn"/> that will hold the control</param>    
        public void AddControl(CanvasControl control, CanvasColumn column)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            control.section = column.Section;
            control.column = column;

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new control to your client side page in the given section with a given order
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="column"><see cref="CanvasColumn"/> that will hold the control</param>    
        /// <param name="order">Order of the control in the given section</param>
        public void AddControl(CanvasControl control, CanvasColumn column, int order)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            control.section = column.Section;
            control.column = column;
            control.Order = order;

            this.controls.Add(control);
        }

        /// <summary>
        /// Adds a new header control to your client side page with a given order
        /// </summary>
        /// <param name="control"><see cref="CanvasControl"/> to add</param>
        /// <param name="order">Order of the control in the given section</param>
        public void AddHeaderControl(CanvasControl control, int order)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            control.section = this.DefaultSection;
            control.column = this.DefaultSection.DefaultColumn;
            control.Order = order;

            this.headerControls.Add(control);
        }
        #endregion

        #region To Html
        internal string HeaderControlsToHtml()
        {
            if (headerControls.Any())
            {
                StringBuilder html = new StringBuilder();

                float order = 1;
                foreach (var headerControl in headerControls)
                {
                    html.Append((headerControl as CanvasControl).ToHtml(order));
                }

                return html.ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Returns the html representation of this client side page. This is the content that will be persisted in the <see cref="ClientSidePage.PageListItem"/> list item.
        /// </summary>
        /// <returns>Html representation</returns>
        public string ToHtml()
        {
            StringBuilder html = new StringBuilder(100);

            if (this.sections.Count == 0) return string.Empty;

            html.Append($@"<div>");
            // Normalize section order by starting from 1, users could have started from 0 or left gaps in the numbering
            var sectionsToOrder = this.sections.OrderBy(p => p.Order).ToList();
            int i = 1;
            foreach (var section in sectionsToOrder)
            {
                section.Order = i;
                i++;
            }

            foreach (var section in this.sections.OrderBy(p => p.Order))
            {
                html.Append(section.ToHtml());

            }
            // Thumbnail
            var thumbnailData = new { controlType = 0, pageSettingsSlice = new { isDefaultDescription = this.isDefaultDescription, isDefaultThumbnail = string.IsNullOrEmpty(thumbnailUrl) } };
            html.Append($@"<div data-sp-canvascontrol="""" data-sp-canvasdataversion=""1.0"" data-sp-controldata=""{JsonSerializer.Serialize(thumbnailData).Replace("\"", "&quot;")}""></div>");

            html.Append("</div>");
            return html.ToString();
        }
        #endregion

        #region From Html

        internal void InitializePage()
        {
            this.pageHeader = new ClientSidePageHeader(PnPContext, PageHeaderType.Default, null);
        }

        public async Task LoadAsync()
        {
            InitializePage();

            var pagesLibrary = await PnPContext.Web.Lists.GetByTitleAsync("Site Pages").ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            sb.Append("<View><ViewFields>");
            sb.Append($"<FieldRef Name='{PageConstants.Title}' />");
            sb.Append($"<FieldRef Name='{PageConstants.ClientSideApplicationId}' />");
            sb.Append($"<FieldRef Name='{PageConstants.PageLayoutType}' />");
            sb.Append($"<FieldRef Name='{PageConstants.SpaceContentField}' />");
            sb.Append($"<FieldRef Name='{PageConstants.TopicEntityId}' />");
            sb.Append($"<FieldRef Name='{PageConstants.TopicEntityType}' />");
            sb.Append($"<FieldRef Name='{PageConstants.TopicEntityRelations}' />");
            sb.Append($"<FieldRef Name='{PageConstants.CanvasField}' />");
            sb.Append($"<FieldRef Name='{PageConstants.PageLayoutContentField}' />");
            sb.Append("</ViewFields></View>");

            var listData = await pagesLibrary.GetListDataAsStreamAsync(new RenderListDataOptions() 
            { 
                ViewXml = sb.ToString(), 
                RenderOptions = RenderListDataOptionsFlags.ListData 
            }).ConfigureAwait(false);    
            
            var item = pagesLibrary.Items.FirstOrDefault(p => p.Title == Title);

            if (item.Values.ContainsKey(PageConstants.ClientSideApplicationId) && item.Values[PageConstants.ClientSideApplicationId] != null && item.Values[PageConstants.ClientSideApplicationId].ToString().Equals(PageConstants.SitePagesFeatureId, StringComparison.InvariantCultureIgnoreCase))
            {
                pageTitle = Convert.ToString(item.Values[PageConstants.Title]);

                //if (int.TryParse(Values[PageConstants.IdField].ToString(), out int pageIdValue))
                //{
                //    pageId = pageIdValue;
                //}

                pageId = Id;

                // set layout type
                if (item.Values.ContainsKey(PageConstants.PageLayoutType) && item.Values[PageConstants.PageLayoutType] != null && !string.IsNullOrEmpty(item.Values[PageConstants.PageLayoutType].ToString()))
                {
                    if (item.Values[PageConstants.PageLayoutType].ToString().Equals(PageConstants.SpacesLayoutType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        LayoutType = PageLayoutType.Spaces;
                    }
                    else if (item.Values[PageConstants.PageLayoutType].ToString().Equals(PageConstants.TopicLayoutType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        LayoutType = PageLayoutType.Topic;
                    }
                    else
                    {
                        LayoutType = (PageLayoutType)Enum.Parse(typeof(PageLayoutType), item.Values[PageConstants.PageLayoutType].ToString());
                    }
                }
                else
                {
                    throw new Exception($"Page layout type could not be determined for page");
                }

                if (LayoutType == PageLayoutType.Spaces)
                {
                    if (item.Values.ContainsKey(PageConstants.SpaceContentField) && item.Values[PageConstants.SpaceContentField] != null && !string.IsNullOrEmpty(item.Values[PageConstants.SpaceContentField].ToString()))
                    {
                        SpaceContent = item.Values[PageConstants.SpaceContentField].ToString();
                    }
                }

                if (LayoutType == PageLayoutType.Topic)
                {
                    if (item.Values.ContainsKey(PageConstants.TopicEntityId) && item.Values[PageConstants.TopicEntityId] != null && !string.IsNullOrEmpty(item.Values[PageConstants.TopicEntityId].ToString()))
                    {
                        EntityId = item.Values[PageConstants.TopicEntityId].ToString();
                    }

                    if (item.Values.ContainsKey(PageConstants.TopicEntityRelations) && item.Values[PageConstants.TopicEntityRelations] != null && !string.IsNullOrEmpty(item.Values[PageConstants.TopicEntityRelations].ToString()))
                    {
                        EntityRelations = item.Values[PageConstants.TopicEntityRelations].ToString();
                    }

                    if (item.Values.ContainsKey(PageConstants.TopicEntityType) && item.Values[PageConstants.TopicEntityType] != null && !string.IsNullOrEmpty(item.Values[PageConstants.TopicEntityType].ToString()))
                    {
                        EntityType = item.Values[PageConstants.TopicEntityType].ToString();
                    }
                }

                // default canvas content for an empty page (this field contains the page's web part properties)
                var canvasContent1Html = @"<div><div data-sp-canvascontrol="""" data-sp-canvasdataversion=""1.0"" data-sp-controldata=""&#123;&quot;controlType&quot;&#58;0,&quot;pageSettingsSlice&quot;&#58;&#123;&quot;isDefaultDescription&quot;&#58;true,&quot;isDefaultThumbnail&quot;&#58;true&#125;&#125;""></div></div>";
                // If the canvasfield1 field is present and filled then let's parse it
                if (item.Values.ContainsKey(PageConstants.CanvasField) && !(item.Values[PageConstants.CanvasField] == null || string.IsNullOrEmpty(item.Values[PageConstants.CanvasField].ToString())))
                {
                    canvasContent1Html = item.Values[PageConstants.CanvasField].ToString();
                }
                var pageHeaderHtml = item.Values[PageConstants.PageLayoutContentField] != null ? item.Values[PageConstants.PageLayoutContentField].ToString() : "";
                LoadFromHtml(canvasContent1Html, pageHeaderHtml);
            }
        }

        private void LoadFromHtml(string html, string pageHeaderHtml)
        {
            if (String.IsNullOrEmpty(html))
            {
                throw new ArgumentException("Passed html cannot be null or empty");
            }

            HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });
            using (var document = parser.ParseDocument(html))
            {
                // select all control div's
                var clientSideControls = document.All.Where(m => m.HasAttribute(CanvasControl.ControlDataAttribute));

                // clear sections as we're constructing them from the loaded html
                this.sections.Clear();

                int controlOrder = 0;
                foreach (var clientSideControl in clientSideControls)
                {
                    var controlData = clientSideControl.GetAttribute(CanvasControl.ControlDataAttribute);
                    var controlType = CanvasControl.GetType(controlData);

                    if (controlType == typeof(ClientSideText))
                    {
                        var control = new ClientSideText()
                        {
                            Order = controlOrder
                        };
                        control.FromHtml(clientSideControl);

                        // Handle control positioning in sections and columns
                        ApplySectionAndColumn(control, control.SpControlData.Position, control.SpControlData.Emphasis);

                        this.AddControl(control);
                    }
                    else if (controlType == typeof(ClientSideWebPart))
                    {
                        var control = new ClientSideWebPart()
                        {
                            Order = controlOrder
                        };
                        control.FromHtml(clientSideControl);

                        // Handle control positioning in sections and columns
                        ApplySectionAndColumn(control, control.SpControlData.Position, control.SpControlData.Emphasis);

                        this.AddControl(control);
                    }
                    else if (controlType == typeof(CanvasColumn))
                    {
                        // Need to parse empty sections
                        //var jsonSerializerSettings = new JsonSerializerSettings()
                        //{
                        //    MissingMemberHandling = MissingMemberHandling.Ignore
                        //};
                        var jsonSerializerSettings = new JsonSerializerOptions() { IgnoreNullValues = true };
                        //var sectionData = JsonConvert.DeserializeObject<ClientSideCanvasData>(controlData, jsonSerializerSettings);
                        var sectionData = JsonSerializer.Deserialize<ClientSideCanvasData>(controlData, jsonSerializerSettings);

                        CanvasSection currentSection = null;
                        if (sectionData.Position != null)
                        {
                            currentSection = this.sections.Where(p => p.Order == sectionData.Position.ZoneIndex).FirstOrDefault();
                        }

                        if (currentSection == null)
                        {
                            if (sectionData.Position != null)
                            {
                                this.AddSection(new CanvasSection(this) { ZoneEmphasis = sectionData.Emphasis != null ? sectionData.Emphasis.ZoneEmphasis : 0 }, sectionData.Position.ZoneIndex);
                                currentSection = this.sections.Where(p => p.Order == sectionData.Position.ZoneIndex).First();
                            }
                        }

                        ICanvasColumn currentColumn = null;
                        if (sectionData.Position != null)
                        {
                            currentColumn = currentSection.Columns.Where(p => p.Order == sectionData.Position.SectionIndex).FirstOrDefault();
                        }

                        if (currentColumn == null)
                        {
                            if (sectionData.Position != null)
                            {
                                currentSection.AddColumn(new CanvasColumn(currentSection, sectionData.Position.SectionIndex, sectionData.Position.SectionFactor));
                                currentColumn = currentSection.Columns.Where(p => p.Order == sectionData.Position.SectionIndex).First();
                            }
                        }

                        if (sectionData.PageSettingsSlice != null)
                        {
                            if (sectionData.PageSettingsSlice.IsDefaultThumbnail.HasValue)
                            {
                                if (sectionData.PageSettingsSlice.IsDefaultThumbnail == false)
                                {
                                    // TODO
                                    //this.thumbnailUrl = Values[PageConstants.BannerImageUrlField] != null ? ((FieldUrlValue)this.PageListItem[BannerImageUrlField]).Url : string.Empty;
                                    this.thumbnailUrl = Values[PageConstants.BannerImageUrlField] != null ? Values[PageConstants.BannerImageUrlField].ToString() : string.Empty;
                                }
                            }
                            if (sectionData.PageSettingsSlice.IsDefaultDescription.HasValue)
                            {
                                this.isDefaultDescription = sectionData.PageSettingsSlice.IsDefaultDescription.Value;
                            }
                        }
                    }

                    controlOrder++;
                }
            }

            // Perform vertical section column matchup if that did not happen yet
            var verticalSectionColumn = this.sections.Where(p => p.VerticalSectionColumn != null).FirstOrDefault();
            // Only continue if the vertical section column we found was "standalone" and not yet matched with other columns
            if (verticalSectionColumn != null && verticalSectionColumn.Columns.Count == 1)
            {
                // find another, non vertical section, column with the same zoneindex
                var matchedUpSection = this.sections.Where(p => p.VerticalSectionColumn == null && p.Order == verticalSectionColumn.Order).FirstOrDefault();
                if (matchedUpSection == null)
                {
                    // matchup did not yet happen, so let's handle it now
                    // Get the top section
                    var topSection = this.sections.Where(p => p.VerticalSectionColumn == null).OrderBy(p => p.Order).FirstOrDefault();
                    if (topSection != null)
                    {
                        // Add the "standalone" vertical section column to this section
                        (topSection as CanvasSection).MergeVerticalSectionColumn(verticalSectionColumn.Columns[0] as CanvasColumn);

                        // Move the controls to the new section/column
                        var controlsToMove = this.controls.Where(p => p.Section == verticalSectionColumn);
                        if (controlsToMove.Any())
                        {
                            foreach (var controlToMove in controlsToMove)
                            {
                                (controlToMove as CanvasControl).MoveTo(topSection, topSection.VerticalSectionColumn);
                            }
                        }

                        // Remove the "standalone" vertical section column section
                        this.sections.Remove(verticalSectionColumn);
                    }
                }
            }

            // Perform section type detection
            foreach (var section in this.sections)
            {
                if (section.VerticalSectionColumn == null)
                {
                    if (section.Columns.Count == 1)
                    {
                        if (section.Columns[0].ColumnFactor == 0)
                        {
                            section.Type = CanvasSectionTemplate.OneColumnFullWidth;
                        }
                        else
                        {
                            section.Type = CanvasSectionTemplate.OneColumn;
                        }
                    }
                    else if (section.Columns.Count == 2)
                    {
                        if (section.Columns[0].ColumnFactor == 6)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumn;
                        }
                        else if (section.Columns[0].ColumnFactor == 4)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumnRight;
                        }
                        else if (section.Columns[0].ColumnFactor == 8)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumnLeft;
                        }
                    }
                    else if (section.Columns.Count == 3)
                    {
                        section.Type = CanvasSectionTemplate.ThreeColumn;
                    }
                }
                else
                {
                    if (section.Columns.Count == 2)
                    {
                        section.Type = CanvasSectionTemplate.OneColumnVerticalSection;
                        (section.Columns[0] as CanvasColumn).ResetColumn(section.Columns[0].Order, section.Columns[0].ColumnFactor != 0 ? section.Columns[0].ColumnFactor : 12);
                    }
                    else if (section.Columns.Count == 3)
                    {
                        if (section.Columns[1].ColumnFactor == 6)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumnVerticalSection;
                        }
                        else if (section.Columns[1].ColumnFactor == 4)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumnRightVerticalSection;
                        }
                        else if (section.Columns[1].ColumnFactor == 8)
                        {
                            section.Type = CanvasSectionTemplate.TwoColumnLeftVerticalSection;
                        }
                    }
                    else if (section.Columns.Count == 4)
                    {
                        section.Type = CanvasSectionTemplate.ThreeColumnVerticalSection;
                    }
                }
            }

            // Reindex the control order. We're starting control order from 1 for each column.
            ReIndex();

            // Load page header controls. Cortex Topic pages do have 5 controls in the header (= controls that cannot be moved)
            if (LayoutType == PageLayoutType.Topic)
            {
                using (var document = parser.ParseDocument(pageHeaderHtml))
                {
                    // select all control div's
                    var clientSideHeaderControls = document.All.Where(m => m.HasAttribute(CanvasControl.ControlDataAttribute));

                    int headerControlOrder = 1;
                    foreach (var clientSideHeaderControl in clientSideHeaderControls)
                    {
                        // Process the extra header controls
                        var controlData = clientSideHeaderControl.GetAttribute(CanvasControl.ControlDataAttribute);

                        var control = new ClientSideWebPart()
                        {
                            Order = headerControlOrder,
                            IsHeaderControl = true,
                        };
                        control.FromHtml(clientSideHeaderControl);

                        headerControls.Add(control);
                        headerControlOrder++;
                    }
                }
            }
            else
            {
                // Load the page header
                this.pageHeader.FromHtml(pageHeaderHtml);
            }
        }

        private void ReIndex()
        {
            foreach (var section in this.sections.OrderBy(s => s.Order))
            {
                foreach (var column in section.Columns.OrderBy(c => c.Order))
                {
                    var indexer = 0;
                    foreach (var control in column.Controls.OrderBy(c => c.Order))
                    {
                        indexer++;
                        control.Order = indexer;
                    }
                }
            }
        }

        private void ApplySectionAndColumn(CanvasControl control, ClientSideCanvasControlPosition position, ClientSideSectionEmphasis emphasis)
        {
            if (position == null)
            {
                var currentSection = this.sections.FirstOrDefault();
                if (currentSection == null)
                {
                    this.AddSection(new CanvasSection(this) { ZoneEmphasis = 0 }, 0);
                    currentSection = this.sections.FirstOrDefault();
                }

                var currentColumn = currentSection.Columns.FirstOrDefault();
                if (currentColumn == null)
                {
                    currentSection.AddColumn(new CanvasColumn(currentSection));
                    currentColumn = currentSection.Columns.FirstOrDefault();
                }

                control.section = currentSection;
                control.column = currentColumn;
            }
            else
            {
                var currentSection = this.sections.Where(p => p.Order == position.ZoneIndex).FirstOrDefault();
                if (currentSection == null)
                {
                    this.AddSection(new CanvasSection(this) { ZoneEmphasis = emphasis != null ? emphasis.ZoneEmphasis : 0 }, position.ZoneIndex);
                    currentSection = this.sections.Where(p => p.Order == position.ZoneIndex).First();
                }

                var currentColumn = currentSection.Columns.Where(p => p.Order == position.SectionIndex).FirstOrDefault();

                // if layout index was set this means that we possibly have a vertical section column
                if (position.LayoutIndex.HasValue)
                {
                    currentColumn = currentSection.Columns.Where(p => p.Order == position.SectionIndex && p.LayoutIndex == position.LayoutIndex.Value).FirstOrDefault();
                }

                if (currentColumn == null)
                {
                    if (position.LayoutIndex.HasValue)
                    {
                        currentSection.AddColumn(new CanvasColumn(currentSection, position.SectionIndex, position.SectionFactor, position.LayoutIndex.Value));
                        currentColumn = currentSection.Columns.Where(p => p.Order == position.SectionIndex && p.LayoutIndex == position.LayoutIndex.Value).First();

                        // ZoneEmphasis on a vertical section column needs to be retained as that "overrides" the zone emphasis set on the section
                        if (currentColumn.IsVerticalSectionColumn)
                        {
                            (currentColumn as CanvasColumn).VerticalSectionEmphasis = emphasis != null ? emphasis.ZoneEmphasis : 0;
                        }
                    }
                    else
                    {
                        currentSection.AddColumn(new CanvasColumn(currentSection, position.SectionIndex, position.SectionFactor));
                        currentColumn = currentSection.Columns.Where(p => p.Order == position.SectionIndex).First();
                    }
                }

                control.section = currentSection;
                control.column = currentColumn;
            }
        }
        #endregion

        #region Create and Save page

        #endregion

        #region Get client side web parts methods
        public IEnumerable<IClientSideComponent> AvailableClientSideComponents(string name)
        {
            return AvailableClientSideComponentsAsync(name).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<IClientSideComponent>> AvailableClientSideComponentsAsync(string name)
        {
            var apiCall = new ApiCall($"_api/web/GetClientSideWebParts", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var jsonSerializerSettings = new JsonSerializerOptions() { IgnoreNullValues = true };
                var clientSideComponents = (IEnumerable<IClientSideComponent>)JsonSerializer.Deserialize<AvailableClientSideComponents>(response.Json, jsonSerializerSettings).value;

                if (!clientSideComponents.Any())
                {
                    throw new ArgumentException("No client side components could be returned for this web...should not happen but it did...");
                }

                if (!String.IsNullOrEmpty(name))
                {
                    return clientSideComponents.Where(p => p.Name == name);
                }

                return clientSideComponents;
            }
            else
            {
                throw new ArgumentException("No client side components could be returned for this web...should not happen but it did...");
            }

        }
        #endregion

        #region Id <-> Enum mappings
        public DefaultWebPart WebPartIdToDefaultWebPart(string id)
        {
            return IdToDefaultWebPart(id);
        }

        internal static DefaultWebPart IdToDefaultWebPart(string id)
        {
            return (id.ToLower()) switch
            {
                "daf0b71c-6de8-4ef7-b511-faae7c388708" => DefaultWebPart.ContentRollup,
                "e377ea37-9047-43b9-8cdb-a761be2f8e09" => DefaultWebPart.BingMap,
                "490d7c76-1824-45b2-9de3-676421c997fa" => DefaultWebPart.ContentEmbed,
                "b7dd04e1-19ce-4b24-9132-b60a1c2b910d" => DefaultWebPart.DocumentEmbed,
                "d1d91016-032f-456d-98a4-721247c305e8" => DefaultWebPart.Image,
                "af8be689-990e-492a-81f7-ba3e4cd3ed9c" => DefaultWebPart.ImageGallery,
                "6410b3b6-d440-4663-8744-378976dc041e" => DefaultWebPart.LinkPreview,
                "0ef418ba-5d19-4ade-9db0-b339873291d0" => DefaultWebPart.NewsFeed,
                "a5df8fdf-b508-4b66-98a6-d83bc2597f63" => DefaultWebPart.NewsReel,
                // Seems like we've been having 2 guids to identify this web part...
                // Now that _api/web/GetClientSideWebParts returns both guids / controls we can distinguish News v NewsReel
                "8c88f208-6c77-4bdb-86a0-0c47b4316588" => DefaultWebPart.News,
                "58fcd18b-e1af-4b0a-b23b-422c2c52d5a2" => DefaultWebPart.PowerBIReportEmbed,
                "91a50c94-865f-4f5c-8b4e-e49659e69772" => DefaultWebPart.QuickChart,
                "eb95c819-ab8f-4689-bd03-0c2d65d47b1f" => DefaultWebPart.SiteActivity,
                "275c0095-a77e-4f6d-a2a0-6a7626911518" => DefaultWebPart.VideoEmbed,
                "31e9537e-f9dc-40a4-8834-0e3b7df418bc" => DefaultWebPart.YammerEmbed,
                "20745d7d-8581-4a6c-bf26-68279bc123fc" => DefaultWebPart.Events,
                "6676088b-e28e-4a90-b9cb-d0d0303cd2eb" => DefaultWebPart.GroupCalendar,
                "c4bd7b2f-7b6e-4599-8485-16504575f590" => DefaultWebPart.Hero,
                "f92bf067-bc19-489e-a556-7fe95f508720" => DefaultWebPart.List,
                "cbe7b0a9-3504-44dd-a3a3-0e5cacd07788" => DefaultWebPart.PageTitle,
                "7f718435-ee4d-431c-bdbf-9c4ff326f46e" => DefaultWebPart.People,
                "c70391ea-0b10-4ee9-b2b4-006d3fcad0cd" => DefaultWebPart.QuickLinks,
                "71c19a43-d08c-4178-8218-4df8554c0b0e" => DefaultWebPart.CustomMessageRegion,
                "2161a1c6-db61-4731-b97c-3cdb303f7cbb" => DefaultWebPart.Divider,
                "b19b3b9e-8d13-4fec-a93c-401a091c0707" => DefaultWebPart.MicrosoftForms,
                "8654b779-4886-46d4-8ffb-b5ed960ee986" => DefaultWebPart.Spacer,
                "243166f5-4dc3-4fe2-9df2-a7971b546a0a" => DefaultWebPart.ClientWebPart,
                "9d7e898c-f1bb-473a-9ace-8b415036578b" => DefaultWebPart.PowerApps,
                "7b317bca-c919-4982-af2f-8399173e5a1e" => DefaultWebPart.CodeSnippet,
                "cf91cf5d-ac23-4a7a-9dbc-cd9ea2a4e859" => DefaultWebPart.PageFields,
                "868ac3c3-cad7-4bd6-9a1c-14dc5cc8e823" => DefaultWebPart.Weather,
                "544dd15b-cf3c-441b-96da-004d5a8cea1d" => DefaultWebPart.YouTube,
                "b519c4f1-5cf7-4586-a678-2f1c62cc175a" => DefaultWebPart.MyDocuments,
                "cb3bfe97-a47f-47ca-bffb-bb9a5ff83d75" => DefaultWebPart.YammerFullFeed,
                "62cac389-787f-495d-beca-e11786162ef4" => DefaultWebPart.CountDown,
                "a8cd4347-f996-48c1-bcfb-75373fed2a27" => DefaultWebPart.ListProperties,
                "1ef5ed11-ce7b-44be-bc5e-4abd55101d16" => DefaultWebPart.MarkDown,
                "39c4c1c2-63fa-41be-8cc2-f6c0b49b253d" => DefaultWebPart.Planner,
                "7cba020c-5ccb-42e8-b6fc-75b3149aba7b" => DefaultWebPart.Sites,
                "df8e44e7-edd5-46d5-90da-aca1539313b8" => DefaultWebPart.CallToAction,
                "0f087d7f-520e-42b7-89c0-496aaf979d58" => DefaultWebPart.Button,
                _ => DefaultWebPart.ThirdParty,
            };
        }

        public string DefaultWebPartToWebPartId(DefaultWebPart webPart)
        {
            return WebPartEnumToId(webPart);
        }

        internal static string WebPartEnumToId(DefaultWebPart webPart)
        {
            return webPart switch
            {
                DefaultWebPart.ContentRollup => "daf0b71c-6de8-4ef7-b511-faae7c388708",
                DefaultWebPart.BingMap => "e377ea37-9047-43b9-8cdb-a761be2f8e09",
                DefaultWebPart.ContentEmbed => "490d7c76-1824-45b2-9de3-676421c997fa",
                DefaultWebPart.DocumentEmbed => "b7dd04e1-19ce-4b24-9132-b60a1c2b910d",
                DefaultWebPart.Image => "d1d91016-032f-456d-98a4-721247c305e8",
                DefaultWebPart.ImageGallery => "af8be689-990e-492a-81f7-ba3e4cd3ed9c",
                DefaultWebPart.LinkPreview => "6410b3b6-d440-4663-8744-378976dc041e",
                DefaultWebPart.NewsFeed => "0ef418ba-5d19-4ade-9db0-b339873291d0",
                DefaultWebPart.NewsReel => "a5df8fdf-b508-4b66-98a6-d83bc2597f63",
                DefaultWebPart.News => "8c88f208-6c77-4bdb-86a0-0c47b4316588",
                DefaultWebPart.PowerBIReportEmbed => "58fcd18b-e1af-4b0a-b23b-422c2c52d5a2",
                DefaultWebPart.QuickChart => "91a50c94-865f-4f5c-8b4e-e49659e69772",
                DefaultWebPart.SiteActivity => "eb95c819-ab8f-4689-bd03-0c2d65d47b1f",
                DefaultWebPart.VideoEmbed => "275c0095-a77e-4f6d-a2a0-6a7626911518",
                DefaultWebPart.YammerEmbed => "31e9537e-f9dc-40a4-8834-0e3b7df418bc",
                DefaultWebPart.Events => "20745d7d-8581-4a6c-bf26-68279bc123fc",
                DefaultWebPart.GroupCalendar => "6676088b-e28e-4a90-b9cb-d0d0303cd2eb",
                DefaultWebPart.Hero => "c4bd7b2f-7b6e-4599-8485-16504575f590",
                DefaultWebPart.List => "f92bf067-bc19-489e-a556-7fe95f508720",
                DefaultWebPart.PageTitle => "cbe7b0a9-3504-44dd-a3a3-0e5cacd07788",
                DefaultWebPart.People => "7f718435-ee4d-431c-bdbf-9c4ff326f46e",
                DefaultWebPart.QuickLinks => "c70391ea-0b10-4ee9-b2b4-006d3fcad0cd",
                DefaultWebPart.CustomMessageRegion => "71c19a43-d08c-4178-8218-4df8554c0b0e",
                DefaultWebPart.Divider => "2161a1c6-db61-4731-b97c-3cdb303f7cbb",
                DefaultWebPart.MicrosoftForms => "b19b3b9e-8d13-4fec-a93c-401a091c0707",
                DefaultWebPart.Spacer => "8654b779-4886-46d4-8ffb-b5ed960ee986",
                DefaultWebPart.ClientWebPart => "243166f5-4dc3-4fe2-9df2-a7971b546a0a",
                DefaultWebPart.PowerApps => "9d7e898c-f1bb-473a-9ace-8b415036578b",
                DefaultWebPart.CodeSnippet => "7b317bca-c919-4982-af2f-8399173e5a1e",
                DefaultWebPart.PageFields => "cf91cf5d-ac23-4a7a-9dbc-cd9ea2a4e859",
                DefaultWebPart.Weather => "868ac3c3-cad7-4bd6-9a1c-14dc5cc8e823",
                DefaultWebPart.YouTube => "544dd15b-cf3c-441b-96da-004d5a8cea1d",
                DefaultWebPart.MyDocuments => "b519c4f1-5cf7-4586-a678-2f1c62cc175a",
                DefaultWebPart.YammerFullFeed => "cb3bfe97-a47f-47ca-bffb-bb9a5ff83d75",
                DefaultWebPart.CountDown => "62cac389-787f-495d-beca-e11786162ef4",
                DefaultWebPart.ListProperties => "a8cd4347-f996-48c1-bcfb-75373fed2a27",
                DefaultWebPart.MarkDown => "1ef5ed11-ce7b-44be-bc5e-4abd55101d16",
                DefaultWebPart.Planner => "39c4c1c2-63fa-41be-8cc2-f6c0b49b253d",
                DefaultWebPart.Sites => "7cba020c-5ccb-42e8-b6fc-75b3149aba7b",
                DefaultWebPart.CallToAction => "df8e44e7-edd5-46d5-90da-aca1539313b8",
                DefaultWebPart.Button => "0f087d7f-520e-42b7-89c0-496aaf979d58",
                _ => "",
            };
        }
        #endregion

        #endregion
    }
}
