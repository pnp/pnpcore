using PnP.Core.Services;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    internal static class TeamChatMessageHelper
    {
        internal static void AddChatMessageOptions(ChatMessageOptions options, TeamChatMessage newChatMessage)
        {
            newChatMessage.Body = GetTeamsChatMessageContent(options, newChatMessage.PnPContext, newChatMessage);

            if (options.Attachments != null && options.Attachments.Count > 0)
            {
                newChatMessage.Attachments = GetMessageAttachments(options.Attachments);
            }

            if (options.HostedContents != null && options.HostedContents.Count > 0)
            {
                newChatMessage.HostedContents = GetMessageHostedContents(options.HostedContents);
            }


            if (options.Mentions != null && options.Mentions.Count > 0)
            {
                newChatMessage.Mentions = GetMessageMentions(options.Mentions);
            }

            newChatMessage.Subject = options.Subject;
        }
        internal static void AddChatMessageOptions(ChatMessageOptions options, TeamChatMessageReply newChatMessage)
        {
            newChatMessage.Body = GetTeamsChatMessageContent(options, newChatMessage.PnPContext, newChatMessage);

            if (options.Attachments != null && options.Attachments.Count > 0)
            {
                newChatMessage.Attachments = GetMessageAttachments(options.Attachments);
            }

            if (options.HostedContents != null && options.HostedContents.Count > 0)
            {
                newChatMessage.HostedContents = GetMessageHostedContents(options.HostedContents);
            }


            if (options.Mentions != null && options.Mentions.Count > 0)
            {
                newChatMessage.Mentions = GetMessageMentions(options.Mentions);
            }

            newChatMessage.Subject = options.Subject;
        }

        private static ITeamChatMessageMentionCollection GetMessageMentions(List<ChatMessageMentionOptions> mentions)
        {
            var mentionsCollection = new TeamChatMessageMentionCollection();

            foreach (var mention in mentions)
            {
                var mentionedIdSet = new TeamChatMessageMentionedIdentitySet
                {
                    User = mention.Mentioned.User,
                    Application = mention.Mentioned.Application,
                    Conversation = mention.Mentioned.Conversation,
                    Tag = mention.Mentioned.Tag
                };

                mentionsCollection.Add(new TeamChatMessageMention()
                {
                    Id = mention.Id,
                    MentionText = mention.MentionText,
                    Mentioned = mentionedIdSet
                });
            }
            return mentionsCollection;
        }

        private static ITeamChatMessageHostedContentCollection GetMessageHostedContents(List<ChatMessageHostedContentOptions> hostedContents)
        {
            var hostedContentsCollection = new TeamChatMessageHostedContentCollection();

            foreach (var hostedContentOption in hostedContents)
            {
                hostedContentsCollection.Add(new TeamChatMessageHostedContent()
                {
                    Id = hostedContentOption.Id,
                    ContentBytes = hostedContentOption.ContentBytes,
                    ContentType = hostedContentOption.ContentType
                });
            }
            return hostedContentsCollection;
        }

        private static ITeamChatMessageAttachmentCollection GetMessageAttachments(List<ChatMessageAttachmentOptions> attachments)
        {
            var attachmentsCollection = new TeamChatMessageAttachmentCollection();

            foreach (var optionAttachment in attachments)
            {
                attachmentsCollection.Add(new TeamChatMessageAttachment()
                {
                    Id = optionAttachment.Id,
                    Content = optionAttachment.Content,
                    ContentType = optionAttachment.ContentType,
                    Name = optionAttachment.Name,
                    ContentUrl = optionAttachment.ContentUrl,
                    ThumbnailUrl = optionAttachment.ThumbnailUrl
                });
            }

            return attachmentsCollection;
        }

        private static ITeamChatMessageContent GetTeamsChatMessageContent(ChatMessageOptions options, PnPContext pnPContext, IDataModelParent newChatMessage)
        {
            return new TeamChatMessageContent
            {
                PnPContext = pnPContext,
                Parent = newChatMessage,
                Content = options.Content,
                ContentType = options.ContentType,
            };
        }

        internal static string GenerateReplyBody(ITeamChatMessageReply chatMessage)
        {
            // Define the JSON body of the update request based on the actual changes
            dynamic body = new ExpandoObject();
            body.body = GetBody(chatMessage.Body);

            if (chatMessage.Attachments.Length > 0)
            {
                body.attachments = GetAttachmentsList(chatMessage.Attachments);
            }

            if (chatMessage.HostedContents.Length > 0)
            {
                body.hostedContents = GetHostedContentsList(chatMessage.HostedContents);
            }

            if (chatMessage.Mentions.Length > 0)
            {
                body.mentions = GetMentionsList(chatMessage.Mentions);
            }

            if (!string.IsNullOrEmpty(chatMessage.Subject))
            {
                body.subject = chatMessage.Subject;
            }

            // Serialize object to json
            return JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);
        }

        internal static string GenerateMessageBody(ITeamChatMessage chatMessage)
        {
            // Define the JSON body of the update request based on the actual changes
            dynamic body = new ExpandoObject();
            body.body = GetBody(chatMessage.Body);

            if (chatMessage.Attachments.Length > 0)
            {
                body.attachments = GetAttachmentsList(chatMessage.Attachments);
            }

            if (chatMessage.HostedContents.Length > 0)
            {
                body.hostedContents = GetHostedContentsList(chatMessage.HostedContents);
            }

            if (chatMessage.Mentions.Length > 0)
            {
                body.mentions = GetMentionsList(chatMessage.Mentions);
            }

            if (!string.IsNullOrEmpty(chatMessage.Subject))
            {
                body.subject = chatMessage.Subject;
            }

            // Serialize object to json
            return JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);
        }

        private static dynamic GetMentionsList(ITeamChatMessageMentionCollection mentions)
        {
            dynamic mentionList = new List<dynamic>();
            foreach (var mention in mentions)
            {
                dynamic ment = new ExpandoObject();
                ment.id = mention.Id;
                ment.mentionText = mention.MentionText;
                ment.mentioned = CreateMentionedObject(mention.Mentioned);
                mentionList.Add(ment);
            }
            return mentionList;
        }

        private static dynamic GetHostedContentsList(ITeamChatMessageHostedContentCollection hostedContents)
        {
            dynamic hostedContentList = new List<dynamic>();
            foreach (var hostedContent in hostedContents)
            {
                dynamic hContent = new ExpandoObject();
                hContent.contentBytes = hostedContent.ContentBytes;
                hContent.contentType = hostedContent.ContentType;

                //Complex named parameter
                ((IDictionary<string, object>)hContent).Add("@microsoft.graph.temporaryId", hostedContent.Id);

                hostedContentList.Add(hContent);
            }
            return hostedContentList;
        }

        private static dynamic GetAttachmentsList(ITeamChatMessageAttachmentCollection attachments)
        {
            dynamic attachmentList = new List<dynamic>();
            foreach (var attachment in attachments)
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
            return attachmentList;
        }

        private static dynamic GetBody(ITeamChatMessageContent body)
        {
            dynamic bodyObject = new ExpandoObject();
            bodyObject.content = body.Content;
            bodyObject.contentType = body.ContentType.ToString();
            return bodyObject;
        }

        private static dynamic CreateMentionedObject(ITeamChatMessageMentionedIdentitySet mentioned)
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

    }
}
