using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// View class, write your custom code here
    /// </summary>
    [SharePointType("SP.View", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/Views(guid'{Id}')",
            Get = "_api/web/lists(guid'{Parent.Id}')/Views", LinqGet = "_api/web/lists(guid'{Parent.Id}')/Views",
            Delete = "_api/web/lists(guid'{Parent.Id}')/Views(guid'{Id}')/DeleteObject")]
    internal sealed class View : BaseDataModel<IView>, IView
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

                // Works around an issue with the serialiser of string and integer values
                var ViewTypeKind = (int)viewOptions.ViewTypeKind;

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
                        ViewTypeKind,
                        viewOptions.ViewType2
                    }
                }.AsExpando();


                string body = JsonSerializer.Serialize(viewCreationInformation, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_SharePointRestCollectionJsonConverter_JsonStringEnumConverter);

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

        public ListPageRenderType PageRenderType { get => GetValue<ListPageRenderType>(); set => SetValue(value); }

        public bool PersonalView { get => GetValue<bool>(); set => SetValue(value); }

        public string ViewProjectedFields { get => GetValue<string>(); set => SetValue(value); }

        public string ViewQuery { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnlyView { get => GetValue<bool>(); set => SetValue(value); }

        public bool RequiresClientIntegration { get => GetValue<bool>(); set => SetValue(value); }

        public int RowLimit { get => GetValue<int>(); set => SetValue(value); }

        public ViewScope Scope { get => GetValue<ViewScope>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string StyleId { get => GetValue<string>(); set => SetValue(value); }

        public bool TabularView { get => GetValue<bool>(); set => SetValue(value); }

        public bool Threaded { get => GetValue<bool>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Toolbar { get => GetValue<string>(); set => SetValue(value); }

        public string ToolbarTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public ViewType ViewType { get => GetValue<ViewType>(); set => SetValue(value); }

        public string ViewData { get => GetValue<string>(); set => SetValue(value); }

        public ViewType2 ViewType2 { get => GetValue<ViewType2>(); set => SetValue(value); }

        public IViewFieldCollection ViewFields { get => GetModelCollectionValue<IViewFieldCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Extension Methods

        public async Task MoveViewFieldToAsync(string internalFieldName, int newOrder)
        {
            var apiCall = GetMoveViewFieldToApiCall(internalFieldName, newOrder);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall GetMoveViewFieldToApiCall(string internalFieldName, int newOrder)
        {
            var body = new
            {
                field = internalFieldName,
                index = newOrder
            };

            var bodyString = JsonSerializer.Serialize(body);

            return new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/Views(guid'{Id}')/viewfields/moveviewfieldto", ApiType.SPORest, bodyString);
        }

        public void MoveViewFieldTo(string internalFieldName, int newOrder)
        {
            MoveViewFieldToAsync(internalFieldName, newOrder).GetAwaiter().GetResult();
        }

        public async Task AddViewFieldAsync(string internalFieldName)
        {
            var apiCall = GetAddViewFieldApiCall(internalFieldName);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall GetAddViewFieldApiCall(string internalFieldName)
        {
            var body = new
            {
                strField = internalFieldName,
            };

            var bodyString = JsonSerializer.Serialize(body);

            return new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/Views(guid'{Id}')/viewfields/addviewfield", ApiType.SPORest, bodyString);
        }

        public void AddViewField(string internalFieldName)
        {
            AddViewFieldAsync(internalFieldName).GetAwaiter().GetResult();
        }

        public async Task RemoveViewFieldAsync(string internalFieldName)
        {
            var apiCall = GetRemoveViewFieldApiCall(internalFieldName);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall GetRemoveViewFieldApiCall(string internalFieldName)
        {
            var body = new
            {
                strField = internalFieldName,
            };

            var bodyString = JsonSerializer.Serialize(body);

            return new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/Views(guid'{Id}')/viewfields/removeviewfield", ApiType.SPORest, bodyString);
        }

        public void RemoveViewField(string internalFieldName)
        {
            RemoveViewFieldAsync(internalFieldName).GetAwaiter().GetResult();
        }
        #endregion
    }
}
