using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a View object
    /// </summary>
    [ConcreteType(typeof(View))]
    public interface IView : IDataModel<IView>, IDataModelGet<IView>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Aggregations { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AggregationsStatus { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AssociatedContentTypeId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string BaseViewId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CalendarViewStyles { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ColumnWidth { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomFormatter { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DefaultView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DefaultViewForContentType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EditorModified { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Formats { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GridLayout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string HtmlSchemaXml { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IncludeRootFolder { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewJoins { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListViewXml { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool MobileDefaultView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool MobileView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ModerationType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string NewDocumentTemplates { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool OrderedView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Paged { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PageRenderType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PersonalView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewProjectedFields { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewQuery { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ReadOnlyView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool RequiresClientIntegration { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RowLimit { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Scope { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StyleId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool TabularView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Threaded { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Toolbar { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ToolbarTemplateName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewData { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ViewType2 { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IViewFieldCollection ViewFields { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomOrder { get; set; }

        #endregion

    }
}
