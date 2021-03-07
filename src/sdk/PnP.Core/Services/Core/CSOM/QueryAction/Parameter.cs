using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Core.Internal;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class Parameter
    {
        internal string Type { get; set; } = "String";
        internal string Name { get; set; }
        internal object Value { get; set; }
        internal string TypeId { get; set; }
        internal string ParameterTagName { get; set; } = "Parameter";
        internal virtual string SerializeParameter()
        {
            string stringValue = Value != null ? SerializeValue() : "";
            string type = Value != null ? Type : "Null";
            if (string.IsNullOrEmpty(TypeId))
            {
                if (Value is List<string>)
                {
                    //Multi-choice specific
                    string multiValue = string.Join("", (Value as List<string>).Select(value => $"<Object Type=\"String\">{TypeSpecificHandling(value, Type)}</Object>"));
                    return $"<{ParameterTagName} Type=\"Array\">{multiValue}</{ParameterTagName}>";
                }
                return $"<{ParameterTagName} Type=\"{type}\">{stringValue}</{ParameterTagName}>";
            }
            return $"<{ParameterTagName} TypeId=\"{{{TypeId}}}\">{stringValue}</{ParameterTagName}>";
        }
        internal virtual string SerializeValue()
        {
            return CsomHelper.XmlString(TypeSpecificHandling(Value.ToString(), Type), false);
        }

        protected static string TypeSpecificHandling(string value, string fieldType)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fieldType) && fieldType.Equals("Boolean"))
            {
                return value.ToLowerInvariant();
            }

            return value;
        }
    }

    internal class ObjectReferenceParameter : Parameter 
    {
        internal int ObjectPathId { get; set; }
        internal override string SerializeParameter()
        {
            return $"<Parameter ObjectPathId=\"{ObjectPathId}\" />";
        }
    }

    internal class SelectQuery
    {
        internal bool SelectAllProperties { get; set; }
        internal List<Property> Properties { get; set; }

        public override string ToString()
        {
            return $"<Query SelectAllProperties=\"{SelectAllProperties.ToString().ToLower()}\"><Properties /></Query>";
        }
    }
}
