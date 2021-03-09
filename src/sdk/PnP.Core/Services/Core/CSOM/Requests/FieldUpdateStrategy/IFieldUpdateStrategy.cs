using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.FieldUpdateStrategy
{
    internal interface IFieldUpdateStrategy
    {
        internal List<ActionObjectPath> GetFieldUpdateAction(CSOMItemField fld, Identity identity);
    }
}
