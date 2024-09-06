using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests.Terms;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = V, LinqGet = baseUri)]
    internal sealed class Term : BaseDataModel<ITerm>, ITerm
    {
        private const string baseUri = "sites/{hostname},{Site.Id},{Web.Id}/termstore/sets/{TermSet.GraphId}/terms";
        private const string V = baseUri + "/{GraphId}";

        #region Construction
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

                if (IsPropertyAvailable(p => p.Descriptions) && (Descriptions as TermLocalizedDescriptionCollection).Count > 0)
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
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                string termApi;
                if (Parent != null && Parent.GetType() == typeof(TermCollection) && (Parent.Parent != null && !(Parent.Parent.GetType() == typeof(Term))))
                {
                    // we're adding a root level term
                    termApi = $"sites/{{hostname}},{{Site.Id}},{{Web.Id}}/termstore/sets/{Set.Id}/children";
                }
                else
                {
                    // we're adding a term to an existing term
                    termApi = $"sites/{{hostname}},{{Site.Id}},{{Web.Id}}/termstore/sets/{Set.Id}/terms/{{Parent.GraphId}}/children";
                }

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, termApi).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.Graph, bodyContent);
            };

            ExpandUpdatePayLoad = (payload) =>
            {
                List<ExpandoObject> propertiesToAdd = new List<ExpandoObject>();
                foreach (var property in Properties)
                {
                    ExpandoObject propertyToAdd = new ExpandoObject();
                    propertyToAdd.SetProperty("key", property.KeyField);
                    propertyToAdd.SetProperty("value", property.Value);
                    propertiesToAdd.Add(propertyToAdd);
                }
                payload.SetProperty("properties", propertiesToAdd.ToArray());

                List<ExpandoObject> localizedLabelsToAdd = new List<ExpandoObject>();
                foreach (var label in Labels)
                {
                    ExpandoObject labelToAdd = new ExpandoObject();
                    labelToAdd.SetProperty("name", label.Name);
                    labelToAdd.SetProperty("languageTag", label.LanguageTag);
                    labelToAdd.SetProperty("isDefault", label.IsDefault);
                    localizedLabelsToAdd.Add(labelToAdd);
                }
                payload.SetProperty("labels", localizedLabelsToAdd.ToArray());

                if (IsPropertyAvailable(p => p.Descriptions) && (Descriptions as TermLocalizedDescriptionCollection).Count > 0)
                {
                    List<ExpandoObject> localizedDescriptionsToAdd = new List<ExpandoObject>();
                    foreach (var localizedDescription in Descriptions)
                    {
                        ExpandoObject descriptionToAdd = new ExpandoObject();
                        descriptionToAdd.SetProperty("description", localizedDescription.Description);
                        descriptionToAdd.SetProperty("languageTag", localizedDescription.LanguageTag);
                        localizedDescriptionsToAdd.Add(descriptionToAdd);
                    }

                    payload.SetProperty("descriptions", localizedDescriptionsToAdd.ToArray());
                }
            };

            // Override update and delete calls to handle the flexible parent issue 
            // (term of a term has a term as parent.parent, while term of termset has the termset as parent.parent)

            GetApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideGetApiCall(apiCallRequest).ConfigureAwait(false);
            };

            GetApiCallNonExpandableCollectionOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideGetApiCall(apiCallRequest).ConfigureAwait(false);
            };

            UpdateApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideUpdateOrDeleteApiCall(apiCallRequest).ConfigureAwait(false);
            };

            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                return await OverrideUpdateOrDeleteApiCall(apiCallRequest).ConfigureAwait(false);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public ITermLocalizedLabelCollection Labels { get => GetModelCollectionValue<ITermLocalizedLabelCollection>(); }

        public ITermLocalizedDescriptionCollection Descriptions { get => GetModelCollectionValue<ITermLocalizedDescriptionCollection>(); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        [GraphProperty("set", Expandable = true)]
        public ITermSet Set
        {
            get
            {
                // Since we quite often have the termset already as part of the term collection let's use that 
                var parentSet = GetParentByType(typeof(TermSet));
                if (parentSet != null)
                {
                    InstantiateNavigationProperty();
                    SetValue(parentSet as TermSet);
                    return GetValue<ITermSet>();
                }

                return GetModelValue<ITermSet>();
            }
        }


        [GraphProperty("children", Expandable = true)]
        public ITermCollection Terms { get => GetModelCollectionValue<ITermCollection>(); }

        public ITermPropertyCollection Properties { get => GetModelCollectionValue<ITermPropertyCollection>(); }

        [GraphProperty("relations", Get = "sites/{hostname},{Site.Id},{Web.Id}/termstore/sets/{Parent.GraphId}/terms/{GraphId}/relations?$expand=fromTerm,set,toTerm")]
        public ITermRelationCollection Relations { get => GetModelCollectionValue<ITermRelationCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }

        #endregion

        #region Extension methods
        private async Task<ApiCallRequest> OverrideUpdateOrDeleteApiCall(ApiCallRequest apiCallRequest)
        {
            var request = await ApiHelper.ParseApiRequestAsync(this, $"sites/{{hostname}},{{Site.Id}},{{Web.Id}}/termstore/sets/{Set.Id}/terms/{{GraphId}}").ConfigureAwait(false);

            var apiCall = new ApiCall(request, ApiType.Graph, apiCallRequest.ApiCall.JsonBody);
            return new ApiCallRequest(apiCall);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<ApiCallRequest> OverrideGetApiCall(ApiCallRequest apiCallRequest)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // The termset ID that was replaced by default is wrong whenever we're getting information for a child of a child term
            var index1 = apiCallRequest.ApiCall.Request.IndexOf("/termstore/sets/");
            var index2 = apiCallRequest.ApiCall.Request.IndexOf("/terms/");

            string request = null;
            if (index1 > 0 && index2 > 0 && index2 > index1 && Set.IsPropertyAvailable(p => p.Id))
            {
                request = $"{apiCallRequest.ApiCall.Request.Substring(0, index1 + 16)}{Set.Id}{apiCallRequest.ApiCall.Request.Substring(index2)}";
            }

            if (request != null)
            {
                var apiCall = new ApiCall(request, ApiType.Graph, apiCallRequest.ApiCall.JsonBody, apiCallRequest.ApiCall.ReceivingProperty);
                return new ApiCallRequest(apiCall);
            }
            else
            {
                // Just return what we got
                return apiCallRequest;
            }
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

            var property = Properties.FirstOrDefault(p => p.KeyField == key);
            if (property != null)
            {
                // update
                property.Value = value;
            }
            else
            {
                // add
                (Properties as TermPropertyCollection).Add(new TermProperty() { KeyField = key, Value = value });
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
                (Labels as TermLocalizedLabelCollection).Add(new TermLocalizedLabel() { Name = label, LanguageTag = languageTag, IsDefault = isDefault });
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
                (Descriptions as TermLocalizedDescriptionCollection).Add(new TermLocalizedDescription() { Description = description, LanguageTag = languageTag });
            }
        }

        private ApiCall GetByIdApiCall(string id)
        {
            return new ApiCall($"sites/{{hostname}},{{Site.Id}},{{Web.Id}}/termstore/sets/{Set.Id}/terms/{id}", ApiType.Graph);
        }

        internal async Task<ITerm> GetByIdAsync(string id, params Expression<Func<ITerm, object>>[] expressions)
        {
            await BaseRetrieveAsync(apiOverride: GetByIdApiCall(id), fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return this;
        }


        internal async Task<Term> GetParentAsync()
        {
            GetParentOfTermRequest request = new GetParentOfTermRequest(PnPContext, Guid.Parse(Id));

            ApiCall getParentOfTermRequest = new ApiCall(
                new List<Services.Core.CSOM.Requests.IRequest<object>>()
                {
                    request
                })
            {
                Request = PnPContext.Uri.ToString()
            };

            // Make the CSOM call on a dummy term object otherwise the term collections will be reinitialized
            var newTerm = new Term
            {
                PnPContext = PnPContext
            };

            var csomResult = await newTerm.RawRequestAsync(getParentOfTermRequest, HttpMethod.Post).ConfigureAwait(false);
            return csomResult.ApiCall.CSOMRequests[0].Result as Term; 
        }

        #endregion
    }
}