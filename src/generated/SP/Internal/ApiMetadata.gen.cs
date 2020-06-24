using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ApiMetadata object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ApiMetadata : BaseDataModel<IApiMetadata>, IApiMetadata
    {

        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IApiMetadata Current
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ApiMetadata
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IApiMetadata>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("Types", Expandable = true)]
        public ITypeInformationCollection Types
        {
            get
            {
                if (!HasValue(nameof(Types)))
                {
                    var collection = new TypeInformationCollection(this.PnPContext, this, nameof(Types));
                    SetValue(collection);
                }
                return GetValue<ITypeInformationCollection>();
            }
        }

        #endregion

    }
}
