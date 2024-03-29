﻿using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Microsoft 365 Term store
    /// </summary>
    [ConcreteType(typeof(TermStore))]
    public interface ITermStore : IDataModel<ITermStore>, IDataModelGet<ITermStore>, IDataModelLoad<ITermStore>, IDataModelUpdate
    {
        #region Properties

        /// <summary>
        /// The Unique ID of the Term Store
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Default language of the termstore.
        /// </summary>
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// List of languages for the term store.
        /// </summary>
        public List<string> Languages { get; }

        /// <summary>
        /// Collection of term groups in this term store
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITermGroupCollection Groups { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        Task<ITermSet> GetTermSetByIdAsync(string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        ITermSet GetTermSetById(string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        Task<ITermSet> GetTermSetByIdBatchAsync(string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        ITermSet GetTermSetByIdBatch(string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="batch">Batch to add this reques to</param>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        Task<ITermSet> GetTermSetByIdBatchAsync(Batch batch, string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a termset from this termstore via it's id
        /// </summary>
        /// <param name="batch">Batch to add this reques to</param>
        /// <param name="id">Termset id to retrieve</param>
        /// <param name="selectors">Properties to load for the termset</param>
        /// <returns>The loaded termset</returns>
        ITermSet GetTermSetByIdBatch(Batch batch, string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        Task<ITerm> GetTermByIdAsync(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        ITerm GetTermById(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        Task<ITerm> GetTermByIdBatchAsync(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        ITerm GetTermByIdBatch(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="batch">Batch to add this reques to</param>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        Task<ITerm> GetTermByIdBatchAsync(Batch batch, string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        /// <summary>
        /// Gets a term from this termstore via it's id and it's termset id
        /// </summary>
        /// <param name="batch">Batch to add this reques to</param>
        /// <param name="termSetId">Termset id of the termset containing the term</param>
        /// <param name="termId">Term id of the term to retrieve</param>
        /// <param name="selectors">Properties to load for the term</param>
        /// <returns>The loaded term</returns>
        ITerm GetTermByIdBatch(Batch batch, string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors);

        #endregion
    }
}