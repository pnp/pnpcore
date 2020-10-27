using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeItem", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeItem : BaseDataModel<IChangeItem>, IChangeItem
    {
        #region Construction
        public ChangeItem()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int ActivityType { get => GetValue<int>(); set => SetValue(value); }

        public string Editor { get => GetValue<string>(); set => SetValue(value); }

        public string EditorEmailHint { get => GetValue<string>(); set => SetValue(value); }

        public string EditorLoginName { get => GetValue<string>(); set => SetValue(value); }

        public int FileSystemObjectType { get => GetValue<int>(); set => SetValue(value); }

        public string FileType { get => GetValue<string>(); set => SetValue(value); }

        public string Hashtag { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemId { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public int ListTemplate { get => GetValue<int>(); set => SetValue(value); }

        public string ListTitle { get => GetValue<string>(); set => SetValue(value); }

        public bool MoveWasForRecycle { get => GetValue<bool>(); set => SetValue(value); }

        public bool MoveWasForRestore { get => GetValue<bool>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
