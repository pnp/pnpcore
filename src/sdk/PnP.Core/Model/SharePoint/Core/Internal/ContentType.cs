using PnP.Core.Services;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ContentType", Uri = "_api/Web/ContentTypes('{Id}')", Get = "_api/web/contenttypes", LinqGet = "_api/web/contenttypes")]
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

                var addParameters = new ContentTypeAdd(this, StringId, Name, Description, Group);
                return new ApiCall($"_api/web/contentTypes", ApiType.SPORest, JsonSerializer.Serialize(addParameters));
            };
        }
    }
}
