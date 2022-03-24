using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType("SP.Navigation", Target = typeof(Web), Get = "_api/Web/Navigation", LinqGet = "_api/Web/Navigation")]
    internal sealed class Navigation : BaseDataModel<INavigation>, INavigation
    {
        #region Construction
        public Navigation()
        {
        }
        #endregion

        #region Properties        

        public Guid Id { get => GetValue<Guid>(); set => Guid.NewGuid(); }

        [SharePointProperty("UseShared")]
        public bool UseShared { get => GetValue<bool>(); set => SetValue(value); }

        public INavigationNodeCollection QuickLaunch { get => GetModelCollectionValue<INavigationNodeCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion
    }
}
