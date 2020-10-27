using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldGeolocation class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldGeolocation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldGeolocation : BaseDataModel<IFieldGeolocation>, IFieldGeolocation
    {
        #region Construction
        public FieldGeolocation()
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
