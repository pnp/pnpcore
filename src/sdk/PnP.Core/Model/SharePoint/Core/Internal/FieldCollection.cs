using PnP.Core.QueryModel.Model;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection
    {
        public IField Add(string title, FieldType fieldType, FieldOptions options)
        {
            return Add(PnPContext.CurrentBatch, title, fieldType, options);
        }

        public IField Add(Batch batch, string title, FieldType fieldType, FieldOptions options)
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

            return newField.Add(batch, additionalInfo) as Field;
        }

        public IField AddCalculated(string title, FieldCalculatedOptions options = null)
        {
            return AddCalculated(PnPContext.CurrentBatch, title, options);
        }

        public IField AddCalculated(Batch batch, string title, FieldCalculatedOptions options = null)
        {
            return Add(batch, title, FieldType.Calculated, options);
        }

        public IField AddChoice(string title, FieldChoiceOptions options = null)
        {
            return AddChoice(PnPContext.CurrentBatch, title, options);
        }

        public IField AddChoice(Batch batch, string title, FieldChoiceOptions options = null)
        {
            return Add(batch, title, FieldType.Choice, options);
        }

        public IField AddCurrency(string title, FieldCurrencyOptions options = null)
        {
            return AddCurrency(PnPContext.CurrentBatch, title, options);
        }

        public IField AddCurrency(Batch batch, string title, FieldCurrencyOptions options = null)
        {
            return Add(batch, title, FieldType.Currency, options);
        }

        public IField AddDateTime(string title, FieldDateTimeOptions options = null)
        {
            return AddDateTime(PnPContext.CurrentBatch, title, options);
        }

        public IField AddDateTime(Batch batch, string title, FieldDateTimeOptions options = null)
        {
            return Add(batch, title, FieldType.DateTime, options);
        }

        public IField AddLookup(string title, FieldLookupOptions options = null)
        {
            return AddLookup(PnPContext.CurrentBatch, title, options);
        }

        public IField AddLookup(Batch batch, string title, FieldLookupOptions options = null)
        {
            return Add(batch, title, FieldType.Lookup, options);
        }

        public IField AddMultiChoice(string title, FieldMultiChoiceOptions options = null)
        {
            return AddMultiChoice(PnPContext.CurrentBatch, title, options);
        }

        public IField AddMultiChoice(Batch batch, string title, FieldMultiChoiceOptions options = null)
        {
            return Add(batch, title, FieldType.MultiChoice, options);
        }

        public IField AddMultilineText(string title, FieldMultilineTextOptions options = null)
        {
            return AddMultilineText(PnPContext.CurrentBatch, title, options);
        }

        public IField AddMultilineText(Batch batch, string title, FieldMultilineTextOptions options = null)
        {
            return Add(batch, title, FieldType.Note, options);
        }

        public IField AddNumber(string title, FieldNumberOptions options = null)
        {
            return AddNumber(PnPContext.CurrentBatch, title, options);
        }

        public IField AddNumber(Batch batch, string title, FieldNumberOptions options = null)
        {
            return Add(batch, title, FieldType.Number, options);
        }

        public IField AddText(string title, FieldTextOptions options = null)
        {
            return AddText(PnPContext.CurrentBatch, title, options);
        }

        public IField AddText(Batch batch, string title, FieldTextOptions options = null)
        {
            return Add(batch, title, FieldType.Number, options);
        }

        public IField AddUrl(string title, FieldUrlOptions options = null)
        {
            return AddUrl(PnPContext.CurrentBatch, title, options);
        }

        public IField AddUrl(Batch batch, string title, FieldUrlOptions options = null)
        {
            return Add(batch, title, FieldType.URL, options);
        }

        public IField AddUser(string title, FieldUserOptions options = null)
        {
            return AddUser(PnPContext.CurrentBatch, title, options);
        }

        public IField AddUser(Batch batch, string title, FieldUserOptions options = null)
        {
            return Add(batch, title, FieldType.User, options);
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
    }
}
