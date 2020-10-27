using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RecycleBinItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.RecycleBinItem", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RecycleBinItem : BaseDataModel<IRecycleBinItem>, IRecycleBinItem
    {
        #region Construction
        public RecycleBinItem()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string AuthorEmail { get => GetValue<string>(); set => SetValue(value); }

        public string AuthorName { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByEmail { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime DeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string DeletedDateLocalFormatted { get => GetValue<string>(); set => SetValue(value); }

        public string DirName { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public int ItemState { get => GetValue<int>(); set => SetValue(value); }

        public int ItemType { get => GetValue<int>(); set => SetValue(value); }

        public string LeafName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public IUser Author { get => GetModelValue<IUser>(); }


        public IUser DeletedBy { get => GetModelValue<IUser>(); }


        #endregion

        #region New properties

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
