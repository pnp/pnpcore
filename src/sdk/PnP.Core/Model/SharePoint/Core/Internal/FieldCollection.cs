using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection
    {
        public async Task<IField> AddBatchAsync(string title, FieldType fieldType, FieldOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, title, fieldType, options).ConfigureAwait(false);
        }

        public async Task<IField> AddBatchAsync(Batch batch, string title, FieldType fieldType, FieldOptions options)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (fieldType == FieldType.Invalid)
                throw new ArgumentException($"{nameof(fieldType)} is invalid");

            if (!ValidateFieldOptions(fieldType, options))
                throw new ClientException(ErrorType.InvalidParameters, $"{nameof(options)} is invalid for field type {fieldType}");

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

        public async Task<IField> AddCalculatedBatchAsync(string title, FieldCalculatedOptions options = null)
        {
            return await AddCalculatedBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddCalculatedBatchAsync(Batch batch, string title, FieldCalculatedOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Calculated, options).ConfigureAwait(false);
        }

        public async Task<IField> AddChoiceBatchAsync(string title, FieldChoiceOptions options = null)
        {
            return await AddChoiceBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddChoiceBatchAsync(Batch batch, string title, FieldChoiceOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Choice, options).ConfigureAwait(false);
        }

        public async Task<IField> AddCurrencyBatchAsync(string title, FieldCurrencyOptions options = null)
        {
            return await AddCurrencyBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddCurrencyBatchAsync(Batch batch, string title, FieldCurrencyOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Currency, options).ConfigureAwait(false);
        }

        public async Task<IField> AddDateTimeBatchAsync(string title, FieldDateTimeOptions options = null)
        {
            return await AddDateTimeBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddDateTimeBatchAsync(Batch batch, string title, FieldDateTimeOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.DateTime, options).ConfigureAwait(false);
        }

        public async Task<IField> AddLookupBatchAsync(string title, FieldLookupOptions options = null)
        {
            return await AddLookupBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddLookupBatchAsync(Batch batch, string title, FieldLookupOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Lookup, options).ConfigureAwait(false);
        }

        public async Task<IField> AddMultiChoiceBatchAsync(string title, FieldMultiChoiceOptions options = null)
        {
            return await AddMultiChoiceBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddMultiChoiceBatchAsync(Batch batch, string title, FieldMultiChoiceOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.MultiChoice, options).ConfigureAwait(false);
        }

        public async Task<IField> AddMultilineTextBatchAsync(string title, FieldMultilineTextOptions options = null)
        {
            return await AddMultilineTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddMultilineTextBatchAsync(Batch batch, string title, FieldMultilineTextOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Note, options).ConfigureAwait(false);
        }

        public async Task<IField> AddNumberBatchAsync(string title, FieldNumberOptions options = null)
        {
            return await AddNumberBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddNumberBatchAsync(Batch batch, string title, FieldNumberOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Number, options).ConfigureAwait(false);
        }

        public async Task<IField> AddTextBatchAsync(string title, FieldTextOptions options = null)
        {
            return await AddTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddTextBatchAsync(Batch batch, string title, FieldTextOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.Number, options).ConfigureAwait(false);
        }

        public async Task<IField> AddUrlBatchAsync(string title, FieldUrlOptions options = null)
        {
            return await AddUrlBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddUrlBatchAsync(Batch batch, string title, FieldUrlOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.URL, options).ConfigureAwait(false);
        }

        public async Task<IField> AddUserBatchAsync(string title, FieldUserOptions options = null)
        {
            return await AddUserBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public async Task<IField> AddUserBatchAsync(Batch batch, string title, FieldUserOptions options = null)
        {
            return await AddBatchAsync(batch, title, FieldType.User, options).ConfigureAwait(false);
        }


        public async Task<IField> AddAsync(string title, FieldType fieldType, FieldOptions options)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (fieldType == FieldType.Invalid)
                throw new ArgumentException($"{nameof(fieldType)} is invalid");

            if (!ValidateFieldOptions(fieldType, options))
                throw new ClientException(ErrorType.InvalidParameters, $"{nameof(options)} is invalid for field type {fieldType}");

            var newField = CreateNewAndAdd() as Field;

            newField.Title = title;
            newField.FieldTypeKind = fieldType;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { Field.FieldOptionsAdditionalInformationKey, options }
            };

            return await newField.AddAsync(additionalInfo ).ConfigureAwait(false) as Field;
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

        public async Task<IField> AddChoiceAsync(string title, FieldChoiceOptions options = null)
        {
            return await AddAsync(title, FieldType.Choice, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddCurrencyAsync(string title, FieldCurrencyOptions options = null)
        {
            return await AddAsync(title, FieldType.Currency, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddDateTimeAsync(string title, FieldDateTimeOptions options = null)
        {
            return await AddAsync(title, FieldType.DateTime, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddLookupAsync(string title, FieldLookupOptions options = null)
        {
            return await AddAsync(title, FieldType.Lookup, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddUserAsync(string title, FieldUserOptions options = null)
        {
            return await AddAsync(title, FieldType.User, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddMultiChoiceAsync(string title, FieldMultiChoiceOptions options = null)
        {
            return await AddAsync(title, FieldType.MultiChoice, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddMultilineTextAsync(string title, FieldMultilineTextOptions options = null)
        {
            return await AddAsync(title, FieldType.Note, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddNumberAsync(string title, FieldNumberOptions options = null)
        {
            return await AddAsync(title, FieldType.Number, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddTextAsync(string title, FieldTextOptions options = null)
        {
            return await AddAsync(title, FieldType.Text, options).ConfigureAwait(false) as Field;
        }

        public async Task<IField> AddUrlAsync(string title, FieldUrlOptions options = null)
        {
            return await AddAsync(title, FieldType.URL, options).ConfigureAwait(false) as Field;
        }



        public async Task<IField> AddFieldAsXmlBatchAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return await AddFieldAsXmlBatchAsync(PnPContext.CurrentBatch, schemaXml, addToDefaultView, options).ConfigureAwait(false);
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
    }
}
