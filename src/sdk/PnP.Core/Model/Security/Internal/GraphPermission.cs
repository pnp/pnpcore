using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    [GraphType(Uri = V, LinqGet = "sites/{Parent.SiteId}/drive/items/{Parent.GraphId}/permissions")]
    internal sealed class GraphPermission : BaseDataModel<IGraphPermission>, IGraphPermission
    {
        private const string baseUri = "sites/{Parent.SiteId}/drive/items/{Parent.GraphId}/permissions";
        private const string V = baseUri + "/{GraphId}";

        #region Constructor
        internal GraphPermission()
        {

        }
        #endregion

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public List<string> Roles { get => GetValue<List<string>>(); set => SetValue(value); }

        public string ShareId { get => GetValue<string>(); set => SetValue(value); }

        public string ExpirationDateTime { get => GetValue<string>(); set => SetValue(value); }

        public bool HasPassword { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
