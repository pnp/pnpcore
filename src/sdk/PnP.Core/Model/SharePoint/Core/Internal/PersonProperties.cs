using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.UserProfiles.PersonProperties", Uri = "")]
    internal class PersonProperties : TransientObject, IPersonProperties, IMetadataExtensible, IDataModelWithKey
    {
        public PersonProperties()
        {
        }

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

        public string AccountName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(AccountName))]
        public object Key { get => AccountName; set { AccountName = value.ToString(); } }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        public string Email { get => GetValue<string>(); set => SetValue(value); }
        public bool IsFollowed { get => GetValue<bool>(); set => SetValue(value); }
        public string LatestPost { get => GetValue<string>(); set => SetValue(value); }
        public string PersonalSiteHostUrl { get => GetValue<string>(); set => SetValue(value); }
        public string PersonalUrl { get => GetValue<string>(); set => SetValue(value); }
        public string PictureUrl { get => GetValue<string>(); set => SetValue(value); }
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        public string UserUrl { get => GetValue<string>(); set => SetValue(value); }
        public List<string> DirectReports { get => GetValue<List<string>>(); set => SetValue(value); }
        public List<string> ExtendedManagers { get => GetValue<List<string>>(); set => SetValue(value); }
        public List<string> ExtendedReports { get => GetValue<List<string>>(); set => SetValue(value); }
        public List<string> Peers { get => GetValue<List<string>>(); set => SetValue(value); }
        public Dictionary<string, object> UserProfileProperties { get => GetValue<Dictionary<string, object>>(); set => SetValue(value); }
    }
}
