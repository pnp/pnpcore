using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeUser class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeUser", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeUser : BaseDataModel<IChangeUser>, IChangeUser
    {
        #region Construction
        public ChangeUser()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool Activate { get => GetValue<bool>(); set => SetValue(value); }

        public int UserId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
