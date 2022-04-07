namespace PnP.Core.Model.Me
{
    [GraphType]
    internal sealed class ChatMessageMention : BaseDataModel<IChatMessageMention>, IChatMessageMention
    {
        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string MentionText { get => GetValue<string>(); set => SetValue(value); }

        public IChatIdentitySet Mentioned { get => GetModelValue<IChatIdentitySet>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }
        #endregion
    }
}
