using PnP.Core.Services;
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
    internal partial class View
    {

        internal const string ViewOptionsAdditionalInformationKey = "ViewOptions";

        public View()
        {
            AddApiCallHandler = async (additionalInformation) =>
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

        
    }
}
