using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A modern SharePoint Page
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Title of the client side page
        /// </summary>
        string PageTitle { get; set; }

        /// <summary>
        /// Layout type of the client side page
        /// </summary>
        public PageLayoutType LayoutType { get; set; }

        /// <summary>
        /// Thumbnail url for the page
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// When a page of type Home is created you can opt to only keep the default client side web parts by setting this to true. This also is a way to reset your home page back the the stock one.
        /// </summary>
        public bool KeepDefaultWebParts { get; set; }

        /// <summary>
        /// List of sections on this page
        /// </summary>
        List<ICanvasSection> Sections { get; }

        /// <summary>
        /// List of control on this page
        /// </summary>
        List<ICanvasControl> Controls { get; }

        /// <summary>
        /// List of controls on this page's header
        /// </summary>
        List<ICanvasControl> HeaderControls { get; }

        /// <summary>
        /// Returns the page header for this page
        /// </summary>
        IPageHeader PageHeader { get; }

        /// <summary>
        /// Space content field (JSON) for spaces pages
        /// </summary>
        string SpaceContent { get; set; }

        /// <summary>
        /// Entity id field for topic pages
        /// </summary>
        string EntityId { get; set; }

        /// <summary>
        /// Entity relations field for topic pages
        /// </summary>
        string EntityRelations { get; set; }

        /// <summary>
        /// Entity type field for topic pages
        /// </summary>
        string EntityType { get; set; }

        /// <summary>
        /// Url being reposted using the repost page
        /// </summary>
        public string RepostSourceUrl { get; set; }

        /// <summary>
        /// Description of the repost page
        /// </summary>
        public string RepostDescription { get; set; }

        /// <summary>
        /// Id of the SharePoint <see cref="ISite"/> containting the page being reposted
        /// </summary>
        public Guid RepostSourceSiteId { get; set; }

        /// <summary>
        /// Id of the SharePoint <see cref="IWeb"/> containting the page being reposted
        /// </summary>
        public Guid RepostSourceWebId { get; set; }

        /// <summary>
        /// Id of the SharePoint <see cref="IList"/> containting the page being reposted
        /// </summary>
        public Guid RepostSourceListId { get; set; }

        /// <summary>
        /// Id of the SharePoint <see cref="IListItem"/> connected to the page being reposted
        /// </summary>
        public Guid RepostSourceItemId { get; set; }

        /// <summary>
        /// PnPContext to work with
        /// </summary>
        PnPContext PnPContext { get; }

        /// <summary>
        /// Pages library
        /// </summary>
        IList PagesLibrary { get; }

        /// <summary>
        /// ListItem linked to this page
        /// </summary>
        public IListItem PageListItem { get; }

        /// <summary>
        /// ID value of the page (only available when the page was saved)
        /// </summary>
        public int? PageId { get; }

        /// <summary>
        /// The default section of the client side page
        /// </summary>
        public ICanvasSection DefaultSection { get; }

        /// <summary>
        /// Folder the page lives in (available after saving the page)
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// The name of this page (available after saving the page)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns the scheduled publish data of a page (only if publish was scheduled)
        /// </summary>
        public DateTime? ScheduledPublishDate { get; }

        /// <summary>
        /// Returns the editor that was used to create/update this page
        /// </summary>
        public EditorType EditorType { get; }

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="sectionTemplate">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        /// <param name="zoneEmphasis">Zone emphasis (section background)</param>
        /// <param name="verticalSectionZoneEmphasis">Vertical Section Zone emphasis (section background)</param>
        public void AddSection(CanvasSectionTemplate sectionTemplate, float order, VariantThemeType zoneEmphasis, VariantThemeType verticalSectionZoneEmphasis = VariantThemeType.None);

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="sectionTemplate">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        /// <param name="zoneEmphasis">Zone emphasis (section background)</param>
        /// <param name="verticalSectionZoneEmphasis">Vertical Section Zone emphasis (section background)</param>
        public void AddSection(CanvasSectionTemplate sectionTemplate, float order, int zoneEmphasis, int? verticalSectionZoneEmphasis = null);

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="sectionTemplate">The <see cref="CanvasSectionTemplate"/> type of the section</param>
        /// <param name="order">Controls the order of the new section</param>
        public void AddSection(CanvasSectionTemplate sectionTemplate, float order);

        /// <summary>
        /// Adds a new section to your client side page
        /// </summary>
        /// <param name="section"><see cref="ICanvasSection"/> object describing the section to add</param>
        public void AddSection(ICanvasSection section);

        /// <summary>
        /// Adds a new section to your client side page with a given order
        /// </summary>
        /// <param name="section"><see cref="ICanvasSection"/> object describing the section to add</param>
        /// <param name="order">Controls the order of the new section</param>
        public void AddSection(ICanvasSection section, float order);

        /// <summary>
        /// Adds a new control to your client side page using the default <see cref="ICanvasSection"/>
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        public void AddControl(ICanvasControl control);

        /// <summary>
        /// Adds a new control to your client side page using the default <see cref="ICanvasSection"/> using a given order
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="order">Order of the control in the default section</param>
        public void AddControl(ICanvasControl control, int order);

        /// <summary>
        /// Adds a new control to your client side page in the given section
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="section"><see cref="ICanvasSection"/> that will hold the control. Control will end up in the <see cref="ICanvasSection.DefaultColumn"/>.</param>
        public void AddControl(ICanvasControl control, ICanvasSection section);

        /// <summary>
        /// Adds a new control to your client side page in the given section with a given order
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="section"><see cref="ICanvasSection"/> that will hold the control. Control will end up in the <see cref="ICanvasSection.DefaultColumn"/>.</param>
        /// <param name="order">Order of the control in the given section</param>
        public void AddControl(ICanvasControl control, ICanvasSection section, int order);

        /// <summary>
        /// Adds a new control to your client side page in the given section
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="column"><see cref="ICanvasColumn"/> that will hold the control</param>    
        public void AddControl(ICanvasControl control, ICanvasColumn column);

        /// <summary>
        /// Adds a new control to your client side page in the given section with a given order
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="column"><see cref="ICanvasColumn"/> that will hold the control</param>    
        /// <param name="order">Order of the control in the given section</param>
        public void AddControl(ICanvasControl control, ICanvasColumn column, int order);

        /// <summary>
        /// Removes the set page header by setting the page banner web part to "Plain" mode
        /// </summary>
        public void RemovePageHeader();

        /// <summary>
        /// Sets page header back to the default page header
        /// </summary>
        public void SetDefaultPageHeader();

        /// <summary>
        /// Sets page header with custom focal point
        /// </summary>
        /// <param name="serverRelativeImageUrl">Server relative page header image url</param>
        /// <param name="translateX">X focal point for image</param>
        /// <param name="translateY">Y focal point for image</param>
        public void SetCustomPageHeader(string serverRelativeImageUrl, double? translateX = null, double? translateY = null);

        /// <summary>
        /// Sets page header back to the default for PageTilte WebPart (Message ID: MC791596 / Roadmap ID: 386904). The PageTitle WebPart has to be added into a first OneColumnFullWith Section separate.
        /// </summary>
        public void SetPageTitleWebPartPageHeader();

        /// <summary>
        /// Adds a new header control to your client side page with a given order. Used for topic page creation
        /// </summary>
        /// <param name="control"><see cref="ICanvasControl"/> to add</param>
        /// <param name="order">Order of the control in the given section</param>
        public void AddHeaderControl(ICanvasControl control, int order);

        /// <summary>
        /// Creates a new text part which can be configured and added to the page
        /// </summary>
        /// <param name="text">Optionally provide the text for the text part</param>
        /// <returns>The created text part</returns>
        public IPageText NewTextPart(string text = null);

        /// <summary>
        /// Creates a new web part
        /// </summary>
        /// <param name="clientSideComponent">The base component to use for this web part</param>
        /// <returns>The created web part</returns>
        public IPageWebPart NewWebPart(IPageComponent clientSideComponent = null);

        /// <summary>
        /// Creates a new web part
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public IPageWebPart NewBackGroundImageControl();

        /// <summary>
        /// Clears the sections and controls from the page
        /// </summary>
        public void ClearPage();

        /// <summary>
        /// Returns the name of the templates folder, and creates if it doesn't exist.
        /// </summary>        
        public Task<string> GetTemplatesFolderAsync();

        /// <summary>
        /// Returns the name of the templates folder, and creates if it doesn't exist.
        /// </summary>        
        public string GetTemplatesFolder();

        /// <summary>
        /// Saves the page to the pages library
        /// </summary>
        /// <param name="pageName">Page name to use</param>        
        /// <returns>The name of the saved page</returns>
        Task<string> SaveAsync(string pageName = null);

        /// <summary>
        /// Saves the page to the pages library
        /// </summary>
        /// <param name="pageName">Page name to use</param>
        /// <returns>The name of the saved page</returns>
        string Save(string pageName = null);

        /// <summary>
        /// Saves the page as a template page
        /// </summary>
        /// <param name="pageName">Page name to use</param>
        /// <returns>The name of the saved page</returns>
        public string SaveAsTemplate(string pageName = null);

        /// <summary>
        /// Saves the page as a template page
        /// </summary>
        /// <param name="pageName">Page name to use</param>
        /// <returns>The name of the saved page</returns>
        public Task<string> SaveAsTemplateAsync(string pageName = null);

        /// <summary>
        /// Deletes the created page
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes the created page
        /// </summary>
        /// <returns></returns>
        void Delete();

        /// <summary>
        /// Gets the file connected to this page
        /// </summary>
        /// <param name="expressions">Properties to load for the file</param>
        /// <returns>The connected <see cref="IFile"/></returns>
        public Task<IFile> GetPageFileAsync(params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Gets the file connected to this page
        /// </summary>
        /// <param name="expressions">Properties to load for the file</param>
        /// <returns>The connected <see cref="IFile"/></returns>
        public IFile GetPageFile(params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Publishes a client side page
        /// </summary>
        /// <param name="comment">Publishing comment</param>
        public void Publish(string comment = null);

        /// <summary>
        /// Publishes a client side page
        /// </summary>
        /// <param name="comment">Publishing comment</param>
        public Task PublishAsync(string comment = null);

        /// <summary>
        /// Schedules the publication of a client side page
        /// </summary>
        /// <param name="publishDate">Date when the page needs to be publishing</param>
        public Task SchedulePublishAsync(DateTime publishDate);

        /// <summary>
        /// Schedules the publication of a client side page
        /// </summary>
        /// <param name="publishDate">Date when the page needs to be publishing</param>
        public void SchedulePublish(DateTime publishDate);

        /// <summary>
        /// Removes the publication schedule of a client side page
        /// </summary>
        public Task RemoveSchedulePublishAsync();

        /// <summary>
        /// Removes the publication schedule of a client side page
        /// </summary>
        public void RemoveSchedulePublish();

        /// <summary>
        /// Demotes an client side <see cref="PageLayoutType.Article"/> news page as a regular client side page
        /// </summary>
        public void DemoteNewsArticle();

        /// <summary>
        /// Demotes an client side <see cref="PageLayoutType.Article"/> news page as a regular client side page
        /// </summary>
        public Task DemoteNewsArticleAsync();

        /// <summary>
        /// Promotes a regular <see cref="PageLayoutType.Article"/> client side page as a news page
        /// </summary>
        public void PromoteAsNewsArticle();

        /// <summary>
        /// Promotes a regular <see cref="PageLayoutType.Article"/> client side page as a news page
        /// </summary>
        public Task PromoteAsNewsArticleAsync();

        /// <summary>
        /// Sets the current <see cref="IPage"/> as home page for the current site
        /// </summary>
        public void PromoteAsHomePage();

        /// <summary>
        /// Sets the current <see cref="IPage"/> as home page for the current site
        /// </summary>
        public Task PromoteAsHomePageAsync();

        /// <summary>
        /// Does this page have comments disabled
        /// </summary>
        public bool AreCommentsDisabled();

        /// <summary>
        /// Does this page have comments disabled
        /// </summary>
        public Task<bool> AreCommentsDisabledAsync();

        /// <summary>
        /// Enable commenting on this page
        /// </summary>
        public void EnableComments();

        /// <summary>
        /// Enable commenting on this page
        /// </summary>
        public Task EnableCommentsAsync();

        /// <summary>
        /// Disable commenting on this page
        /// </summary>
        public void DisableComments();

        /// <summary>
        /// Disable commenting on this page
        /// </summary>
        public Task DisableCommentsAsync();

        /// <summary>
        /// Translated a given web part id to a <see cref="DefaultWebPart"/> enum. Non default web parts will be returned as <see cref="DefaultWebPart.ThirdParty"/>
        /// </summary>
        /// <param name="id">Web part id to lookup</param>
        /// <returns>Corresponding <see cref="DefaultWebPart"/> enum value</returns>
        DefaultWebPart WebPartIdToDefaultWebPart(string id);

        /// <summary>
        /// Translates a given <see cref="DefaultWebPart"/> enum to it's corresponding web part id. Non default web parts will be returned as empty string
        /// </summary>
        /// <param name="webPart"><see cref="DefaultWebPart"/> enum to translate to it's id</param>
        /// <returns>The corresponding web part id</returns>
        string DefaultWebPartToWebPartId(DefaultWebPart webPart);

        /// <summary>
        /// Gets a list of available client side web parts to use, optionally filtered by a given name
        /// </summary>
        /// <param name="name">Name of the web part to retrieve</param>
        /// <returns>List of available <see cref="IPageComponent"/></returns>
        Task<IEnumerable<IPageComponent>> AvailablePageComponentsAsync(string name = null);

        /// <summary>
        /// Gets a list of available client side web parts to use, optionally filtered by a given name
        /// </summary>
        /// <param name="name">Name of the web part to retrieve</param>
        /// <returns>List of available <see cref="IPageComponent"/></returns>
        IEnumerable<IPageComponent> AvailablePageComponents(string name = null);

        /// <summary>
        /// Instantiates a new web part with default data and returns it
        /// </summary>
        /// <param name="webPart">Default web part to instantiate</param>
        /// <returns>Instantiated web part</returns>
        public Task<IPageWebPart> InstantiateDefaultWebPartAsync(DefaultWebPart webPart);

        /// <summary>
        /// Instantiates a new web part with default data and returns it
        /// </summary>
        /// <param name="webPart">Default web part to instantiate</param>
        /// <returns>Instantiated web part</returns>
        public IPageWebPart InstantiateDefaultWebPart(DefaultWebPart webPart);

        /// <summary>
        /// Returns the page translation status for a given page
        /// </summary>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        Task<IPageTranslationStatusCollection> GetPageTranslationsAsync();

        /// <summary>
        /// Returns the page translation status for a given page
        /// </summary>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        IPageTranslationStatusCollection GetPageTranslations();

        /// <summary>
        /// Creates page translations for the requested languages
        /// </summary>
        /// <param name="pageTranslationOptions">Options describing the languages in which a translation needs to be created</param>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        Task<IPageTranslationStatusCollection> TranslatePagesAsync(PageTranslationOptions pageTranslationOptions);

        /// <summary>
        /// Creates page translations for all the languages defined as multi-lingual languages in the site
        /// </summary>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        Task<IPageTranslationStatusCollection> TranslatePagesAsync();

        /// <summary>
        /// Creates page translations for the requested languages
        /// </summary>
        /// <param name="pageTranslationOptions">Options describing the languages in which a translation needs to be created</param>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        IPageTranslationStatusCollection TranslatePages(PageTranslationOptions pageTranslationOptions);

        /// <summary>
        /// Creates page translations for all the languages defined as multi-lingual languages in the site
        /// </summary>
        /// <returns><see cref="IPageTranslationStatusCollection"/> list containing information about this page's translations</returns>
        IPageTranslationStatusCollection TranslatePages();

        /// <summary>
        /// Get list item comments
        /// </summary>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        public Task<ICommentCollection> GetCommentsAsync(params Expression<Func<IComment, object>>[] selectors);

        /// <summary>
        /// Get list item comments
        /// </summary>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        public ICommentCollection GetComments(params Expression<Func<IComment, object>>[] selectors);

        /// <summary>
        /// Likes a page
        /// </summary>
        /// <returns></returns>
        public Task LikeAsync();

        /// <summary>
        /// Likes a page
        /// </summary>
        /// <returns></returns>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public void Like();
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Unlikes a page
        /// </summary>
        /// <returns></returns>
        public Task UnlikeAsync();

        /// <summary>
        /// Unlikes a page
        /// </summary>
        /// <returns></returns>
        public void Unlike();

        /// <summary>
        /// Returns information about the likes on this page
        /// </summary>
        /// <returns></returns>
        public Task<ILikedByInformation> GetLikedByInformationAsync();

        /// <summary>
        /// Returns information about the likes on this page
        /// </summary>
        /// <returns></returns>
        public ILikedByInformation GetLikedByInformation();

        /// <summary>
        /// Prepares an Image web part for adding to a page
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative url of the image to show in the image web part</param>
        /// <param name="imageOptions">Options to configure the image when being added</param>
        /// <returns>Configured image web part which can be added to the page</returns>
        public Task<IPageWebPart> GetImageWebPartAsync(string serverRelativeUrl, PageImageOptions imageOptions = null);

        /// <summary>
        /// Prepares an Image web part for adding to a page
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative url of the image to show in the image web part</param>
        /// <param name="imageOptions">Options to configure the image when being added</param>
        /// <returns>Configured image web part which can be added to the page</returns>
        public IPageWebPart GetImageWebPart(string serverRelativeUrl, PageImageOptions imageOptions = null);

        /// <summary>
        /// Prepares an inline image for adding to text by adding the needed image web part and returning the HTML snippet that needs to be appended to the text editor content
        /// </summary>
        /// <param name="textEditorInstance">Text editor instance to add the inline image to, when the <see cref="Page.EditorType"/> is <see cref="EditorType.CK5"/> then this can be left null</param>
        /// <param name="serverRelativeUrl">Server relative url of the image to add inline</param>
        /// <param name="imageOptions">Options to configure the inline image when being added</param>
        /// <returns>Html snippet to be appended to the text editor content</returns>
        public Task<string> GetInlineImageAsync(IPageText textEditorInstance, string serverRelativeUrl, PageImageOptions imageOptions = null);

        /// <summary>
        /// Prepares an inline image for adding to text by adding the needed image web part and returning the HTML snippet that needs to be appended to the text editor content
        /// </summary>
        /// <param name="textEditorInstance">Text editor instance to add the inline image to</param>
        /// <param name="serverRelativeUrl">Server relative url of the image to add inline</param>
        /// <param name="imageOptions">Options to configure the inline image when being added</param>
        /// <returns>Html snippet to be appended to the text editor content</returns>
        public string GetInlineImage(IPageText textEditorInstance, string serverRelativeUrl, PageImageOptions imageOptions = null);
    }

}
