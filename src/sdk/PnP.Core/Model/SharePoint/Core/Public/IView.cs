using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a View object
    /// </summary>
    [ConcreteType(typeof(View))]
    public interface IView : IDataModel<IView>, IDataModelGet<IView>, IDataModelLoad<IView>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        #region Properties

        /// <summary>
        ///  Specifies the aggregations displayed by the list view. The Aggregations property contains a CAML string.
        /// </summary>
        public string Aggregations { get; set; }

        /// <summary>
        /// Specifies whether the aggregations are shown in the list view
        /// </summary>
        public string AggregationsStatus { get; set; }

        /// <summary>
        /// Gets or sets Associated Content Type Id
        /// </summary>
        public string AssociatedContentTypeId { get; set; }

        /// <summary>
        /// Specifies the base view identifier of the list view
        /// </summary>
        public string BaseViewId { get; }

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
        /// Gets or sets a Boolean value that specifies whether the view is the default view
        /// </summary>
        public bool DefaultView { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the view is the default view
        /// for the associated content type
        /// </summary>
        public bool DefaultViewForContentType { get; set; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the view was modified in an HTML editor
        /// </summary>
        public bool EditorModified { get; set; }

        /// <summary>
        /// Specifies the definitions for column and row formatting that are used in a datasheet view
        /// </summary>
        public string Formats { get; set; }

        /// <summary>
        /// An attribute of the view, specifies the quick edit layout
        /// </summary>
        public string GridLayout { get; set; }

        /// <summary>
        /// Gets or sets Hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets the Html Schema Xml
        /// </summary>
        public string HtmlSchemaXml { get; }

        /// <summary>
        /// Gets the Id
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Specifies the server relative or absolute URL of the Image for the List View
        /// </summary>
        public string ImageUrl { get; }

        /// <summary>
        /// Specifies if the Root Folder is included in the List View
        /// </summary>
        public bool IncludeRootFolder { get; set; }

        /// <summary>
        /// Specifies the list joins that will be used by the list view
        /// </summary>
        public string ViewJoins { get; set; }

        /// <summary>
        /// An attribute of the view, specifies the Javascript files used for the view.
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// Gets or sets List View Xml
        /// </summary>
        public string ListViewXml { get; set; }

        /// <summary>
        /// Specifies the view method for the list view
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Specifies whether the list view is the default for a mobile device
        /// </summary>
        public bool MobileDefaultView { get; set; }

        /// <summary>
        /// Specifies whether the list view applies to a mobile device
        /// </summary>
        public bool MobileView { get; set; }

        /// <summary>
        /// Specifies the content approval type for the list view. A string that indicates the Content Approval type, which can be one of the following values:
        ///  HideUnapproved -- Unapproved draft items are hidden from users who only have permission to read items.
        ///  Contributor -- Pending and rejected items for the current user are displayed.
        ///  Moderator -- Pending and rejected items for all users are displayed to users who have managed list permissions.
        /// </summary>
        public string ModerationType { get; }

        /// <summary>
        /// An attribute of the view, indicates what documents/templates are visible in "New" menu of a document library
        /// </summary>
        public string NewDocumentTemplates { get; set; }

        /// <summary>
        /// Specifies whether users can reorder items through the user interface
        /// </summary>
        public bool OrderedView { get; }

        /// <summary>
        /// Specifies whether the list view supports displaying items across multiple pages
        /// </summary>
        public bool Paged { get; set; }

        /// <summary>
        /// Gets the reason why the page is rendered in classic UX, or Modern if the page is in Modern UX
        /// </summary>
        public ListPageRenderType PageRenderType { get; }

        /// <summary>
        /// Specifies whether the view is a personal view or a public view
        /// </summary>
        public bool PersonalView { get; }

        /// <summary>
        /// Specifies the projected fields that will be used by the list view
        /// </summary>
        public string ViewProjectedFields { get; set; }

        /// <summary>
        /// Specifies the CAML query that will be used by the list view
        /// </summary>
        public string ViewQuery { get; set; }

        /// <summary>
        /// Specifies if the view is read-only
        /// </summary>
        public bool ReadOnlyView { get; }

        /// <summary>
        /// Specifies whether this view requires client integration
        /// </summary>
        public bool RequiresClientIntegration { get; }

        /// <summary>
        /// Specifies the limit for the number of items that the list view will return per page
        /// </summary>
        public int RowLimit { get; set; }

        /// <summary>
        /// Specifies the recursive scope for the list view of a document library
        /// </summary>
        public ViewScope Scope { get; set; }

        /// <summary>
        /// Specifies the server relative URL of the list view page
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Specifies the identifier of the view style for the list view
        /// </summary>
        public string StyleId { get; }

        /// <summary>
        /// Gets or sets the TabularView attribute in the View Schema XML
        /// </summary>
        public bool TabularView { get; set; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the view is threaded
        /// </summary>
        public bool Threaded { get; }

        /// <summary>
        /// Specifies the Display Name of the List View
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Specifies the toolbar (CAML) for the list view
        /// </summary>
        public string Toolbar { get; set; }

        /// <summary>
        /// Specifies the name of the toolbar template that is used for the list view toolbar
        /// </summary>
        public string ToolbarTemplateName { get; }

        /// <summary>
        /// Specifies the type of the view
        /// </summary>
        public ViewType ViewType { get; }

        /// <summary>
        /// Specifies the view data for the list view 
        /// </summary>
        public string ViewData { get; set; }

        /// <summary>
        /// Gets or sets the ViewType2
        /// </summary>
        public ViewType2 ViewType2 { get; set; }

        /// <summary>
        /// Gets or sets View Fields
        /// </summary>
        public IViewFieldCollection ViewFields { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Moves a view field to a new position in the view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the view field to move</param>
        /// <param name="newOrder">New position</param>
        /// <returns></returns>
        Task MoveViewFieldToAsync(string internalFieldName, int newOrder);

        /// <summary>
        /// Moves a view field to a new position in the view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the view field to move</param>
        /// <param name="newOrder">New position</param>
        /// <returns></returns>
        void MoveViewFieldTo(string internalFieldName, int newOrder);

        /// <summary>
        /// Adds a field to the current view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the field to add</param>
        /// <returns></returns>
        Task AddViewFieldAsync(string internalFieldName);

        /// <summary>
        /// Adds a field to the current view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the field to add</param>
        /// <returns></returns>
        void AddViewField(string internalFieldName);

        /// <summary>
        /// Removes a field from the current view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the field to remove</param>
        /// <returns></returns>
        Task RemoveViewFieldAsync(string internalFieldName);

        /// <summary>
        /// Removes a field from the current view
        /// </summary>
        /// <param name="internalFieldName">Internal name of the field to remove</param>
        /// <returns></returns>
        void RemoveViewField(string internalFieldName);

        #endregion

    }
}
