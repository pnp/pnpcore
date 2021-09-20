using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeList")]
    internal partial class ChangeList : Change, IChangeList
    {
        public string Editor { get => GetValue<string>(); set => SetValue(value); }
        
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }
        
        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }
        
        [SharePointProperty("BaseTemplate")]
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }
        
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

    }
}