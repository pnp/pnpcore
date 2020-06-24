using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RelatedField object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RelatedField : BaseDataModel<IRelatedField>, IRelatedField
    {

        #region New properties

        public Guid FieldId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public int RelationshipDeleteBehavior { get => GetValue<int>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        public IList LookupList
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


        #endregion

    }
}
