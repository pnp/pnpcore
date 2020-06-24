using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Alert object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Alert : BaseDataModel<IAlert>, IAlert
    {

        #region New properties

        public int AlertFrequency { get => GetValue<int>(); set => SetValue(value); }

        public string AlertTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime AlertTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public int AlertType { get => GetValue<int>(); set => SetValue(value); }

        public bool AlwaysNotify { get => GetValue<bool>(); set => SetValue(value); }

        public int DeliveryChannels { get => GetValue<int>(); set => SetValue(value); }

        public int EventType { get => GetValue<int>(); set => SetValue(value); }

        public string Filter { get => GetValue<string>(); set => SetValue(value); }

        public Guid ID { get => GetValue<Guid>(); set => SetValue(value); }

        public int ItemID { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListID { get => GetValue<Guid>(); set => SetValue(value); }

        public string ListUrl { get => GetValue<string>(); set => SetValue(value); }

        public int Status { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int UserId { get => GetValue<int>(); set => SetValue(value); }

        public IPropertyValues AllProperties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PropertyValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPropertyValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IListItem Item
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ListItem
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IListItem>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IList List
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new List
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IList>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IUser User
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

    }
}
