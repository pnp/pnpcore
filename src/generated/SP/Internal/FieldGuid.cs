using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldGuid class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldGuid", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldGuid : BaseDataModel<IFieldGuid>, IFieldGuid
    {
        #region Construction
        public FieldGuid()
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
