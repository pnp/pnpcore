using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldLinkCollection : QueryableDataModelCollection<IFieldLink>, IFieldLinkCollection
    {
        public FieldLinkCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public async Task<IFieldLink> AddBatchAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, fieldInternalName, displayName, hidden, required, readOnly, showInDisplayForm).ConfigureAwait(false);
        }

        public IFieldLink AddBatch(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return AddBatchAsync(fieldInternalName, displayName, hidden, required, readOnly, showInDisplayForm).GetAwaiter().GetResult();
        }

        public async Task<IFieldLink> AddBatchAsync(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
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

            return await newFieldLink.AddBatchAsync(batch).ConfigureAwait(false) as FieldLink;
        }

        public IFieldLink AddBatch(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return AddBatchAsync(batch, fieldInternalName, displayName, hidden, required, readOnly, showInDisplayForm).GetAwaiter().GetResult();
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

        public IFieldLink Add(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true)
        {
            return AddAsync(fieldInternalName, displayName, hidden, required, readOnly, showInDisplayForm).GetAwaiter().GetResult();
        }

    }
}
