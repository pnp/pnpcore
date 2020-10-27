using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RemoteWeb class, write your custom code here
    /// </summary>
    [SharePointType("SP.RemoteWeb", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RemoteWeb : BaseDataModel<IRemoteWeb>, IRemoteWeb
    {
        #region Construction
        public RemoteWeb()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool CanSendEmail { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public IWeb Web { get => GetModelValue<IWeb>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
