using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeList class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeList", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeList : BaseDataModel<IChangeList>, IChangeList
    {
        #region Construction
        public ChangeList()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int BaseTemplate { get => GetValue<int>(); set => SetValue(value); }

        public string Editor { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string RootFolderUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
