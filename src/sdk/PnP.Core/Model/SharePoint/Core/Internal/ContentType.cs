using PnP.Core.Services;
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
        /// <summary>
        /// Class to model the Rest Content Type Add request
        /// </summary>
        internal class ContentTypeAdd : RestBaseAdd<IContentType>
        {
            //public string Id { get; set; }
            public string Description { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }

            internal ContentTypeAdd(BaseDataModel<IContentType> model, string id, string name, string description, string group) : base(model)
            {
                //Id = id;
                Name = name;
                Description = description;
                Group = group;
            }
        }
        
        public ContentType()
        {
            // Handler to construct the Add request for this content type
            AddApiCallHandler = (keyValuePairs) =>
            {
                // Content type creation by specifying the parent id is not possible using SharePoint REST
                //https://github.com/pnp/pnpjs/issues/457
                //https://github.com/SharePoint/sp-dev-docs/issues/3276
                //https://stackoverflow.com/questions/55529315/how-to-create-site-content-type-with-id-using-rest-api

                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.Instance.GetClassInfo<IContentType>(GetType(), this);

                // Adding new content types on a list is not something we should promote
                if (entity.Target == typeof(List))
                {
                    throw new ClientException(ErrorType.Unsupported, "Adding new content types on a list is not possible, use the AddAvailableContentType method to add an existing site content type");
                }

                var addParameters = new ContentTypeAdd(this, StringId, Name, Description, Group);
                return new ApiCall(entity.SharePointGet, ApiType.SPORest, JsonSerializer.Serialize(addParameters));
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
            var entity = EntityManager.Instance.GetClassInfo<IContentType>(GetType(), this);

            return new ApiCall($"{entity.SharePointGet}/AddAvailableContentType", ApiType.SPORest, bodyContent);
        }

        public IContentType AddAvailableContentType(Batch batch, string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            BaseBatchRequest(batch, apiCall, HttpMethod.Post, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler);
            return this;
        }

        public async Task<IContentType> AddAvailableContentTypeAsync(string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            await BaseRequest(apiCall, HttpMethod.Post, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
            return this;
        }
        #endregion
    }
}
