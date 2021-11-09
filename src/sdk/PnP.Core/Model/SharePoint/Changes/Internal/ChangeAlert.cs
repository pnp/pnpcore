using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeAlert")]
    internal sealed class ChangeAlert : Change, IChangeAlert
    {
        public Guid AlertId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}