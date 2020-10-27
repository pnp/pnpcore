using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TeamChannel class, write your custom code here
    /// </summary>
    [SharePointType("SP.TeamChannel", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TeamChannel : BaseDataModel<ITeamChannel>, ITeamChannel
    {
        #region Construction
        public TeamChannel()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid FolderId { get => GetValue<Guid>(); set => SetValue(value); }

        public int GroupId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
