using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint.Core.Internal;
using PnP.Core.Services;
using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Field class, write your custom code here
    /// </summary>
    [SharePointType("SP.ContentType", Target = typeof(Web), Uri = "_api/Web/Fields('{Id}')", Get = "_api/Web/Fields", LinqGet = "_api/Web/Fields", ResolveUriFromMetadataFor = ResolveUriFromMetadataFor.None)]
    [SharePointType("SP.ContentType", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/Fields('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/Fields", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/Fields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Field
    {
        /// <summary>
        /// Class to model the Rest Field Add request
        /// </summary>
        internal class FieldAdd : RestBaseAdd<IField>
        {
            public FieldType FieldTypeKind { get; private set; }
            public string Title { get; set; }
            public string InternalName { get; set; }
            public string Description { get; set; }
            public string DefaultFormula { get; set; }
            public bool EnforceUniqueValues { get; set; }
            public string Group { get; set; }
            public bool Hidden { get; set; }
            public bool Indexed { get; set; }
            public bool Required { get; set; }
            public string ValidationFormula { get; set; }
            public string ValidationMessage { get; set; }

            internal FieldAdd(BaseDataModel<IField> field, FieldType fieldType)
                : base(field, SharePointFieldTypeMapping.GetEntityTypeFromFieldType(fieldType))
            {
                FieldTypeKind = fieldType;
            }
        }

        public Field()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case nameof(FieldType): return JsonMappingHelper.ToEnum<FieldType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this list
            AddApiCallHandler = (keyValuePairs) =>
            {
                // Given this method can apply on both Web.Fields as List.Fields we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.Instance.GetClassInfo<IField>(GetType(), this);

                // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/lists(id) )
                var addParameters = GetFIeldAddRequestParameters();
                return new ApiCall(entity.SharePointGet, ApiType.SPORest, JsonSerializer.Serialize(addParameters, new JsonSerializerOptions()
                {
                    IgnoreNullValues = true
                }));
            };
        }

        private FieldAdd GetFIeldAddRequestParameters()
        {
            var fieldAddParameters = new FieldAdd(this, FieldTypeKind)
            {
                Title = Title,
                InternalName = HasValue(nameof(InternalName)) ? InternalName : null,
                DefaultFormula = HasValue(nameof(DefaultFormula)) ? DefaultFormula : default,
                Description = HasValue(nameof(Description)) ? Description : default,
                EnforceUniqueValues = HasValue(nameof(EnforceUniqueValues)) && EnforceUniqueValues,
                Group = HasValue(nameof(Group)) ? Group : default,
                Hidden = HasValue(nameof(Hidden)) && Hidden,
                Required = HasValue(nameof(Required)) && Required,
                Indexed = HasValue(nameof(Indexed)) && Indexed,
                ValidationFormula = HasValue(nameof(ValidationFormula)) ? ValidationFormula : default,
                ValidationMessage = HasValue(nameof(ValidationMessage)) ? ValidationMessage : default
            };

            // TODO Map field specific options

            return fieldAddParameters;
        }

        // TODO Implement extension methods here

    }
}
