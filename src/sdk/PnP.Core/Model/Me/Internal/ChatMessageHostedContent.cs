namespace PnP.Core.Model.Me
{
    [GraphType(Beta = true)]
    internal sealed class ChatMessageHostedContent : BaseDataModel<IChatMessageHostedContent>, IChatMessageHostedContent
    {

        #region Properties

        [GraphProperty("@microsoft.graph.temporaryId")]
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("contentType")]
        public string ContentType { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("contentBytes")]
        public string ContentBytes { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
