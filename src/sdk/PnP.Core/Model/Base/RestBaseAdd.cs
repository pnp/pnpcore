using System.Text.Json.Serialization;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base class for REST Add json payload
    /// </summary>
    /// <typeparam name="TModel">Model we're adding</typeparam>
    internal abstract class RestBaseAdd<TModel>
    {

        internal class RestBaseAddType
        {
            internal RestBaseAddType(string spType)
            {
                SpType = spType;
            }

            [JsonPropertyName("type")]
            public string SpType { get; set; }
        }

        [JsonPropertyName("__metadata")]
        public RestBaseAddType Metadata { get; set; }

        internal RestBaseAdd(BaseDataModel<TModel> model)
        {
            // Get entity information for the entity to load
            var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(model.GetType(), model);

            // Each model that can be handled via SharePoint rest does need to have it's type defined, so populate that by default
            Metadata = new RestBaseAddType(entityInfo.SharePointType);
        }


        internal RestBaseAdd(string spType)
        {
            Metadata = new RestBaseAddType(spType);
        }
    }
}
