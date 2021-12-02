using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.Social.SocialRestFollowingManager", Uri = "")]
    internal class FollowingInfo : TransientObject, IMetadataExtensible, IFollowingInfo, IDataModelWithKey
    {
        public string MyFollowedDocumentsUri { get; set; }
        public string MyFollowedSitesUri { get; set; }
        public ISocialActor SocialActor { get; set; }

        public Guid Id { get; set; } = Guid.Empty;

        [SystemProperty]
        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        [KeyProperty(nameof(Id))]
        public object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

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
