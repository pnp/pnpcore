using PnP.Core.Services.Core.CSOM.QueryIdentities;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                else if (Value is DateTime valueAsDateTime)
                {
                    return $"<{ParameterTagName} Type=\"{type}\">{valueAsDateTime.ToUniversalTime():yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz}</{ParameterTagName}>";
                }
                else if (Value is double valueAsDouble)
                {
                    return $"<{ParameterTagName} Type=\"{type}\">{valueAsDouble.ToString("G", CultureInfo.InvariantCulture)}</{ParameterTagName}>";
                }
                else if (Value is bool valueAsBool)
                {
                    return $"<{ParameterTagName} Type=\"{type}\">{valueAsBool.ToString().ToLowerInvariant()}</{ParameterTagName}>";
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
