using System;

namespace PnP.Core.Model.Security
{
    internal partial class GraphGroup : BaseDataModel<IGraphGroup>, IGraphGroup
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }
        
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl", Get = "groups/{GraphId}/sites/root/weburl", ExpandByDefault = true)]
        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }
        
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public bool MailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string MailNickname { get => GetValue<string>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public GroupVisibility Visibility { get => GetValue<GroupVisibility>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
