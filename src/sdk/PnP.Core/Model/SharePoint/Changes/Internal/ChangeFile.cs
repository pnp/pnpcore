using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeFile")]
    internal partial class ChangeFile : Change, IChangeFile
    {
        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}