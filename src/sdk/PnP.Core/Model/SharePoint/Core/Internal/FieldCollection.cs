using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection : QueryableDataModelCollection<IField>, IFieldCollection
    {
        public FieldCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public async Task<IField> AddBatchAsync(string title, FieldType fieldType, FieldOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, title, fieldType, options).ConfigureAwait(false);
        }

        public IField AddBatch(string title, FieldType fieldType, FieldOptions options)
        {
            return AddBatchAsync(title, fieldType, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddBatchAsync(Batch batch, string title, FieldType fieldType, FieldOptions options)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (fieldType == FieldType.Invalid)
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_Invalid_FieldType, nameof(fieldType)));

            if (!ValidateFieldOptions(fieldType, options))
                throw new ClientException(ErrorType.InvalidParameters,
                    string.Format(PnPCoreResources.Exception_Invalid_ForFieldType, nameof(options), fieldType));

            var newField = CreateNewAndAdd() as Field;

            newField.Title = title;
            newField.FieldTypeKind = fieldType;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { Field.FieldOptionsAdditionalInformationKey, options }
            };

            return await newField.AddBatchAsync(batch, additionalInfo).ConfigureAwait(false) as Field;
        }

        public IField AddBatch(Batch batch, string title, FieldType fieldType, FieldOptions options)
        {
            return AddBatchAsync(batch, title, fieldType, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCalculatedBatchAsync(string title, FieldCalculatedOptions options = null)
        {
            return await AddCalculatedBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddCalculatedBatch(string title, FieldCalculatedOptions options = null)
        {
            return AddCalculatedBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCalculatedBatchAsync(Batch batch, string title, FieldCalculatedOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Calculated, options).ConfigureAwait(false);
        }

        public IField AddCalculatedBatch(Batch batch, string title, FieldCalculatedOptions options = null)
        {
            return AddCalculatedBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceBatchAsync(string title, FieldChoiceOptions options = null)
        {
            return await AddChoiceBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddChoiceBatch(string title, FieldChoiceOptions options = null)
        {
            return AddChoiceBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceBatchAsync(Batch batch, string title, FieldChoiceOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Choice, options).ConfigureAwait(false);
        }

        public IField AddChoiceBatch(Batch batch, string title, FieldChoiceOptions options = null)
        {
            return AddChoiceBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCurrencyBatchAsync(string title, FieldCurrencyOptions options = null)
        {
            return await AddCurrencyBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddCurrencyBatch(string title, FieldCurrencyOptions options = null)
        {
            return AddCurrencyBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCurrencyBatchAsync(Batch batch, string title, FieldCurrencyOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Currency, options).ConfigureAwait(false);
        }

        public IField AddCurrencyBatch(Batch batch, string title, FieldCurrencyOptions options = null)
        {
            return AddCurrencyBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddDateTimeBatchAsync(string title, FieldDateTimeOptions options = null)
        {
            return await AddDateTimeBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddDateTimeBatch(string title, FieldDateTimeOptions options = null)
        {
            return AddDateTimeBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddDateTimeBatchAsync(Batch batch, string title, FieldDateTimeOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.DateTime, options).ConfigureAwait(false);
        }

        public IField AddDateTimeBatch(Batch batch, string title, FieldDateTimeOptions options = null)
        {
            return AddDateTimeBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupBatchAsync(string title, FieldLookupOptions options = null)
        {
            return await AddLookupBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddLookupBatch(string title, FieldLookupOptions options = null)
        {
            return AddLookupBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupBatchAsync(Batch batch, string title, FieldLookupOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Lookup, options).ConfigureAwait(false);
        }

        public IField AddLookupBatch(Batch batch, string title, FieldLookupOptions options = null)
        {
            return AddLookupBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultiChoiceBatchAsync(string title, FieldMultiChoiceOptions options = null)
        {
            return await AddMultiChoiceBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddMultiChoiceBatch(string title, FieldMultiChoiceOptions options = null)
        {
            return AddMultiChoiceBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultiChoiceBatchAsync(Batch batch, string title, FieldMultiChoiceOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.MultiChoice, options).ConfigureAwait(false);
        }

        public IField AddMultiChoiceBatch(Batch batch, string title, FieldMultiChoiceOptions options = null)
        {
            return AddMultiChoiceBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultilineTextBatchAsync(string title, FieldMultilineTextOptions options = null)
        {
            return await AddMultilineTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddMultilineTextBatch(string title, FieldMultilineTextOptions options = null)
        {
            return AddMultilineTextBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultilineTextBatchAsync(Batch batch, string title, FieldMultilineTextOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Note, options).ConfigureAwait(false);
        }

        public IField AddMultilineTextBatch(Batch batch, string title, FieldMultilineTextOptions options = null)
        {
            return AddMultilineTextBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddNumberBatchAsync(string title, FieldNumberOptions options = null)
        {
            return await AddNumberBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddNumberBatch(string title, FieldNumberOptions options = null)
        {
            return AddNumberBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddNumberBatchAsync(Batch batch, string title, FieldNumberOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Number, options).ConfigureAwait(false);
        }

        public IField AddNumberBatch(Batch batch, string title, FieldNumberOptions options = null)
        {
            return AddNumberBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTextBatchAsync(string title, FieldTextOptions options = null)
        {
            return await AddTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddTextBatch(string title, FieldTextOptions options = null)
        {
            return AddTextBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTextBatchAsync(Batch batch, string title, FieldTextOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Text, options).ConfigureAwait(false);
        }

        public IField AddTextBatch(Batch batch, string title, FieldTextOptions options = null)
        {
            return AddTextBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUrlBatchAsync(string title, FieldUrlOptions options = null)
        {
            return await AddUrlBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddUrlBatch(string title, FieldUrlOptions options = null)
        {
            return AddUrlBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUrlBatchAsync(Batch batch, string title, FieldUrlOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.URL, options).ConfigureAwait(false);
        }

        public IField AddUrlBatch(Batch batch, string title, FieldUrlOptions options = null)
        {
            return AddUrlBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserBatchAsync(string title, FieldUserOptions options = null)
        {
            return await AddUserBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddUserBatch(string title, FieldUserOptions options = null)
        {
            return AddUserBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserBatchAsync(Batch batch, string title, FieldUserOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.User, options).ConfigureAwait(false);
        }

        public IField AddUserBatch(Batch batch, string title, FieldUserOptions options = null)
        {
            return AddUserBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddAsync(string title, FieldType fieldType, FieldOptions options)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (fieldType == FieldType.Invalid)
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_Invalid_FieldType, nameof(fieldType)));

            if (!ValidateFieldOptions(fieldType, options))
                throw new ClientException(ErrorType.InvalidParameters,
                    string.Format(PnPCoreResources.Exception_Invalid_ForFieldType, nameof(options), fieldType));

            var newField = CreateNewAndAdd() as Field;

            newField.Title = title;
            newField.FieldTypeKind = fieldType;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { Field.FieldOptionsAdditionalInformationKey, options }
            };

            return await newField.AddAsync(additionalInfo).ConfigureAwait(false) as Field;
        }

        public IField Add(string title, FieldType fieldType, FieldOptions options)
        {
            return AddAsync(title, fieldType, options).GetAwaiter().GetResult();
        }

        private static bool ValidateFieldOptions(FieldType fieldType, FieldOptions fieldOptions)
        {
            if (fieldOptions == null)
                return true;

            switch (fieldType)
            {
                case FieldType.Text:
                    return fieldOptions is FieldTextOptions;
                case FieldType.Note:
                    return fieldOptions is FieldMultilineTextOptions;
                case FieldType.DateTime:
                    return fieldOptions is FieldDateTimeOptions;
                case FieldType.Choice:
                    return fieldOptions is FieldChoiceOptions;
                case FieldType.MultiChoice:
                    return fieldOptions is FieldMultiChoiceOptions;
                case FieldType.Lookup:
                    return fieldOptions is FieldLookupOptions;
                case FieldType.Number:
                    return fieldOptions is FieldNumberOptions;
                case FieldType.Currency:
                    return fieldOptions is FieldCurrencyOptions;
                case FieldType.URL:
                    return fieldOptions is FieldUrlOptions;
                case FieldType.Calculated:
                    return fieldOptions is FieldCalculatedOptions;
                case FieldType.User:
                    return fieldOptions is FieldUserOptions;
                //case FieldType.GridChoice:
                //return fieldOptions is FieldGeo;
                default:
                    return false;
            }
        }

        public async Task<IField> AddCalculatedAsync(string title, FieldCalculatedOptions options = null)
        {
            return await AddAsync(title, FieldType.Calculated, options).ConfigureAwait(false) as Field;
        }

        public IField AddCalculated(string title, FieldCalculatedOptions options = null)
        {
            return AddCalculatedAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceAsync(string title, FieldChoiceOptions options = null)
        {
            return await AddAsync(title, FieldType.Choice, options).ConfigureAwait(false) as Field;
        }

        public IField AddChoice(string title, FieldChoiceOptions options = null)
        {
            return AddChoiceAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCurrencyAsync(string title, FieldCurrencyOptions options = null)
        {
            return await AddAsync(title, FieldType.Currency, options).ConfigureAwait(false) as Field;
        }

        public IField AddCurrency(string title, FieldCurrencyOptions options = null)
        {
            return AddCurrencyAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddDateTimeAsync(string title, FieldDateTimeOptions options = null)
        {
            return await AddAsync(title, FieldType.DateTime, options).ConfigureAwait(false) as Field;
        }

        public IField AddDateTime(string title, FieldDateTimeOptions options = null)
        {
            return AddDateTimeAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupAsync(string title, FieldLookupOptions options = null)
        {
            return await AddAsync(title, FieldType.Lookup, options).ConfigureAwait(false) as Field;
        }

        public IField AddLookup(string title, FieldLookupOptions options = null)
        {
            return AddLookupAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserAsync(string title, FieldUserOptions options = null)
        {
            return await AddAsync(title, FieldType.User, options).ConfigureAwait(false) as Field;
        }

        public IField AddUser(string title, FieldUserOptions options = null)
        {
            return AddUserAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultiChoiceAsync(string title, FieldMultiChoiceOptions options = null)
        {
            return await AddAsync(title, FieldType.MultiChoice, options).ConfigureAwait(false) as Field;
        }

        public IField AddMultiChoice(string title, FieldMultiChoiceOptions options = null)
        {
            return AddMultiChoiceAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultilineTextAsync(string title, FieldMultilineTextOptions options = null)
        {
            return await AddAsync(title, FieldType.Note, options).ConfigureAwait(false) as Field;
        }

        public IField AddMultilineText(string title, FieldMultilineTextOptions options = null)
        {
            return AddMultilineTextAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddNumberAsync(string title, FieldNumberOptions options = null)
        {
            return await AddAsync(title, FieldType.Number, options).ConfigureAwait(false) as Field;
        }

        public IField AddNumber(string title, FieldNumberOptions options = null)
        {
            return AddNumberAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTextAsync(string title, FieldTextOptions options = null)
        {
            return await AddAsync(title, FieldType.Text, options).ConfigureAwait(false) as Field;
        }

        public IField AddText(string title, FieldTextOptions options = null)
        {
            return AddTextAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUrlAsync(string title, FieldUrlOptions options = null)
        {
            return await AddAsync(title, FieldType.URL, options).ConfigureAwait(false) as Field;
        }

        public IField AddUrl(string title, FieldUrlOptions options = null)
        {
            return AddUrlAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsXmlBatchAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return await AddFieldAsXmlBatchAsync(PnPContext.CurrentBatch, schemaXml, addToDefaultView, options).ConfigureAwait(false);
        }

        public IField AddFieldAsXmlBatch(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlBatchAsync(schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsXmlBatchAsync(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            if (addToDefaultView)
            {
                options |= AddFieldOptionsFlags.AddFieldToDefaultView;
            }

            var newField = CreateNewAndAdd() as Field;
            await newField.AddAsXmlBatchAsync(batch, schemaXml, options).ConfigureAwait(false);
            return newField;
        }

        public IField AddFieldAsXmlBatch(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlBatchAsync(batch, schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsXmlAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            if (addToDefaultView)
            {
                options |= AddFieldOptionsFlags.AddFieldToDefaultView;
            }

            var newField = CreateNewAndAdd() as Field;
            await newField.AddAsXmlAsync(schemaXml, options).ConfigureAwait(false);
            return newField;
        }

        public IField AddFieldAsXml(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlAsync(schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }

    }
}
