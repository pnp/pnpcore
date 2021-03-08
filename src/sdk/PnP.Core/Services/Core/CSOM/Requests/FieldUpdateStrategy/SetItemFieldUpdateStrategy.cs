using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests.FieldUpdateStrategy
{
    internal class SetItemFieldUpdateStrategy : IFieldUpdateStrategy
    {
        internal IIdProvider IdProvider { get; private set; }
        internal SetItemFieldUpdateStrategy(IIdProvider idProvider)
        {
            IdProvider = idProvider;
        }
        List<ActionObjectPath> IFieldUpdateStrategy.GetFieldUpdateAction(CSOMItemField fld, Identity identity)
        {
            MethodAction action = new MethodAction()
            {
                Id = IdProvider.GetActionId(),
                Name = "SetFieldValue",
                ObjectPathId = identity.Id.ToString(),
                Parameters = fld.GetRequestParameters()
            };

            return new List<ActionObjectPath>()
            {
                new ActionObjectPath()
                {
                    Action = action
                }
            };
        }
    }
}
