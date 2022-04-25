using System;

namespace PnP.Core.Model.Me
{
    [GraphType(Beta = true)]
    internal sealed class ChatMessageAttachment : BaseDataModel<IChatMessageAttachment>, IChatMessageAttachment
    {

        #region Properties

        [GraphProperty("id")]
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("contentType")]
        public string ContentType { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("contentUrl")]
        public Uri ContentUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [GraphProperty("content")]
        public string Content { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("name")]
        public string Name { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("thumbnailUrl")]
        public Uri ThumbnailUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
