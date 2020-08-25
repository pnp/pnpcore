using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = V, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Term
    {
        private const string baseUri = "termstore/sets/{Parent.GraphId}/terms";
        private const string V = baseUri + "/{GraphId}";

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

        public void AddProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var property = Properties.FirstOrDefault(p => p.Key == key);
            if (property != null)
            {
                // update
                property.Value = value;
            }
            else
            {
                // add
                Properties.Add(new TermProperty() { Key = key, Value = value });
            }
        }

        public void AddLabelAndDescription(string label, string languageTag, bool isDefault = false, string description = null)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (string.IsNullOrEmpty(languageTag))
            {
                throw new ArgumentNullException(nameof(languageTag));
            }

            // Is there already a value for the provided language
            var labelSet = Labels.FirstOrDefault(p => p.LanguageTag == languageTag);
            if (labelSet != null)
            {
                // Update
                labelSet.Name = label;
                labelSet.IsDefault = isDefault;
                if (description != null)
                {
                    AddDescription(languageTag, description);
                }
            }
            else
            {
                // Add
                Labels.Add(new TermLocalizedLabel() { Name = label, LanguageTag = languageTag, IsDefault = isDefault });
                if (description != null)
                {
                    AddDescription(languageTag, description);
                }
            }
        }

        private void AddDescription(string languageTag, string description)
        {
            var descriptionSet = Descriptions.FirstOrDefault(p => p.LanguageTag == languageTag);
            if (descriptionSet != null)
            {
                descriptionSet.Description = description;
            }
            else
            {
                Descriptions.Add(new TermLocalizedDescription() { Description = description, LanguageTag = languageTag });
            }
        }

        private ApiCall GetByIdApiCall(string id)
        {
            return new ApiCall($"termstore/sets/{Set.Id}/terms/{id}", ApiType.GraphBeta);
        }

        internal async Task<ITerm> GetByIdAsync(string id, params Expression<Func<ITerm, object>>[] expressions)
        {
            await BaseGet(apiOverride: GetByIdApiCall(id), fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return this;
        }

    }
}
