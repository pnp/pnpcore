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

        #region New properties

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

        public IUser Author
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IUser DeletedBy
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
