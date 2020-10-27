using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SPMigrationJobStatus class, write your custom code here
    /// </summary>
    [SharePointType("SP.SPMigrationJobStatus", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SPMigrationJobStatus : BaseDataModel<ISPMigrationJobStatus>, ISPMigrationJobStatus
    {
        #region Construction
        public SPMigrationJobStatus()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid JobId { get => GetValue<Guid>(); set => SetValue(value); }

        public int JobState { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
