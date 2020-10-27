using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLocation class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLocation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldLocation : BaseDataModel<IFieldLocation>, IFieldLocation
    {
        #region Construction
        public FieldLocation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
