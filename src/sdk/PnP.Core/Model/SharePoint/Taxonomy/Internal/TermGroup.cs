using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{

    [GraphType(Uri = V, LinqGet = baseUri, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TermGroup : BaseDataModel<ITermGroup>, ITermGroup
    {
        private const string baseUri = "termstore/groups";
        private const string V = baseUri + "/{GraphId}";

        #region Construction
        public TermGroup()
        {

            // Handler to construct the Add request for this group
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.displayName = Name;
                if (this.IsPropertyAvailable(p => p.Description))
                {
                    body.description = Description;
                }
                if (this.IsPropertyAvailable(p => p.Scope))
                {
                    body.scope = Scope;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);                

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public TermGroupScope Scope { get => GetValue<TermGroupScope>(); set => SetValue(value); }

        [GraphProperty("sets", Get = "termstore/groups/{GraphId}/sets")]
        public ITermSetCollection Sets { get => GetModelCollectionValue<ITermSetCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
        #endregion
    }
}
