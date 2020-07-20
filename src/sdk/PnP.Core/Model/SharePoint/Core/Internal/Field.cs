using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint.Core.Internal;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Field class, write your custom code here
    /// </summary>
    [SharePointType("SP.Field", Target = typeof(Web), Uri = "_api/Web/Fields('{Id}')", Get = "_api/Web/Fields", LinqGet = "_api/Web/Fields")]
    [SharePointType("SP.Field", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/Fields('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/Fields", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/Fields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Field
    {
        internal const string FieldOptionsAdditionalInformationKey = "FieldOptions";

        public Field()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case nameof(FieldType): return JsonMappingHelper.ToEnum<FieldType>(input.JsonElement);
                    case nameof(CalendarType): return JsonMappingHelper.ToEnum<CalendarType>(input.JsonElement);
                    case nameof(ChoiceFormatType): return JsonMappingHelper.ToEnum<ChoiceFormatType>(input.JsonElement);
                    case nameof(DateTimeFieldFormatType): return JsonMappingHelper.ToEnum<DateTimeFieldFormatType>(input.JsonElement);
                    case nameof(DateTimeFieldFriendlyFormatType): return JsonMappingHelper.ToEnum<DateTimeFieldFriendlyFormatType>(input.JsonElement);
                    case nameof(FieldUserSelectionMode): return JsonMappingHelper.ToEnum<FieldUserSelectionMode>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this list
            AddApiCallHandler = (additionalInformation) =>
            {
                var fieldOptions = (FieldOptions)additionalInformation[FieldOptionsAdditionalInformationKey];

                // Given this method can apply on both Web.Fields as List.Fields we're getting the entity info which will 
                // automatically provide the correct 'parent'
                // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/lists(id) )
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

                string endpointUrl = entity.SharePointGet;
                dynamic addParameters = null;
                if (FieldTypeKind == FieldType.Lookup)
                {
                    if (!(fieldOptions is FieldLookupOptions fieldLookupOptions))
                    {
                        throw new ClientException(ErrorType.InvalidParameters, "Specified field parameters are not valid for lookup type fields");
                    }

                    endpointUrl += "/AddField";
                    addParameters = GetFieldLookupAddParameters(fieldLookupOptions);
                }
                else
                {
                    addParameters = GetFieldGenericAddParameters(fieldOptions);
                }

                // To handle the serialization of string collections
                var serializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true };
                serializerOptions.Converters.Add(new SharePointRestCollectionJsonConverter<string>());

                string json = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject), serializerOptions);
                return new ApiCall(endpointUrl, ApiType.SPORest, json);
            };
        }

        private dynamic GetFieldGenericAddParameters(FieldOptions fieldOptions)
        {
            ExpandoObject baseAddPayload = new
            {
                __metadata = new { type = SharePointFieldType.GetEntityTypeFromFieldType(FieldTypeKind) },
                FieldTypeKind,
                Title
            }.AsExpando();

            // Merge the base add payload with options if any
            dynamic fieldAddParameters = fieldOptions != null
                ? fieldOptions.AsExpando(ignoreNullValues: true).MergeWith(baseAddPayload)
                : baseAddPayload;

            return fieldAddParameters;
        }

        private dynamic GetFieldLookupAddParameters(FieldLookupOptions fieldOptions)
        {
            ExpandoObject baseAddPayload = new
            {
                __metadata = new { type = SharePointFieldType.FieldCreationInformation },
                FieldTypeKind,
                Title
            }.AsExpando();

            // Merge the base add payload with options if any
            dynamic parameters = fieldOptions != null
                ? fieldOptions.AsExpando().MergeWith(baseAddPayload)
                : baseAddPayload;

            return new { parameters }.AsExpando();
        }

        internal async Task<IField> AddAsXmlBatchAsync(Batch batch, string schemaXml, AddFieldOptionsFlags options)
        {
            // Given this method can apply on both Web.Fields as List.Fields we're getting the entity info which will 
            // automatically provide the correct 'parent'
            // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/lists(id) )
            EntityInfo entity = EntityManager.Instance.GetClassInfo(typeof(Field), this);
            string endpointUrl = $"{entity.SharePointGet}/CreateFieldAsXml";

            var body = new
            {
                parameters = new
                {
                    SchemaXml = schemaXml,
                    Options = (int)options
                }
            }.AsExpando();

            string json = JsonSerializer.Serialize(body, typeof(ExpandoObject));
            var apiCall = new ApiCall(endpointUrl, ApiType.SPORest, json);

            await RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return this;
        }

        internal async Task<IField> AddAsXmlAsync(string schemaXml, AddFieldOptionsFlags options)
        {
            // Given this method can apply on both Web.Fields as List.Fields we're getting the entity info which will 
            // automatically provide the correct 'parent'
            // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/lists(id) )
            EntityInfo entity = EntityManager.Instance.GetClassInfo(typeof(Field), this);
            string endpointUrl = $"{entity.SharePointGet}/CreateFieldAsXml";

            var body = new
            {
                parameters = new
                {
                    SchemaXml = schemaXml,
                    Options = (int)options
                }
            }.AsExpando();

            string json = JsonSerializer.Serialize(body, typeof(ExpandoObject));
            var apiCall = new ApiCall(endpointUrl, ApiType.SPORest, json);

            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return this;
        }
    }
}
