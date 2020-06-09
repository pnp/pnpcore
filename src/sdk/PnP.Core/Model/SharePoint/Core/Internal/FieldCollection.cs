using PnP.Core.QueryModel.Model;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection
    {
        public IField Add(string title, FieldType fieldType, BaseFieldAddOptions options = null)
        {
            return Add(PnPContext.CurrentBatch, title, fieldType, options);
        }

        public IField Add(Batch batch, string title, FieldType fieldType, BaseFieldAddOptions options = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (fieldType == FieldType.Invalid)
            {
                throw new ArgumentException($"{nameof(fieldType)} is invalid");
            }

            var newField = AddNewField();

            newField.Title = title;
            newField.FieldTypeKind = fieldType;
            if (options != null)
            {
                newField.DefaultFormula = options.DefaultFormula;
                newField.Description = options.Description;
                newField.EnforceUniqueValues = options.EnforceUniqueValues;
                newField.Group = options.Group;
                newField.Hidden = options.Hidden;
                newField.Required = options.Required;
                newField.Indexed = options.Indexed;
                newField.ValidationFormula = options.ValidationFormula;
                newField.ValidationMessage = options.ValidationMessage;
            }

            return newField.Add(batch) as Field;
        }

        public async Task<IField> AddAsync(string title, FieldType fieldType, BaseFieldAddOptions options = null)
        {
            return await AddAsync(title, null, fieldType, options);
        }

        public async Task<IField> AddAsync(string title, string internalName, FieldType fieldType, BaseFieldAddOptions options = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (fieldType == FieldType.Invalid)
            {
                throw new ArgumentException($"{nameof(fieldType)} is invalid");
            }

            var newField = AddNewField();

            newField.Title = title;
            
            if (!string.IsNullOrEmpty(internalName))
                newField.InternalName = internalName;

            newField.FieldTypeKind = fieldType;
            if (options != null)
            {
                newField.DefaultFormula = options.DefaultFormula;
                newField.Description = options.Description;
                newField.EnforceUniqueValues = options.EnforceUniqueValues;
                newField.Group = options.Group;
                newField.Hidden = options.Hidden;
                newField.Required = options.Required;
                newField.Indexed = options.Indexed;
                newField.ValidationFormula = options.ValidationFormula;
                newField.ValidationMessage = options.ValidationMessage;
            }

            return await newField.AddAsync().ConfigureAwait(false) as Field;
        }
    }
}
