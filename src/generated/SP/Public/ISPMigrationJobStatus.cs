using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SPMigrationJobStatus object
    /// </summary>
    [ConcreteType(typeof(SPMigrationJobStatus))]
    public interface ISPMigrationJobStatus : IDataModel<ISPMigrationJobStatus>, IDataModelGet<ISPMigrationJobStatus>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int JobState { get; set; }

        #endregion

    }
}
