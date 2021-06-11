using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// View class, write your custom code here
    /// </summary>
    [SharePointType("SP.View", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class View : BaseDataModel<IView>, IView
    {
        #region Construction
        public View()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string Aggregations { get => GetValue<string>(); set => SetValue(value); }

        public string AggregationsStatus { get => GetValue<string>(); set => SetValue(value); }

        public string AssociatedContentTypeId { get => GetValue<string>(); set => SetValue(value); }

        public string BaseViewId { get => GetValue<string>(); set => SetValue(value); }

        public string CalendarViewStyles { get => GetValue<string>(); set => SetValue(value); }

        public string ColumnWidth { get => GetValue<string>(); set => SetValue(value); }

        public string CustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public bool DefaultView { get => GetValue<bool>(); set => SetValue(value); }

        public bool DefaultViewForContentType { get => GetValue<bool>(); set => SetValue(value); }

        public bool EditorModified { get => GetValue<bool>(); set => SetValue(value); }

        public string Formats { get => GetValue<string>(); set => SetValue(value); }

        public string GridLayout { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string HtmlSchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IncludeRootFolder { get => GetValue<bool>(); set => SetValue(value); }

        public string ViewJoins { get => GetValue<string>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public string ListViewXml { get => GetValue<string>(); set => SetValue(value); }

        public string Method { get => GetValue<string>(); set => SetValue(value); }

        public bool MobileDefaultView { get => GetValue<bool>(); set => SetValue(value); }

        public bool MobileView { get => GetValue<bool>(); set => SetValue(value); }

        public string ModerationType { get => GetValue<string>(); set => SetValue(value); }

        public string NewDocumentTemplates { get => GetValue<string>(); set => SetValue(value); }

        public bool OrderedView { get => GetValue<bool>(); set => SetValue(value); }

        public bool Paged { get => GetValue<bool>(); set => SetValue(value); }

        public int PageRenderType { get => GetValue<int>(); set => SetValue(value); }

        public bool PersonalView { get => GetValue<bool>(); set => SetValue(value); }

        public string ViewProjectedFields { get => GetValue<string>(); set => SetValue(value); }

        public string ViewQuery { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnlyView { get => GetValue<bool>(); set => SetValue(value); }

        public bool RequiresClientIntegration { get => GetValue<bool>(); set => SetValue(value); }

        public int RowLimit { get => GetValue<int>(); set => SetValue(value); }

        public int Scope { get => GetValue<int>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string StyleId { get => GetValue<string>(); set => SetValue(value); }

        public bool TabularView { get => GetValue<bool>(); set => SetValue(value); }

        public bool Threaded { get => GetValue<bool>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Toolbar { get => GetValue<string>(); set => SetValue(value); }

        public string ToolbarTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string ViewType { get => GetValue<string>(); set => SetValue(value); }

        public string ViewData { get => GetValue<string>(); set => SetValue(value); }

        public string ViewType2 { get => GetValue<string>(); set => SetValue(value); }

        public IViewFieldCollection ViewFields { get => GetModelValue<IViewFieldCollection>(); }


        #endregion

        #region New properties

        public string CustomOrder { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
