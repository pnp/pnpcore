using System;

namespace PnP.Core.Model.Me
{
    [GraphType(Uri = "me", LinqGet = "me")]
    internal sealed class Me : BaseDataModel<IMe>, IMe
    {
        #region Construction
        
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [GraphProperty("chats", Get = "me/chats")]
        public IChatCollection Chats { get => GetModelCollectionValue<IChatCollection>(); }
        #endregion
    }
}
