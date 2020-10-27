using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RecentList class, write your custom code here
    /// </summary>
    [SharePointType("SP.RecentList", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RecentList : BaseDataModel<IRecentList>, IRecentList
    {
        #region Construction
        public RecentList()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Color { get => GetValue<string>(); set => SetValue(value); }

        public string CreatedByName { get => GetValue<string>(); set => SetValue(value); }

        public string CreatedByUpn { get => GetValue<string>(); set => SetValue(value); }

        public string CreatedDate { get => GetValue<string>(); set => SetValue(value); }

        public string Icon { get => GetValue<string>(); set => SetValue(value); }

        public string LastModifiedDate { get => GetValue<string>(); set => SetValue(value); }

        public int LastViewDate { get => GetValue<int>(); set => SetValue(value); }

        public string LastviewDateTime { get => GetValue<string>(); set => SetValue(value); }

        public string ListId { get => GetValue<string>(); set => SetValue(value); }

        public string ListTitle { get => GetValue<string>(); set => SetValue(value); }

        public string ListUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShouldRemove { get => GetValue<bool>(); set => SetValue(value); }

        public string SiteColor { get => GetValue<string>(); set => SetValue(value); }

        public string SiteId { get => GetValue<string>(); set => SetValue(value); }

        public string SiteTitle { get => GetValue<string>(); set => SetValue(value); }

        public string SiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebId { get => GetValue<string>(); set => SetValue(value); }

        public string WebTemplateConfiguration { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
