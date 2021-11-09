using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeField")]
    internal sealed class ChangeField : Change, IChangeField
    {
        public Guid FieldId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}