using PnP.Core.Model.Teams;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Me
{
    [GraphType(Uri = chatMessageUri, Beta = true, LinqGet = baseUri)]
    internal sealed class ChatMessage : BaseDataModel<IChatMessage>, IChatMessage
    {
        private const string baseUri = "chats/{Parent.GraphId}/messages";
        private const string chatMessageUri = baseUri + "/{GraphId}";

        #region Construction
        public ChatMessage()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.body = new ExpandoObject();
                body.body.content = Body.Content;
                body.body.contentType = Body.ContentType.ToString();

                if (Attachments.Length > 0)
                {
                    dynamic attachmentList = new List<dynamic>();
                    foreach (var attachment in Attachments)
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

                if (HostedContents.Length > 0)
                {
                    dynamic hostedContentList = new List<dynamic>();
                    foreach (var hostedContent in HostedContents)
                    {
                        dynamic hContent = new ExpandoObject();
                        hContent.contentBytes = hostedContent.ContentBytes;
                        hContent.contentType = hostedContent.ContentType;

                        //Complex named parameter
                        ((IDictionary<string, object>)hContent).Add("@microsoft.graph.temporaryId", hostedContent.Id);

                        hostedContentList.Add(hContent);
                    }

                    body.hostedContents = hostedContentList;
                }

                if (!string.IsNullOrEmpty(Subject))
                {
                    body.subject = Subject;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ReplyToId { get => GetValue<string>(); set => SetValue(value); }

        public IChatIdentitySet From { get => GetModelValue<IChatIdentitySet>(); }

        public string Etag { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageType MessageType { get => GetValue<ChatMessageType>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset DeletedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string Subject { get => GetValue<string>(); set => SetValue(value); }

        public IChatMessageContent Body { get => GetModelValue<IChatMessageContent>(); set => SetModelValue(value); }

        public string Summary { get => GetValue<string>(); set => SetValue(value); }

        public string Locale { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageImportance Importance { get => GetValue<ChatMessageImportance>(); set => SetValue(value); }

        public IChatMessageReactionCollection Reactions { get => GetModelCollectionValue<IChatMessageReactionCollection>(); }

        public IChatMessageMentionCollection Mentions { get => GetModelCollectionValue<IChatMessageMentionCollection>(); }

        public IChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<IChatMessageAttachmentCollection>(); set => SetModelValue(value); }

        public IChatMessageHostedContentCollection HostedContents { get => GetModelCollectionValue<IChatMessageHostedContentCollection>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
