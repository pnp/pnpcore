using System;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldLinkCollection
    {
        public IFieldLink Add(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return Add(PnPContext.CurrentBatch, fieldInternalName, displayName, hidden, required, readOnly, showInDisplayForm);
        }

        public IFieldLink Add(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            if (string.IsNullOrEmpty(fieldInternalName))
            {
                throw new ArgumentNullException(nameof(fieldInternalName));
            }

            var newFieldLink = CreateNewAndAdd() as FieldLink;

            newFieldLink.FieldInternalName = fieldInternalName;
            newFieldLink.DisplayName = displayName;
            newFieldLink.Hidden = hidden;
            newFieldLink.Required = required;
            newFieldLink.ReadOnly = readOnly;
            newFieldLink.ShowInDisplayForm = showInDisplayForm;

            return newFieldLink.Add(batch) as FieldLink;
        }

        public async Task<IFieldLink> AddAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            if (string.IsNullOrEmpty(fieldInternalName))
            {
                throw new ArgumentNullException(nameof(fieldInternalName));
            }

            var newFieldLink = CreateNewAndAdd() as FieldLink;

            newFieldLink.FieldInternalName = fieldInternalName;
            newFieldLink.DisplayName = displayName;
            newFieldLink.Hidden = hidden;
            newFieldLink.Required = required;
            newFieldLink.ReadOnly = readOnly;
            newFieldLink.ShowInDisplayForm = showInDisplayForm;

            return await newFieldLink.AddAsync().ConfigureAwait(false) as FieldLink;
        }
    }
}
