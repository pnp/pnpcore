using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests.Terms;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = V, Delete = "sites/{hostname},{Site.Id},{Web.Id}/termStore/groups/{Parent.GraphId}/sets/{GraphId}", LinqGet = "sites/{hostname},{Site.Id},{Web.Id}/termStore/groups/{Parent.GraphId}/sets")]
    internal sealed class TermSet : BaseDataModel<ITermSet>, ITermSet
    {
        private const string baseUri = "sites/{hostname},{Site.Id},{Web.Id}/termstore/sets";
        private const string V = baseUri + "/{GraphId}";

        #region Construction
        public TermSet()
        {
            // Handler to construct the Add request for this group
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic localizedNames = new List<dynamic>();
                foreach (var localizedName in LocalizedNames)
                {
                    dynamic field = new ExpandoObject();
                    field.languageTag = localizedName.LanguageTag;
                    field.name = localizedName.Name;
                    localizedNames.Add(field);
                }

                dynamic body = new ExpandoObject();
                body.localizedNames = localizedNames;
                body.parentGroup = new
                {
                    id = Group.Id
                };

                if (IsPropertyAvailable(p => p.Description))
                {
                    body.description = Description;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

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

                List<ExpandoObject> localizedNamesToAdd = new List<ExpandoObject>();
                foreach (var property in LocalizedNames)
                {
                    ExpandoObject propertyToAdd = new ExpandoObject();
                    propertyToAdd.SetProperty("name", property.Name);
                    propertyToAdd.SetProperty("languageTag", property.LanguageTag);
                    localizedNamesToAdd.Add(propertyToAdd);
                }
                payload.SetProperty("localizedNames", localizedNamesToAdd.ToArray());
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public ITermSetLocalizedNameCollection LocalizedNames { get => GetModelCollectionValue<ITermSetLocalizedNameCollection>(); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        [GraphProperty("children", Expandable = true)]
        public ITermCollection Terms { get => GetModelCollectionValue<ITermCollection>(); }

        [GraphProperty("parentGroup", Expandable = true)]
        public ITermGroup Group
        {
            get
            {
                // Since we quite often have the group already as part of the termset collection let's use that 
                if (Parent != null && Parent.Parent != null)
                {
                    InstantiateNavigationProperty();
                    SetValue(Parent.Parent as TermGroup);
                    return GetValue<ITermGroup>();
                }

                // Seems there was no group available, so process the loaded group and assign it
                return GetModelValue<ITermGroup>();
            }
        }

        public ITermSetPropertyCollection Properties { get => GetModelCollectionValue<ITermSetPropertyCollection>(); }

        [GraphProperty("relations", Get = "sites/{hostname},{Site.Id},{Web.Id}/termstore/sets/{GraphId}/relations?$expand=fromTerm,set,toTerm")]
        public ITermRelationCollection Relations { get => GetModelCollectionValue<ITermRelationCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods
        public async Task AddPropertyAsync(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            await EnsurePropertiesAsync(p => p.Properties).ConfigureAwait(false);

            CheckIfPropertyExistsAndAdd(key, value);
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

            EnsureProperties(p => p.Properties);

            CheckIfPropertyExistsAndAdd(key, value);
        }

        private void CheckIfPropertyExistsAndAdd(string key, string value)
        {
            var property = Properties.FirstOrDefault(p => p.KeyField == key);
            if (property != null)
            {
                // update
                property.Value = value;
            }
            else
            {
                // add
                (Properties as TermSetPropertyCollection).Add(new TermSetProperty() { KeyField = key, Value = value });
            }
        }

        public async Task<IList<ITerm>> GetTermsByCustomPropertyAsync(string key, string value, bool trimUnavailable = false)
        {
            var result = new List<ITerm>();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
          
            GetTermsByCustomPropertyRequest request = new GetTermsByCustomPropertyRequest(
                key, 
                value, 
                trimUnavailable,
                Id,
                Group.Id
                );

            ApiCall getTermsCall = new ApiCall(
                new List<Services.Core.CSOM.Requests.IRequest<object>>()
                {
                    request
                })
             {
                 Commit = true,
                 Request = PnPContext.Uri.ToString()
             };

            var csomResult = 
                await RawRequestAsync(getTermsCall, HttpMethod.Post)
                    .ConfigureAwait(false);

            if (csomResult.ApiCall.CSOMRequests[0] is not GetTermsByCustomPropertyRequest getTermsByCustomPropertyRequest) return result;

            foreach (var termGuidString in getTermsByCustomPropertyRequest.Result.Select(guid => guid.ToString()))
            {
                var term = await Terms
                    .Where(p => p.Id == termGuidString)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                
                result.Add(term);
            }

            return result;
        }

        public IList<ITerm> GetTermsByCustomProperty(string key, string value, bool trimUnavailable = false)
        {
            return GetTermsByCustomPropertyAsync(key, value, trimUnavailable).GetAwaiter().GetResult();
        }

        public void AddLocalizedName(string name, string languageTag)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(languageTag))
            {
                throw new ArgumentNullException(nameof(languageTag));
            }

            var newTermSetLocalizedName = new TermSetLocalizedName
            {
                Name = name,
                LanguageTag = languageTag
            };

            (LocalizedNames as TermSetLocalizedNameCollection).Add(newTermSetLocalizedName);
        }
        #endregion
    }
}