using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeFolder")]
    internal sealed class ChangeFolder : Change, IChangeFolder
    {
        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}