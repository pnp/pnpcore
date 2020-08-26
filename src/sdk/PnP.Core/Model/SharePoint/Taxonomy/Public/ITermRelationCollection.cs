using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of terms
    /// </summary>
    public interface ITermRelationCollection : IDataModelCollection<ITermRelation>, ISupportPaging<ITermRelation>
    {
        /// <summary>
        /// Adds a new term relation
        /// </summary>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public Task<ITermRelation> AddAsync(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);

        /// <summary>
        /// Adds a new term relation
        /// </summary>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public ITermRelation Add(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);


        /// <summary>
        /// Adds a new term relation via a batch request
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public Task<ITermRelation> AddBatchAsync(Batch batch, TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);

        /// <summary>
        /// Adds a new term relation via a batch request
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public ITermRelation AddBatch(Batch batch, TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);

        /// <summary>
        /// Adds a new term relation via a batch request
        /// </summary>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public Task<ITermRelation> AddBatchAsync(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);

        /// <summary>
        /// Adds a new term relation via a batch request
        /// </summary>
        /// <param name="relationship">Type of relation to be created. Possible values are: pin, reuse.</param>
        /// <param name="targetSet">The set where the relationship needs to be created.</param>
        /// <param name="fromTerm">The term with which the relationship needs to be created.</param>
        /// <returns>Newly added term relation ship</returns>
        public ITermRelation AddBatch(TermRelationType relationship, ITermSet targetSet, ITerm fromTerm = null);

    }
}
