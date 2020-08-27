using PnP.Core.Services;
using System.Dynamic;
using System.Text.Json;
using PnP.Core.Utilities;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLink class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLink", Target = typeof(ContentType), Uri = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks')", Get = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks", LinqGet = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks")]
    // TODO A special target should be achieve to support List Content Types
    //[SharePointType("SP.FieldLink", Target = typeof(ContentType), Uri = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks')", Get = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks", LinqGet = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks")]
    internal partial class FieldLink
    {
        public FieldLink()
        {
            // Handler to construct the Add request for this content type
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (keyValuePairs) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

                // The FieldLinkCollection Add REST endpoint has many limitations
                // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-visio/jj245869%28v%3doffice.15%29#rest-resource-endpoint
                // It ONLY works with Site Columns already present in the parent content content type or  with list columns if they are already added to the list
                // TODO: Probably worthy to recommend not using this endpoint for adding fieldlinks... What alternative ?
                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    FieldInternalName,
                    Hidden,
                    Required,
                    ReadOnly,
                    ShowInDisplayForm,
                    DisplayName = HasValue(nameof(DisplayName)) ? DisplayName : null
                }.AsExpando(ignoreNullValues: true);
                return new ApiCall(entity.SharePointGet, ApiType.SPORest, JsonSerializer.Serialize(addParameters, typeof(ExpandoObject)));
            };
        }
    }
}
