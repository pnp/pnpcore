using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessageCollection
    {
        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddAsync(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var newChannelChatMessage = AddNewTeamChatMessage();

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                Content = body,
                ContentType = ChatMessageContentType.Text
            };


            return await newChannelChatMessage.AddAsync().ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(Batch batch, string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var newChannelChatMessage = AddNewTeamChatMessage();

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                Content = body,
                ContentType = ChatMessageContentType.Text
            };

            return newChannelChatMessage.Add(batch) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string body)
        {
            return Add(PnPContext.CurrentBatch, body);
        }
    }
}
