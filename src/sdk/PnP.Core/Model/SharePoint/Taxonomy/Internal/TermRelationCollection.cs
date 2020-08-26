using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermRelationCollection
    {
        public async Task<ITermRelation> AddAsync(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            if (targetSet == null)
            {
                throw new ArgumentNullException(nameof(targetSet));
            }

            TermRelation newTermRelation = PrepNewTermRelation(relationship, targetSet, fromTerm);

            return await newTermRelation.AddAsync().ConfigureAwait(false) as TermRelation;
        }

        private TermRelation PrepNewTermRelation(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm)
        {
            var newTermRelation = CreateNewAndAdd() as TermRelation;

            // Assign field values
            newTermRelation.Relationship = relationship;
            newTermRelation.Set = targetSet;
            newTermRelation.FromTerm = fromTerm;

            return newTermRelation;
        }

        public ITermRelation Add(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            return AddAsync(relationship, targetSet, fromTerm).GetAwaiter().GetResult();
        }

        public async Task<ITermRelation> AddBatchAsync(Batch batch, TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            if (targetSet == null)
            {
                throw new ArgumentNullException(nameof(targetSet));
            }

            TermRelation newTermRelation = PrepNewTermRelation(relationship, targetSet, fromTerm);

            return await newTermRelation.AddBatchAsync(batch).ConfigureAwait(false) as TermRelation;
        }

        public ITermRelation AddBatch(Batch batch, TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            return AddBatchAsync(batch, relationship, targetSet, fromTerm).GetAwaiter().GetResult();
        }

        public async Task<ITermRelation> AddBatchAsync(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, relationship, targetSet, fromTerm).ConfigureAwait(false);
        }

        public ITermRelation AddBatch(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null)
        {
            return AddBatchAsync(relationship, targetSet, fromTerm).GetAwaiter().GetResult();
        }

    }
}
