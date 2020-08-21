using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermGroupCollection
    {
        public async Task<ITermGroup> AddAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as TermGroup;

            // Assign field values
            newGroup.Name = name;
            
            if (scope != TermGroupScope.Global)
            {
                newGroup.Scope = scope;
            }

            if (description != null)
            {
                newGroup.Description = description;
            }

            return await newGroup.AddAsync().ConfigureAwait(false) as TermGroup;
        }

        public ITermGroup Add(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddAsync(name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermGroup> AddBatchAsync(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as TermGroup;

            // Assign field values
            newGroup.Name = name;

            if (scope != TermGroupScope.Global)
            {
                newGroup.Scope = scope;
            }

            if (description != null)
            {
                newGroup.Description = description;
            }

            return await newGroup.AddBatchAsync(batch).ConfigureAwait(false) as TermGroup;
        }

        public ITermGroup AddBatch(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddBatchAsync(batch, name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermGroup> AddBatchAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, description).ConfigureAwait(false);
        }

        public ITermGroup AddBatch(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddBatchAsync(name, description).GetAwaiter().GetResult();
        }

    }
}
