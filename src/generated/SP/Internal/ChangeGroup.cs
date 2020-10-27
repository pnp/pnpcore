using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeGroup class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeGroup", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeGroup : BaseDataModel<IChangeGroup>, IChangeGroup
    {
        #region Construction
        public ChangeGroup()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int GroupId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
