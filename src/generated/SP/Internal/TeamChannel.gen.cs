using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a TeamChannel object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class TeamChannel : BaseDataModel<ITeamChannel>, ITeamChannel
    {

        #region New properties

        public Guid FolderId { get => GetValue<Guid>(); set => SetValue(value); }

        public int GroupId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
