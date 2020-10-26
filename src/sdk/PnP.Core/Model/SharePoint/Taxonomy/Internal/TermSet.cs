using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    //TODO: the delete uri needs be removed once the needed fix is deployed to Graph
    [GraphType(Uri = V, Delete = "termStore/groups/{Parent.GraphId}/sets/{GraphId}", LinqGet = "termStore/groups/{Parent.GraphId}/sets", Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TermSet
    {
        private const string baseUri = "termstore/sets";
        private const string V = baseUri + "/{GraphId}";

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

                if (this.IsPropertyAvailable(p => p.Description))
                {
                    body.description = Description;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };
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
                (Properties as TermSetPropertyCollection).Add(new TermSetProperty() { KeyField = key, Value = value });
            }
        }

    }
}
