using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// NavigationNode class, write your custom code here
    /// </summary>
    [SharePointType("SP.NavigationNode", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class NavigationNode : BaseDataModel<INavigationNode>, INavigationNode
    {
        #region Construction
        public NavigationNode()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int CurrentLCID { get => GetValue<int>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsDocLib { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsExternal { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public int ListTemplateType { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public INavigationNodeCollection Children { get => GetModelCollectionValue<INavigationNodeCollection>(); }


        public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
