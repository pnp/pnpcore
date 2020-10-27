using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// WebTemplate class, write your custom code here
    /// </summary>
    [SharePointType("SP.WebTemplate", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class WebTemplate : BaseDataModel<IWebTemplate>, IWebTemplate
    {
        #region Construction
        public WebTemplate()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayCategory { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IsHidden { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRootWebOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSubWebOnly { get => GetValue<bool>(); set => SetValue(value); }

        public int Lcid { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
