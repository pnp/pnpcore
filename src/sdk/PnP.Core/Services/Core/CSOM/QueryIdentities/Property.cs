using PnP.Core.Services.Core.CSOM.QueryAction;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    internal class Property : Identity
    {
        internal int ParentId { get; set; }

        public override string ToString()
        {
            return $"<Property Id=\"{Id}\" ParentId=\"{ParentId}\" Name=\"{Name}\" />";
        }
    }

    internal sealed class QueryProperty: Property
    {
        internal bool SelectAll { get; set; }

        internal SelectQuery Query { get; set; }

        public override string ToString()
        {
            return $"<Property Name=\"{Name}\" SelectAll=\"{SelectAll.ToString().ToLower()}\">{Query}<Property>";
        }
    }

    internal sealed class StaticProperty : Identity
    {
        internal string TypeId { get; set; }

        public override string ToString()
        {
            return $"<StaticProperty Id=\"{Id}\" TypeId=\"{TypeId}\" Name=\"{Name}\" />";
        }
    }

    internal sealed class NamedProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            string stringValue = Value != null ? CsomHelper.XmlString(Parameter.TypeSpecificHandling(Value.ToString(), Type), false) : "";
            _ = Value != null ? Type : "Null";

            if (Value == null)
            {
                return $"<Property Name=\"{Name}\" Type=\"Null\" />";
            }
            return $"<Property Name=\"{Name}\" Type=\"{Type}\">{stringValue}</Property>";
        }
    }

    internal sealed class IdentityProperty
    {
        internal string Name { get; set; }

        internal string ObjectPathId { get; set; }

        public override string ToString()
        {
            return $"<Property Name=\"{Name}\" ObjectPathId=\"{ObjectPathId}\" />";
        }
    }

}
