using System;

namespace PnP.Core.Model.SharePoint.Core.Internal
{
    internal interface ICSOMField
    {
        internal string ToCsomXml();
        internal abstract Guid CsomType { get; }
    }
}
