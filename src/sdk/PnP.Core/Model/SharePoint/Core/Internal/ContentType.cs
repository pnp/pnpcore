using PnP.Core.Services;
using PnP.Core.Utilities;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ContentType", Target = typeof(Web), Uri = "_api/Web/ContentTypes('{Id}')", Get = "_api/web/contenttypes", LinqGet = "_api/web/contenttypes")]
    [SharePointType("SP.ContentType", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/ContentTypes('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class ContentType
    {
        public ContentType()
        {
            // Handler to construct the Add request for this content type
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Content type creation by specifying the parent id is not possible using SharePoint REST
                //https://github.com/pnp/pnpjs/issues/457
                //https://github.com/SharePoint/sp-dev-docs/issues/3276
                //https://stackoverflow.com/questions/55529315/how-to-create-site-content-type-with-id-using-rest-api

                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

                // Adding new content types on a list is not something we should allow
                if (entity.Target == typeof(List))
                {
                    throw new ClientException(ErrorType.Unsupported, "Adding new content types on a list is not possible, use the AddAvailableContentType method to add an existing site content type");
                }

                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    Name,
                    Description,
                    Group,
                }.AsExpando();
                return new ApiCall(entity.SharePointGet, ApiType.SPORest, JsonSerializer.Serialize(addParameters, typeof(ExpandoObject)));
            };
        }

        #region AddAvailableContentType
        private ApiCall AddAvailableContentTypeApiCall(string id)
        {
            dynamic body = new ExpandoObject();
            body.contentTypeId = id;

            var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

            // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
            // automatically provide the correct 'parent'
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

            return new ApiCall($"{entity.SharePointGet}/AddAvailableContentType", ApiType.SPORest, bodyContent);
        }

        internal async Task<IContentType> AddAvailableContentTypeBatchAsync(Batch batch, string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            await RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return this;
        }

        internal async Task<IContentType> AddAvailableContentTypeAsync(string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return this;
        }
        #endregion
    }
}
