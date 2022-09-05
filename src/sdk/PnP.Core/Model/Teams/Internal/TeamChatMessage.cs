using PnP.Core.Services;
using System;
using System.Threading.Tasks;

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
                var bodyContent = TeamChatMessageHelper.GenerateMessageBody(this);

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

        public async Task<ITeamChatMessageReply> AddReplyAsync(ChatMessageOptions options)
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

            var message = new TeamChatMessageReply
            {
                Parent = this,
                PnPContext = PnPContext
            };

            TeamChatMessageHelper.AddChatMessageOptions(options, message);
            
            await message.AddAsync().ConfigureAwait(false);

            return message;
        }

        public ITeamChatMessageReply AddReply(ChatMessageOptions options)
        {
            return AddReplyAsync(options).GetAwaiter().GetResult();
        }

        public async Task<ITeamChatMessageReply> AddReplyAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddReplyAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        public ITeamChatMessageReply AddReply(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddReplyAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }

        public async Task<ITeamChatMessageReply> AddReplyBatchAsync(Batch batch, ChatMessageOptions options)
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

            var message = new TeamChatMessageReply
            {
                Parent = this,
                PnPContext = PnPContext
            };

            TeamChatMessageHelper.AddChatMessageOptions(options, message);

            await message.AddBatchAsync(batch).ConfigureAwait(false);

            return message;
        }

        public ITeamChatMessageReply AddReplyBatch(Batch batch, ChatMessageOptions options)
        {
            return AddReplyBatchAsync(batch, options).GetAwaiter().GetResult();
        }

        public async Task<ITeamChatMessageReply> AddReplyBatchAsync(ChatMessageOptions options)
        {
            return await AddReplyBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        public ITeamChatMessageReply AddReplyBatch(ChatMessageOptions options)
        {
            return AddReplyBatchAsync(PnPContext.CurrentBatch, options).GetAwaiter().GetResult();
        }

        public async Task<ITeamChatMessageReply> AddReplyBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddReplyBatchAsync(batch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        public ITeamChatMessageReply AddReplyBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddReplyBatchAsync(batch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }

        public async Task<ITeamChatMessageReply> AddReplyBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddReplyBatchAsync(PnPContext.CurrentBatch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        public ITeamChatMessageReply AddReplyBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddReplyBatchAsync(PnPContext.CurrentBatch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }



        #endregion
    }
}
