using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class ContentTypeCreationParameter : Parameter
    {
        internal new ContentTypeCreationInfo Value { get; set; }
        internal ContentTypeCreationParameter()
        {
            TypeId = "{168f3091-4554-4f14-8866-b20d48e45b54}";
        }
        internal override string SerializeParameter()
        {
            NamedProperty descriptionProperty = new NamedProperty()
            {
                Name = "Description",
                Type = "String",
                Value = Value.Description
            };
            NamedProperty groupProperty = new NamedProperty()
            {
                Name = "Group",
                Type = "String",
                Value = Value.Group
            };
            NamedProperty idProperty = new NamedProperty()
            {
                Name = "Id",
                Type = "String",
                Value = Value.Id
            };
            NamedProperty nameProperty = new NamedProperty()
            {
                Name = "Name",
                Type = "String",
                Value = Value.Name
            };
            NamedProperty parentProperty = new NamedProperty()
            {
                Name = "ParentContentType",
                Type = "String",
                Value = Value.ParentContentType
            };
            return $"<{ParameterTagName} TypeId=\"{TypeId}\">{descriptionProperty.ToString() + groupProperty.ToString() + idProperty.ToString() + nameProperty.ToString() + parentProperty.ToString()}</{ParameterTagName}>";
        }
    }
}
