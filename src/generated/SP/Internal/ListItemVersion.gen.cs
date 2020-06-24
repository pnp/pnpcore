using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ListItemVersion object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ListItemVersion : BaseDataModel<IListItemVersion>, IListItemVersion
    {

        #region New properties

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int VersionId { get => GetValue<int>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public IUser CreatedBy
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


        [SharePointProperty("Fields", Expandable = true)]
        public IFieldCollection Fields
        {
            get
            {
                if (!HasValue(nameof(Fields)))
                {
                    var collection = new FieldCollection(this.PnPContext, this, nameof(Fields));
                    SetValue(collection);
                }
                return GetValue<IFieldCollection>();
            }
        }

        public IFileVersion FileVersion
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new FileVersion
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFileVersion>();
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
