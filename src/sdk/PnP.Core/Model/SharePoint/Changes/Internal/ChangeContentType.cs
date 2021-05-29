using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeContentType")]
    internal partial class ChangeContentType : Change, IChangeContentType
    {
        public IContentType ContentTypeId { get => GetValue<IContentType>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}