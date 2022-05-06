using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = chatMessageUri, Beta = true, LinqGet = baseUri)]
    internal sealed class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage
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

                if (Mentions.Length > 0)
                {
                    dynamic mentionList = new List<dynamic>();
                    foreach (var mention in Mentions)
                    {
                        dynamic ment = new ExpandoObject();
                        ment.id = mention.Id;
                        ment.mentionText = mention.MentionText;
                        ment.mentioned = CreateMentionedObject(mention.Mentioned);
                        mentionList.Add(ment);
                    }
                    body.mentions = mentionList;
                }

                if (!string.IsNullOrEmpty(Subject))
                {
                    body.subject = Subject;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }

        private dynamic CreateMentionedObject(ITeamChatMessageMentionedIdentitySet mentioned)
        {
            dynamic mentionedObj = new ExpandoObject();

            if (mentioned.User.HasValue(PnPConstants.MetaDataId))
            {
                dynamic user = new ExpandoObject();
                user.id = mentioned.User.Id;
                user.displayName = mentioned.User.DisplayName;
                user.userIdentityType = mentioned.User.UserIdentityType.ToString();
                mentionedObj.user = user;
            }

            if (mentioned.Application.HasValue(PnPConstants.MetaDataId))
            {
                dynamic application = new ExpandoObject();
                application.id = mentioned.Application.Id;
                application.displayName = mentioned.Application.DisplayName;
                mentionedObj.application = application;
            }

            if (mentioned.Conversation.HasValue(PnPConstants.MetaDataId))
            {
                dynamic conversation = new ExpandoObject();
                conversation.id = mentioned.Conversation.Id;
                if (mentioned.Conversation.HasValue("DisplayName"))
                {
                    conversation.displayName = mentioned.Conversation.DisplayName;
                }
                conversation.conversationIdentityType = mentioned.Conversation.ConversationIdentityType.ToString();
                mentionedObj.conversation = conversation;
            }

            if (mentioned.Tag.HasValue(PnPConstants.MetaDataId))
            {
                dynamic tag = new ExpandoObject();
                tag.id = mentioned.Tag.Id;
                tag.displayName = mentioned.Tag.DisplayName;
                mentionedObj.tag = tag;
            }

            return mentionedObj;
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

        public ITeamChatMessageMentionCollection Mentions { get => GetModelCollectionValue<ITeamChatMessageMentionCollection>(); set => SetModelValue(value); }

        public ITeamChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<ITeamChatMessageAttachmentCollection>(); set => SetModelValue(value); }

        public ITeamChatMessageHostedContentCollection HostedContents { get => GetModelCollectionValue<ITeamChatMessageHostedContentCollection>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
