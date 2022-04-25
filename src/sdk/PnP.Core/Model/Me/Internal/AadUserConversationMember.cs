using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Me
{
    [GraphType(LinqGet = baseUri, Uri = memberUri)]
    internal sealed class AadUserConversationMember : BaseDataModel<IAadUserConversationMember>, IAadUserConversationMember
    {
        private const string baseUri = "me/chats/{Parent.GraphId}/Members";
        private const string memberUri = baseUri + "/{Id}";

        #region Constructor
        public AadUserConversationMember()
        {

        }

        #endregion

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public List<string> Roles { get => GetValue<List<string>>(); set => SetValue(value); }

        public string UserId { get => GetValue<string>(); set => SetValue(value); }

        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime VisibleHistoryStartDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }

        #endregion
    }
}
