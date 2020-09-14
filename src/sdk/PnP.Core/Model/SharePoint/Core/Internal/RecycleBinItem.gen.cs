using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RecycleBinItem object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RecycleBinItem : BaseDataModel<IRecycleBinItem>, IRecycleBinItem
    {
        public string AuthorEmail { get => GetValue<string>(); set => SetValue(value); }

        public string AuthorName { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByEmail { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime DeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string DeletedDateLocalFormatted { get => GetValue<string>(); set => SetValue(value); }

        public string DirName { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public RecycleBinItemState ItemState { get => GetValue<RecycleBinItemState>(); set => SetValue(value); }

        public RecycleBinItemType ItemType { get => GetValue<RecycleBinItemType>(); set => SetValue(value); }

        public string LeafName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public long Size { get => GetValue<long>(); set => SetValue(value); }

        [SharePointProperty("Author", Expandable = true)]
        public ISharePointUser Author
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("DeletedBy", Expandable = true)]
        public ISharePointUser DeletedBy
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
