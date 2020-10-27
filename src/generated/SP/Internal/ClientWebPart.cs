using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ClientWebPart class, write your custom code here
    /// </summary>
    [SharePointType("SP.ClientWebPart", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ClientWebPart : BaseDataModel<IClientWebPart>, IClientWebPart
    {
        #region Construction
        public ClientWebPart()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
