using PnP.Core.Services;
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
    internal partial class Field : BaseDataModel<IField>, IField
    {
        internal const string FieldOptionsAdditionalInformationKey = "FieldOptions";

        #region Construction
        public Field()
        {
            // Handler to construct the Add request for this list
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var fieldOptions = (FieldOptions)additionalInformation[FieldOptionsAdditionalInformationKey];

                // Given this method can apply on both Web.Fields as List.Fields we're getting the entity info which will 
                // automatically provide the correct 'parent'
                // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/lists(id) )
                var entity = EntityManager.GetClassInfo(GetType(), this);

                string endpointUrl = entity.SharePointGet;
                dynamic addParameters = null;
                if (FieldTypeKind == FieldType.Lookup)
                {
                    if (!(fieldOptions is FieldLookupOptions fieldLookupOptions))
                    {
                        throw new ClientException(ErrorType.InvalidParameters,
                            PnPCoreResources.Exception_Invalid_LookupFields);
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
        #endregion

        #region Properties
        public bool AutoIndexed { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanBeDeleted { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ClientSideComponentId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClientSideComponentProperties { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public string CustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultFormula { get => GetValue<string>(); set => SetValue(value); }

        public object DefaultValue { get => GetValue<object>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string Direction { get => GetValue<string>(); set => SetValue(value); }

        public bool EnforceUniqueValues { get => GetValue<bool>(); set => SetValue(value); }

        public string EntityPropertyName { get => GetValue<string>(); set => SetValue(value); }

        public bool Filterable { get => GetValue<bool>(); set => SetValue(value); }

        public bool FromBaseType { get => GetValue<bool>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public bool Indexed { get => GetValue<bool>(); set => SetValue(value); }

        public int IndexStatus { get => GetValue<int>(); set => SetValue(value); }

        public string InternalName { get => GetValue<string>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public bool PinnedToFiltersPane { get => GetValue<bool>(); set => SetValue(value); }

        public bool ReadOnlyField { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        public int ShowInFiltersPane { get => GetValue<int>(); set => SetValue(value); }

        public bool Sortable { get => GetValue<bool>(); set => SetValue(value); }

        public string StaticName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public FieldType FieldTypeKind { get => GetValue<FieldType>(); set => SetValue(value); }

        public string TypeAsString { get => GetValue<string>(); set => SetValue(value); }

        public string TypeDisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string TypeShortDescription { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public int MaxLength { get => GetValue<int>(); set => SetValue(value); }

        public int CurrencyLocaleId { get => GetValue<int>(); set => SetValue(value); }

        public DateTimeFieldFormatType DateFormat { get => GetValue<DateTimeFieldFormatType>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public int EditFormat { get => GetValue<int>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        public double MaximumValue { get => GetValue<double>(); set => SetValue(value); }

        public double MinimumValue { get => GetValue<double>(); set => SetValue(value); }

        public string Formula { get => GetValue<string>(); set => SetValue(value); }

        public FieldType OutputType { get => GetValue<FieldType>(); set => SetValue(value); }

        public bool FillInChoice { get => GetValue<bool>(); set => SetValue(value); }

        public string Mappings { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Choices", JsonPath = "results")]
        public string[] Choices { get => GetValue<string[]>(); set => SetValue(value); }

        public CalendarType DateTimeCalendarType { get => GetValue<CalendarType>(); set => SetValue(value); }

        public DateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get => GetValue<DateTimeFieldFriendlyFormatType>(); set => SetValue(value); }

        public bool AllowDisplay { get => GetValue<bool>(); set => SetValue(value); }

        public bool Presence { get => GetValue<bool>(); set => SetValue(value); }

        public int SelectionGroup { get => GetValue<int>(); set => SetValue(value); }

        public FieldUserSelectionMode SelectionMode { get => GetValue<FieldUserSelectionMode>(); set => SetValue(value); }

        public bool AllowHyperlink { get => GetValue<bool>(); set => SetValue(value); }

        public bool AppendOnly { get => GetValue<bool>(); set => SetValue(value); }

        public int NumberOfLines { get => GetValue<int>(); set => SetValue(value); }

        public bool RestrictedMode { get => GetValue<bool>(); set => SetValue(value); }

        public bool RichText { get => GetValue<bool>(); set => SetValue(value); }

        public bool UnlimitedLengthInDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableLookup { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowMultipleValues { get => GetValue<bool>(); set => SetValue(value); }

        public string[] DependentLookupInternalNames { get => GetValue<string[]>(); set => SetValue(value); }

        public bool IsDependentLookup { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRelationship { get => GetValue<bool>(); set => SetValue(value); }

        public string LookupField { get => GetValue<string>(); set => SetValue(value); }

        public string LookupList { get => GetValue<string>(); set => SetValue(value); }

        public Guid LookupWebId { get => GetValue<Guid>(); set => SetValue(value); }

        public string PrimaryFieldId { get => GetValue<string>(); set => SetValue(value); }

        public RelationshipDeleteBehaviorType RelationshipDeleteBehavior { get => GetValue<RelationshipDeleteBehaviorType>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion

        #region Extension methods
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
            EntityInfo entity = EntityManager.GetClassInfo(typeof(Field), this);
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
            EntityInfo entity = EntityManager.GetClassInfo(typeof(Field), this);
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

        #region FieldValue object creation
        public IFieldUrlValue NewFieldUrlValue(string url, string description = null)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return new FieldUrlValue(InternalName, null)
            {
                Url = url,
                Description = description ?? url,
                Field = this
            };
        }

        public IFieldLookupValue NewFieldLookupValue(int lookupId)
        {
            if (lookupId < -1)
            {
                throw new ArgumentNullException(nameof(lookupId));
            }

            return new FieldLookupValue(InternalName, null)
            {
                LookupId = lookupId,
                Field = this
            };
        }

        public IFieldUserValue NewFieldUserValue(int userId)
        {
            if (userId < -1)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return new FieldUserValue(InternalName, null)
            {
                LookupId = userId,
                Field = this
            };
        }

        public IFieldTaxonomyValue NewFieldTaxonomyValue(Guid termId, string label, int wssId = -1)
        {
            if (termId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(termId));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            return new FieldTaxonomyValue(InternalName, null)
            {
                TermId = termId,
                Label = label,
                WssId = wssId,
                Field = this
            };
        }

        public IFieldValueCollection NewFieldValueCollection(TransientDictionary parent)
        {
            return new FieldValueCollection(this, InternalName, parent);
        }

        #endregion

        #endregion
    }
}
