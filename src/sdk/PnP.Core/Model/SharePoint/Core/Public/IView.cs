using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a View object
    /// </summary>
    [ConcreteType(typeof(View))]
    public interface IView : IDataModel<IView>, IDataModelUpdate, IDataModelDelete
    {
        #region Properties

        /// <summary>
        /// Gets or sets Aggregations
        /// </summary>
        public string Aggregations { get; set; }

        /// <summary>
        /// Gets or sets Aggregations Status
        /// </summary>
        public string AggregationsStatus { get; set; }

        /// <summary>
        /// Gets or sets Associated Content Type Id
        /// </summary>
        public string AssociatedContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets Base View Id
        /// </summary>
        public string BaseViewId { get; set; }

        /// <summary>
        /// Gets or sets Calendar View Styles
        /// </summary>
        public string CalendarViewStyles { get; set; }

        /// <summary>
        /// Gets or sets Column Width
        /// </summary>
        public string ColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets Custom Formatter
        /// </summary>
        public string CustomFormatter { get; set; }

        /// <summary>
        /// Gets or sets Default View
        /// </summary>
        public bool DefaultView { get; set; }

        /// <summary>
        /// Gets or sets Default View For Content Type
        /// </summary>
        public bool DefaultViewForContentType { get; set; }

        /// <summary>
        /// Gets or sets Editor Modified
        /// </summary>
        public bool EditorModified { get; set; }

        /// <summary>
        /// Gets or sets Formats
        /// </summary>
        public string Formats { get; set; }

        /// <summary>
        /// Gets or sets Grid Layout
        /// </summary>
        public string GridLayout { get; set; }

        /// <summary>
        /// Gets or sets Hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets Html Schema Xml
        /// </summary>
        public string HtmlSchemaXml { get; set; }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Image Url
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets Include Root Folder
        /// </summary>
        public bool IncludeRootFolder { get; set; }

        /// <summary>
        /// Gets or sets View Joins
        /// </summary>
        public string ViewJoins { get; set; }

        /// <summary>
        /// Gets or sets JS Link
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// Gets or sets List View Xml
        /// </summary>
        public string ListViewXml { get; set; }

        /// <summary>
        /// Gets or sets Method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets Mobile Default View
        /// </summary>
        public bool MobileDefaultView { get; set; }

        /// <summary>
        /// Gets or sets Mobile View
        /// </summary>
        public bool MobileView { get; set; }

        /// <summary>
        /// Gets or sets Moderation Type
        /// </summary>
        public string ModerationType { get; set; }

        /// <summary>
        /// Gets or sets New Document Templates
        /// </summary>
        public string NewDocumentTemplates { get; set; }

        /// <summary>
        /// Gets or sets Ordered View
        /// </summary>
        public bool OrderedView { get; set; }

        /// <summary>
        /// Gets or sets Paged
        /// </summary>
        public bool Paged { get; set; }

        /// <summary>
        /// Gets or sets Page Render Type
        /// </summary>
        public int PageRenderType { get; set; }

        /// <summary>
        /// Gets or sets Personal View
        /// </summary>
        public bool PersonalView { get; set; }

        /// <summary>
        /// Gets or sets View Projected Fields
        /// </summary>
        public string ViewProjectedFields { get; set; }

        /// <summary>
        /// Gets or sets View Query
        /// </summary>
        public string ViewQuery { get; set; }

        /// <summary>
        /// Gets or sets Read Only View
        /// </summary>
        public bool ReadOnlyView { get; set; }

        /// <summary>
        /// Gets or sets Requires Client Integration
        /// </summary>
        public bool RequiresClientIntegration { get; set; }

        /// <summary>
        /// Gets or sets Row Limit
        /// </summary>
        public int RowLimit { get; set; }

        /// <summary>
        /// Gets or sets Scope
        /// </summary>
        public int Scope { get; set; }

        /// <summary>
        /// Gets or sets Server Relative Url
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// Gets or sets Style Id
        /// </summary>
        public string StyleId { get; set; }

        /// <summary>
        /// Gets or sets Tabular View
        /// </summary>
        public bool TabularView { get; set; }

        /// <summary>
        /// Gets or sets Threaded
        /// </summary>
        public bool Threaded { get; set; }

        /// <summary>
        /// Gets or sets Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Toolbar
        /// </summary>
        public string Toolbar { get; set; }

        /// <summary>
        /// Gets or sets Toolbar Template Name
        /// </summary>
        public string ToolbarTemplateName { get; set; }

        /// <summary>
        /// Gets or sets View Type
        /// </summary>
        public string ViewType { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string ViewData { get; set; }

        /// <summary>
        /// Gets or sets View Type 2
        /// </summary>
        public ViewType2 ViewType2 { get; set; }

        /// <summary>
        /// Gets or sets View Fields
        /// </summary>
        public IViewFieldCollection ViewFields { get; }

        #endregion

    }
}
