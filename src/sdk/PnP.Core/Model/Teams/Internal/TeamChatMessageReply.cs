using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = chatReplyUri, Beta = true, LinqGet = baseUri)]
    internal sealed class TeamChatMessageReply : BaseDataModel<ITeamChatMessageReply>, ITeamChatMessageReply
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.ChannelIdentity.ChannelId}/messages/{Parent.GraphId}/replies";
        private const string chatReplyUri = baseUri + "/{GraphId}";

        #region Construction
        public TeamChatMessageReply()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                var bodyContent = TeamChatMessageHelper.GenerateReplyBody(this);

                var parent = Parent as TeamChatMessage;

                var callUri = baseUri.Replace("{Parent.ChannelIdentity.ChannelId}", parent.ChannelIdentity.ChannelId);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, callUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
            #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
                var parent = Parent.Parent.Parent as TeamChannel;

                var callUri = baseUri.Replace("{Parent.ChannelIdentity.ChannelId}", parent.Id);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, callUri).ConfigureAwait(false);

                api.ApiCall = new ApiCall(parsedApiCall, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                return api;
            };
            #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

        public ITeamChannelIdentity ChannelIdentity { get => GetModelValue<ITeamChannelIdentity>(); set => SetModelValue(value); }

        public string Summary { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public string Locale { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageImportance Importance { get => GetValue<ChatMessageImportance>(); set => SetValue(value); }

        public ITeamChatMessageReactionCollection Reactions { get => GetModelCollectionValue<ITeamChatMessageReactionCollection>(); }

        public ITeamChatMessageMentionCollection Mentions { get => GetModelCollectionValue<ITeamChatMessageMentionCollection>(); set => SetModelValue(value); }

        public ITeamChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<ITeamChatMessageAttachmentCollection>(); set => SetModelValue(value); }

        public ITeamChatMessageHostedContentCollection HostedContents { get => GetModelCollectionValue<ITeamChatMessageHostedContentCollection>(); set => SetModelValue(value); }

        [GraphProperty("replies", Get = "teams/{Site.GroupId}/channels/{Parent.GraphId}/messages/{GraphId}/replies")]
        public ITeamChatMessageReplyCollection Replies { get => GetModelCollectionValue<ITeamChatMessageReplyCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods

        public async Task AddReplyAsync(ChatMessageOptions options)
        {
            if (options == default)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //Minimum for a message
            if (string.IsNullOrEmpty(options.Content))
            {
                throw new ArgumentNullException(nameof(options), "parameter must include message content");
            }

            var message = new TeamChatMessage
            {
                Parent = this,
                PnPContext = PnPContext
            };

            TeamChatMessageHelper.AddChatMessageOptions(options, message);

            await message.AddAsync().ConfigureAwait(false);

        }

        public void AddReply(ChatMessageOptions options)
        {
            AddReplyAsync(options).GetAwaiter().GetResult();
        }

        public async Task AddReplyAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            await AddReplyAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        public void AddReply(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            AddReplyAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }



        #endregion
    }
}
