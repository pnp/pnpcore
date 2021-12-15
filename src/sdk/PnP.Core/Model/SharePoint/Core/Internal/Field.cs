using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
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
    internal sealed class Field : BaseDataModel<IField>, IField
    {
        internal const string FieldOptionsAdditionalInformationKey = "FieldOptions";

        #region Construction
        public Field()
        {
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

        public FieldIndexStatus IndexStatus { get => GetValue<FieldIndexStatus>(); set => SetValue(value); }

        public string InternalName { get => GetValue<string>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnlyField { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        public ShowInFiltersPaneStatus ShowInFiltersPane { get => GetValue<ShowInFiltersPaneStatus>(); set => SetValue(value); }

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

        public ChoiceFormatType EditFormat { get => GetValue<ChoiceFormatType>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        public double MaximumValue { get => GetValue<double>(); set => SetValue(value); }

        public double MinimumValue { get => GetValue<double>(); set => SetValue(value); }

        public string Formula { get => GetValue<string>(); set => SetValue(value); }

        public FieldType OutputType { get => GetValue<FieldType>(); set => SetValue(value); }

        public bool FillInChoice { get => GetValue<bool>(); set => SetValue(value); }

        public string Mappings { get => GetValue<string>(); set => SetValue(value); }

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

        public Guid AnchorId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool CreateValuesInEditForm { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsKeyword { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsPathRendered { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsAnchorValid { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsTermSetValid { get => GetValue<bool>(); set => SetValue(value); }

        public bool Open { get => GetValue<bool>(); set => SetValue(value); }

        public Guid SspId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid TermSetId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid TextField { get => GetValue<Guid>(); set => SetValue(value); }

        public bool UserCreated { get => GetValue<bool>(); set => SetValue(value); }

        public string TargetTemplate { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Extension methods

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

            return new FieldUrlValue()
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

            return new FieldLookupValue()
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

            return new FieldUserValue()
            {
                LookupId = userId,
                Field = this
            };
        }

        public IFieldUserValue NewFieldUserValue(ISharePointPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return new FieldUserValue()
            {
                Principal = principal,
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

            return new FieldTaxonomyValue()
            {
                TermId = termId,
                Label = label,
                WssId = wssId,
                Field = this
            };
        }

        public IFieldValueCollection NewFieldValueCollection()
        {
            return new FieldValueCollection(this, InternalName);
        }

        public IFieldValueCollection NewFieldValueCollection(IEnumerable<IFieldValue> fieldValues)
        {
            var fieldValueCollection = NewFieldValueCollection();

            foreach (var value in fieldValues)
            {
                fieldValueCollection.Values.Add(value);
            }

            return fieldValueCollection;
        }

        public IFieldValueCollection NewFieldValueCollection(IEnumerable<KeyValuePair<Guid, string>> fieldValues)
        {
            var fieldValueCollection = NewFieldValueCollection();

            foreach (var keyValuePair in fieldValues)
            {
                fieldValueCollection.Values.Add(NewFieldTaxonomyValue(keyValuePair.Key, keyValuePair.Value));
            }

            return fieldValueCollection;
        }
        #endregion

        #endregion
    }
}
