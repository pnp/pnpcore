using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeView")]
    internal sealed class ChangeView : Change, IChangeView
    {
        public Guid ViewId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}