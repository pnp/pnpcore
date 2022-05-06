using PnP.Core.Model.Security;
using PnP.Core.Services;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = tagUri, LinqGet = baseUri, Beta = true)]
    internal sealed class TeamTagMember : BaseDataModel<ITeamTagMember>, ITeamTagMember
    {
        private const string baseUri = "teams/{Parent.Parent.GraphId}/tags/{Parent.GraphId}/Members";
        private const string tagUri = baseUri + "/{GraphId}";

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public string UserId { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }

        #endregion
    }
}
