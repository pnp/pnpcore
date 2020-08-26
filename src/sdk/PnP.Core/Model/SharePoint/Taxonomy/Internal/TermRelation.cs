using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = V, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TermRelation
    {
        private const string baseUri = "termstore/sets/{Parent.GraphId}/relations";
        private const string V = baseUri + "/{GraphId}";

        public TermRelation()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case "TermRelationType": return JsonMappingHelper.ToEnum<TermRelationType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this term relation
            AddApiCallHandler = async (keyValuePairs) =>
            {
                dynamic body = new ExpandoObject();
                body.relationship = Relationship.ToString().ToLowerInvariant();

                if (FromTerm != null)
                {
                    body.fromTerm = new
                    {
                        id = FromTerm.Id
                    };
                }

                body.set = new
                {
                    id = Set.Id
                };

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                Term parentTerm = null;
                if (Parent != null && Parent.GetType() == typeof(TermRelationCollection) && (Parent.Parent != null && (Parent.Parent.GetType() == typeof(Term))))
                {
                    parentTerm = Parent.Parent as Term;
                }
                
                if (parentTerm == null)
                {
                    throw new ClientException(ErrorType.ConfigurationError, "Added a relationship can only be done starting from a term");
                }

                string termApi = $"termstore/sets/{parentTerm.Set.Id}/terms/{parentTerm.Id}/relations";
                
                var apiCall = await ApiHelper.ParseApiRequestAsync(this, termApi).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };
        }
    }
}
