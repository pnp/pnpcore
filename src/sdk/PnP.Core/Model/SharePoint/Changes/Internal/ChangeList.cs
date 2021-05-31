using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeList")]
    internal partial class ChangeList : Change, IChangeList
    {
        public string Editor { get => GetValue<string>(); set => SetValue(value); }
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }
        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }
        public string RootFolderUrl { get => GetValue<string>(); set => SetValue(value); }
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        // This is a navigation property, and the __deferred url it gives us is awkward
        public ISharePointUser Creator { get => GetValue<ISharePointUser>(); set => SetValue(value); }
    }
}