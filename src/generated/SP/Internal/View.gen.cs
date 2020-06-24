using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a View object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class View : BaseDataModel<IView>, IView
    {

        #region New properties

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

        public IViewFieldCollection ViewFields
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ViewFieldCollection
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IViewFieldCollection>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
