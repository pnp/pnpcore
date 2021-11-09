using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeWeb")]
    internal sealed class ChangeWeb : Change, IChangeWeb
    {
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}