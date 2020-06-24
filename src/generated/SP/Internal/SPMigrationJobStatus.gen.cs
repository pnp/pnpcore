using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SPMigrationJobStatus object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SPMigrationJobStatus : BaseDataModel<ISPMigrationJobStatus>, ISPMigrationJobStatus
    {

        #region New properties

        public Guid JobId { get => GetValue<Guid>(); set => SetValue(value); }

        public int JobState { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
