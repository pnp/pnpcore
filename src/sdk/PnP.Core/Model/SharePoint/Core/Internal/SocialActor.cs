using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.Social.SocialActor", Uri = "")]
    internal class SocialActor : TransientObject, ISocialActor, IDataModelWithKey, IMetadataExtensible
    {
        public string AccountName { get => GetValue<string>(); set => SetValue(value); }
        public SocialActorType ActorType { get => GetValue<SocialActorType>(); set => SetValue(value); }
        public bool CanFollow { get => GetValue<bool>(); set => SetValue(value); }
        public string ContentUri { get => GetValue<string>(); set => SetValue(value); }
        public string EmailAddress { get => GetValue<string>(); set => SetValue(value); }
        public string FollowedContentUri { get => GetValue<string>(); set => SetValue(value); }
        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }
        public string Id { get => GetValue<string>(); set => SetValue(value); }
        public string ImageUri { get => GetValue<string>(); set => SetValue(value); }
        public bool IsFollowed { get => GetValue<bool>(); set => SetValue(value); }
        public string LibraryUri { get => GetValue<string>(); set => SetValue(value); }
        public string Name { get => GetValue<string>(); set => SetValue(value); }
        public string PersonalSiteUri { get => GetValue<string>(); set => SetValue(value); }
        public SocialStatusCode Status { get => GetValue<SocialStatusCode>(); set => SetValue(value); }
        public string StatusText { get => GetValue<string>(); set => SetValue(value); }
        public Guid TagGuid { get => GetValue<Guid>(); set => SetValue(value); }
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        public string Uri { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public object Key { get => Id; set => Id = value.ToString(); }

        [SystemProperty]
        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        Task IMetadataExtensible.SetGraphToRestMetadataAsync()
        {
            return Task.CompletedTask;
        }

        Task IMetadataExtensible.SetRestToGraphMetadataAsync()
        {
            return Task.CompletedTask;
        }
    }
}
