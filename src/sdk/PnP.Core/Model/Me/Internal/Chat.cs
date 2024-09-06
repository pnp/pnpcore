using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Me
{
    [GraphType(LinqGet = baseUri, Uri = chatUri)]
    internal sealed class Chat : BaseDataModel<IChat>, IChat
    {
        private const string baseUri = "chats";
        private const string chatUri = baseUri + "/{GraphId}";

        #region Constructor
        public Chat()
        {
            AddApiCallHandler = async (keyValuePairs) =>
            {
                dynamic body = new ExpandoObject();
                body.ChatType = ChatType.ToString();

                dynamic memberList = new List<dynamic>();
                foreach (var member in Members.AsRequested())
                {
                    dynamic memb = new ExpandoObject();
                    memb.roles = member.Roles;
                    ((IDictionary<string, object>)memb).Add(PnPConstants.MetaDataGraphType, "#microsoft.graph.aadUserConversationMember");
                    ((IDictionary<string, object>)memb).Add("user@odata.bind", $"{CloudManager.GetGraphBaseUrl(PnPContext)}v1.0/users('" + member.UserId + "')");
                    memberList.Add(memb);
                }

                body.Members = memberList;

                if (!string.IsNullOrEmpty(Topic))
                    body.Topic = Topic;

                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);
                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }

        #endregion

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string Topic { get => GetValue<string>(); set => SetValue(value); }

        public DateTime CreatedDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastUpdatedDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public string ChatType { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("members", Get = chatUri + "/members")]
        public IAadUserConversationMemberCollection Members { get => GetModelCollectionValue<IAadUserConversationMemberCollection>(); set => SetModelValue(value); }

        [GraphProperty("messages", Get = chatUri + "/messages")]
        public IChatMessageCollection Messages { get => GetModelCollectionValue<IChatMessageCollection>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }

        public string WebUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid TenantId { get => GetValue<Guid>(); set => SetValue(value); }


        #endregion
    }
}
