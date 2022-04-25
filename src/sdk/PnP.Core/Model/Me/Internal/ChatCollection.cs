using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Me
{
    internal sealed class ChatCollection : QueryableDataModelCollection<IChat>, IChatCollection
    {
        #region Constructor
        public ChatCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #endregion


        #region Methods
        public async Task<IChat> AddAsync(ChatOptions chatOptions)
        {
            if (chatOptions.Members == null || chatOptions.Members.Count == 0)
            {
                throw new ArgumentNullException(nameof(chatOptions.Members));
            }
            if (chatOptions.ChatType == ChatType.Meeting)
            {
                throw new InvalidOperationException("Meeting chat cannot be created");
            }

            var newChat = CreateNewAndAdd() as Chat;
            newChat.ChatType = chatOptions.ChatType.ToString();

            var aadUserConversationMembers = new AadUserConversationMemberCollection(newChat.PnPContext, newChat);

            foreach (var optionMember in chatOptions.Members)
            {
                aadUserConversationMembers.Add(new AadUserConversationMember()
                {
                    Roles = optionMember.Roles,
                    UserId = optionMember.UserId
                });
            }

            await newChat.PnPContext.Me.LoadAsync(y => y.Id).ConfigureAwait(false);

            // Add context user to chat
            if (aadUserConversationMembers.AsRequested().FirstOrDefault(y => y.UserId == newChat.PnPContext.Me.Id.ToString()) == null)
                aadUserConversationMembers.Add(new AadUserConversationMember
                {
                    Roles = new List<string> { "owner" },
                    UserId = newChat.PnPContext.Me.Id.ToString()
                });

            newChat.Members = aadUserConversationMembers;
            newChat.Topic = chatOptions.Topic;

            return await newChat.AddAsync().ConfigureAwait(false) as Chat;
        }

        public IChat Add(ChatOptions chatOptions)
        {
            return AddAsync(chatOptions).GetAwaiter().GetResult();
        }
        #endregion
    }
}
