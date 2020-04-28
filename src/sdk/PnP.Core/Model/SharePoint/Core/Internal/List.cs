using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint.Core;
using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List class, write your custom code here
    /// </summary>
    [SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Get = "_api/web/lists", Update = "_api/web/lists/getbyid(guid'{Id}')")]
    [GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class List
    {
        /// <summary>
        /// Class to model the Rest List Add request
        /// </summary>
        internal class ListAdd: RestBaseAdd<IList>
        {
            public ListTemplateType BaseTemplate { get; set; }
            
            public string Title { get; set; }

            internal ListAdd(BaseDataModel<IList> model, ListTemplateType templateType, string title) : base(model) 
            {
                BaseTemplate = templateType;
                Title = title;
            }            
        }

        public List()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case "ListExperience": return JsonMappingHelper.ToEnum<ListExperience>(input.JsonElement);
                    case "ListReadingDirection": return JsonMappingHelper.ToEnum<ListReadingDirection>(input.JsonElement);
                }

                input.Log.LogWarning($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this list
            AddApiCallHandler = (keyValuePairs) =>
            {
                return new ApiCall($"_api/web/lists", ApiType.SPORest, JsonSerializer.Serialize(new ListAdd(this, TemplateType, Title)));
            };

            /** 
            // Sample handler that shows how to override the API call used for the delete of this entity
            DeleteApiCallOverrideHandler = (ApiCall apiCall) =>
            {
                return apiCall;
            };
            */

            /**
            // Sample update validation handler, can be used to prevent updates to a field (e.g. field validation, make readonly field, ...)
            ValidateUpdateHandler = (ref FieldUpdateRequest fieldUpdateRequest) => 
            {
                if (fieldUpdateRequest.FieldName == "Description")
                {
                    // Cancel update
                    //fieldUpdateRequest.CancelUpdate();

                    // Set other value to the field
                    //fieldUpdateRequest.Value = "bla";
                }
            };
            */
        }

        #region Extension methods

        private static ApiCall GetByTitleApiCall(string title)
        {
            return new ApiCall($"_api/web/lists/getbytitle('{title}')", ApiType.SPORest); 
        }

        public IList GetByTitle(Batch batch, string title, params Expression<Func<IList, object>>[] expressions)
        {
            BaseBatchGet(batch, apiOverride: GetByTitleApiCall(title), fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions);
            return this;
        }

        public IList GetByTitle(string title, params Expression<Func<IList, object>>[] expressions)
        {
            return GetByTitle(PnPContext.CurrentBatch, title, expressions);
        }

        public async Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] expressions)
        {
            await BaseGet(apiOverride: GetByTitleApiCall(title), fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return this;
        }

        #endregion
    }
}
