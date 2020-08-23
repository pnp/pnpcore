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
        public ContentType()
        {
            PostMappingHandler = (json) =>
            {
                // In the case of Add operation, the CSOM api is used and JSON mapping to model is not possible
                // So here, populate the Rest Id metadata field to enable actions upon 
                // this content type without having to read it again from the server
                // TODO This might be replaced by a more generic approach ensuring the metadata on object created with CSOM API
                AddMetadata(PnPConstants.MetaDataRestId, StringId);
                AddMetadata(PnPConstants.MetaDataType, "SP.ContentType");
            };

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

                // Fallback to CSOM call
                string actualeDescription = !string.IsNullOrEmpty(Description)
                    ? $@"<Property Name=""Description"" Type=""String"">{Description}</Property>"
                    : $@"<Property Name=""Description"" Type=""Null"" />";
                string actualGroup = !string.IsNullOrEmpty(Group)
                    ? $@"<Property Name=""Group"" Type=""String"">{Group}</Property>"
                    : @"<Property Name=""Group"" Type=""Null"" />";

                string appName = "PnP SDK"; // TODO Replace by appropriate value
                string xml =
$@"<Request xmlns=""http://schemas.microsoft.com/sharepoint/clientquery/2009"" AddExpandoFieldTypeSuffix=""true"" SchemaVersion=""15.0.0.0"" LibraryVersion=""16.0.0.0"" ApplicationName=""{appName}""><Actions><ObjectPath Id=""40"" ObjectPathId=""39"" /><ObjectIdentityQuery Id=""41"" ObjectPathId=""39"" /></Actions><ObjectPaths><Method Id=""39"" ParentId=""5"" Name=""Add""><Parameters><Parameter TypeId=""{{168f3091-4554-4f14-8866-b20d48e45b54}}"">{actualeDescription}{actualGroup}<Property Name=""Id"" Type=""String"">{StringId}</Property><Property Name=""Name"" Type=""String"">{Name}</Property><Property Name=""ParentContentType"" Type=""Null"" /></Parameter></Parameters></Method><Property Id=""5"" ParentId=""3"" Name=""ContentTypes"" /><Property Id=""3"" ParentId=""1"" Name=""Web"" /><StaticProperty Id=""1"" TypeId=""{{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}}"" Name=""Current"" /></ObjectPaths></Request>";

                return new ApiCall(xml);
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
