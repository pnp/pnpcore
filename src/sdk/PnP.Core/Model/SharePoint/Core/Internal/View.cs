using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// View class, write your custom code here
    /// </summary>
    [SharePointType("SP.View", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/Views({Id})", 
            Get = "_api/web/lists(guid'{Parent.Id}')/Views", LinqGet = "_api/web/lists(guid'{Parent.Id}')/Views", 
            Delete = "_api/web/lists(guid'{Parent.Id}')/Views(guid'{Id}')/DeleteObject")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class View : BaseDataModel<IView>, IView
    {
        internal const string ViewOptionsAdditionalInformationKey = "ViewOptions";

        #region Construction
        public View()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var viewOptions = (ViewOptions)additionalInformation[ViewOptionsAdditionalInformationKey];
                var entity = EntityManager.GetClassInfo(GetType(), this);
                             
                // Build body
                var viewCreationInformation = new
                {
                    parameters = new
                    {
                        __metadata = new { type = "SP.ViewCreationInformation" },
                        viewOptions.AssociatedContentTypeId,
                        viewOptions.BaseViewId,
                        viewOptions.CalendarViewStyles,
                        viewOptions.Paged,
                        viewOptions.PersonalView,
                        viewOptions.Query,
                        viewOptions.RowLimit,
                        viewOptions.SetAsDefaultView,
                        viewOptions.Title,
                        viewOptions.ViewData,
                        viewOptions.ViewFields,
                        viewOptions.ViewTypeKind,
                        viewOptions.ViewType2
                    }
                }.AsExpando();

                // To handle the serialization of string collections
                var serializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true };
                serializerOptions.Converters.Add(new SharePointRestCollectionJsonConverter<string>());

                string body = JsonSerializer.Serialize(viewCreationInformation, typeof(ExpandoObject), serializerOptions);

                return new ApiCall($"{entity.SharePointGet}/Add", ApiType.SPORest, body);
            };
        }
        #endregion

        #region Properties
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

        public IViewFieldCollection ViewFields { get => GetModelCollectionValue<IViewFieldCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
        #endregion
    }
}
