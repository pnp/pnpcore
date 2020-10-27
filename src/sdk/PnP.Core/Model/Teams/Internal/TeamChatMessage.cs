using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = V, Beta = true, LinqGet = baseUri)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage  
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.GraphId}/messages";
        private const string V = baseUri + "/{GraphId}";

        #region Construction
        public TeamChatMessage()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.body = new ExpandoObject();
                body.body.content = Body.Content;
                body.body.contentType = Body.ContentType.ToString();

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ReplyToId { get => GetValue<string>(); set => SetValue(value); }

        public ITeamIdentitySet From { get => GetModelValue<ITeamIdentitySet>(); }

        public string Etag { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageType MessageType { get => GetValue<ChatMessageType>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset DeletedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string Subject { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChatMessageContent Body { get => GetModelValue<ITeamChatMessageContent>(); set => SetModelValue(value); }

        public string Summary { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public string Locale { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageImportance Importance { get => GetValue<ChatMessageImportance>(); set => SetValue(value); }

        public ITeamChatMessageReactionCollection Reactions { get => GetModelCollectionValue<ITeamChatMessageReactionCollection>(); }

        public ITeamChatMessageMentionCollection Mentions { get => GetModelCollectionValue<ITeamChatMessageMentionCollection>(); }

        public ITeamChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<ITeamChatMessageAttachmentCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
        #endregion
    }
}
