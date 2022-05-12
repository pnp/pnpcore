using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class GraphEmailAddress : BaseDataModel<IGraphEmailAddress>, IGraphEmailAddress
    {
        #region Construction
        public GraphEmailAddress()
        {
        }
        #endregion

        #region Properties
        public string Address { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }
        #endregion
    }
}
