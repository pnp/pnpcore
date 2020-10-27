using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RequestUserContext class, write your custom code here
    /// </summary>
    [SharePointType("SP.RequestUserContext", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RequestUserContext : BaseDataModel<IRequestUserContext>, IRequestUserContext
    {
        #region Construction
        public RequestUserContext()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IRequestUserContext Current { get => GetModelValue<IRequestUserContext>(); }


        public IUser User { get => GetModelValue<IUser>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
