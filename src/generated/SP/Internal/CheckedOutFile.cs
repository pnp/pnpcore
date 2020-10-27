using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// CheckedOutFile class, write your custom code here
    /// </summary>
    [SharePointType("SP.CheckedOutFile", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class CheckedOutFile : BaseDataModel<ICheckedOutFile>, ICheckedOutFile
    {
        #region Construction
        public CheckedOutFile()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int CheckedOutById { get => GetValue<int>(); set => SetValue(value); }

        public IUser CheckedOutBy { get => GetModelValue<IUser>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
