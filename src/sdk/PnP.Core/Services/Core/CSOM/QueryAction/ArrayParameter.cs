using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class ArrayParameter : Parameter
    {
        IFieldValueCollection FieldValueCollection { get; set; }

        internal ArrayParameter(IFieldValueCollection fieldValueCollection)
        {
            FieldValueCollection = fieldValueCollection;
        }

        internal override string SerializeParameter()
        {
            List<Parameter> childParameter = new List<Parameter>();
            foreach (var fld in FieldValueCollection.Values)
            {
                if (fld is ICSOMField)
                {
                    childParameter.Add(new CSOMFieldParameter(fld as ICSOMField)
                    {
                        Type = Type,
                        Name = Name,
                        Value = fld,
                        ParameterTagName = "Object"
                    });
                }
                else
                {
                    childParameter.Add(new Parameter()
                    {
                        Type = Type,
                        Value = fld,
                        ParameterTagName = "Object"
                    });
                }
            }
            return $"<Parameter Type=\"Array\">{string.Join("", childParameter.Select(child => child.SerializeParameter()))}</Parameter>";
        }
    }
}
