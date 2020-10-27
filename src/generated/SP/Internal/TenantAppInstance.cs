using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TenantAppInstance class, write your custom code here
    /// </summary>
    [SharePointType("SP.TenantAppInstance", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TenantAppInstance : BaseDataModel<ITenantAppInstance>, ITenantAppInstance
    {
        #region Construction
        public TenantAppInstance()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
