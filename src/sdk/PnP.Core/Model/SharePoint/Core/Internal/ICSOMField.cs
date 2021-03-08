using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint.Core.Internal
{
    internal interface ICSOMField
    {
        internal string ToCsomXml();
        internal abstract Guid CsomType { get; }
    }
}
