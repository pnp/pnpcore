using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = chatMessageUri, Beta = true, LinqGet = baseUri)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.GraphId}/messages";
        private const string chatMessageUri = baseUri + "/{GraphId}";

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

                if(Attachments.Length > 0)
                {
                    dynamic attachmentList = new List<dynamic>();
                    foreach(var attachment in Attachments)
                    {
                        dynamic attach = new ExpandoObject();
                        attach.id = attachment.Id;
                        attach.content = attachment.Content;
                        attach.contentUrl = attachment.ContentUrl;
                        attach.contentType = attachment.ContentType;
                        attach.name = attachment.Name;
                        attach.thumbnailUrl = attachment.ThumbnailUrl;

                        attachmentList.Add(attach);
                    }

                    body.attachments = attachmentList;
                }

                //TODO: Add subject
                //body.subject = Subject;

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

        public ITeamChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<ITeamChatMessageAttachmentCollection>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
