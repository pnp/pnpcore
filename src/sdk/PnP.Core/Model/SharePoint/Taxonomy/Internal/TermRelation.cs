using PnP.Core.Services;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = V, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TermRelation : BaseDataModel<ITermRelation>, ITermRelation
    {
        private const string baseUri = "termstore/sets/{Parent.GraphId}/relations";
        private const string V = baseUri + "/{GraphId}";

        #region Construction
        public TermRelation()
        {

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
                    throw new ClientException(ErrorType.Unsupported,
                        PnPCoreResources.Exception_Unsupported_FailedAddingTermRelation);
                }

                string termApi = $"termstore/sets/{parentTerm.Set.Id}/terms/{parentTerm.Id}/relations";

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, termApi).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public TermRelationType Relationship { get => GetValue<TermRelationType>(); set => SetValue(value); }

        public ITermSet Set { get => GetModelValue<ITermSet>(); set => SetModelValue(value); }

        public ITerm FromTerm { get => GetModelValue<ITerm>(); set => SetModelValue(value); }

        public ITerm ToTerm { get => GetModelValue<ITerm>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
