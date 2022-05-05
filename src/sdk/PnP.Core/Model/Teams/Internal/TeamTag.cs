using PnP.Core.Model.Security;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = tagUri, LinqGet = baseUri, Beta = true)]
    internal sealed class TeamTag : BaseDataModel<ITeamTag>, ITeamTag
    {
        private const string baseUri = "teams/{Parent.GraphId}/tags";
        private const string tagUri = baseUri + "/{GraphId}";

        public TeamTag()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                dynamic body = new ExpandoObject();
                body.displayName = DisplayName;
                
                dynamic memberList = new List<dynamic>();
                foreach (var member in Members)
                {
                    dynamic memb = new ExpandoObject();
                    memb.userId = member.UserId;
                    memberList.Add(memb);
                }

                body.members = memberList;

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }

        #region Properties

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public int MemberCount { get => GetValue<int>(); set => SetValue(value); }

        public string TeamId { get => GetValue<string>(); set => SetValue(value); }

        public TeamTagType TagType { get => GetValue<TeamTagType>(); set => SetValue(value); }

        [GraphProperty("members", Get = "teams/{Site.GroupId}/tags/{GraphId}/members", Beta = true)]
        public ITeamTagMemberCollection Members { get => GetModelCollectionValue<ITeamTagMemberCollection>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }
        #endregion
    }
}
