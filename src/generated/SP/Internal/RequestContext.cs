using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RequestContext class, write your custom code here
    /// </summary>
    [SharePointType("SP.RequestContext", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RequestContext : BaseDataModel<IRequestContext>, IRequestContext
    {
        #region Construction
        public RequestContext()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IRequestContext Current { get => GetModelValue<IRequestContext>(); }


        public IList List { get => GetModelValue<IList>(); }


        public ISite Site { get => GetModelValue<ISite>(); }


        public IWeb Web { get => GetModelValue<IWeb>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
