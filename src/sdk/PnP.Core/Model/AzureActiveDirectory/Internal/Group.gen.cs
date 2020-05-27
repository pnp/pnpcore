using System;

namespace PnP.Core.Model.AzureActiveDirectory
{
    internal partial class Group : BaseDataModel<IGroup>, IGroup
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }
        
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl", Get = "groups/{GraphId}/sites/root/weburl", ExpandByDefault = true)]
        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }
        
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
