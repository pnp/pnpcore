using PnP.Core.Services;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = "termstore/sets/{Parent.GraphId}/terms/{GraphId}", LinqGet = "termstore/sets/{Parent.GraphId}/terms/{GraphId}/children",  Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Term
    {
        public Term()
        {
            // Handler to construct the Add request for this group
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic localizedLabels = new List<dynamic>();
                foreach (var localizedLabel in Labels)
                {
                    dynamic field = new ExpandoObject();
                    field.languageTag = localizedLabel.LanguageTag;
                    field.name = localizedLabel.Name;
                    field.isDefault = localizedLabel.IsDefault;
                    localizedLabels.Add(field);
                }

                dynamic body = new ExpandoObject();
                body.labels = localizedLabels;

                if (this.IsPropertyAvailable(p => p.Descriptions) && Descriptions.Count > 0)
                {
                    dynamic localizedDescriptions = new List<dynamic>();
                    foreach (var localizedDescription in Descriptions)
                    {
                        dynamic field = new ExpandoObject();
                        field.languageTag = localizedDescription.LanguageTag;
                        field.description = localizedDescription.Description;
                        localizedDescriptions.Add(field);
                    }

                    body.descriptions = localizedDescriptions;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                string termApi;
                if (Parent != null && Parent.GetType() == typeof(TermCollection) && (Parent.Parent != null && !(Parent.Parent.GetType() == typeof(Term))))
                {
                    // we're adding a term to an existing term
                    termApi = $"termstore/sets/{Set.Id}/children";
                }
                else
                {
                    // we're adding a root level term
                    termApi = $"termstore/sets/{Set.Id}/terms/{{Parent.GraphId}}/children";
                }

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, termApi).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };

            // Override update and delete calls to handle the flexible parent issue 
            // (term of a term has a term as parent.parent, while term of termset has the termset as parent.parent)
            UpdateApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideApiCall(apiCallRequest).ConfigureAwait(false);
            };

            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideApiCall(apiCallRequest).ConfigureAwait(false);
            };
        }

        private async Task<ApiCallRequest> OverrideApiCall(ApiCallRequest apiCallRequest)
        {
            var request = await ApiHelper.ParseApiRequestAsync(this, $"termstore/sets/{Set.Id}/terms/{{GraphId}}").ConfigureAwait(false);
            var apiCall = new ApiCall(request, ApiType.GraphBeta, apiCallRequest.ApiCall.JsonBody);
            return new ApiCallRequest(apiCall);
        }
    }
}
