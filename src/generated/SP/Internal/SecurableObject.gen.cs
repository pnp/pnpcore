using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SecurableObject object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SecurableObject : BaseDataModel<ISecurableObject>, ISecurableObject
    {

        #region New properties

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public ISecurableObject FirstUniqueAncestorSecurableObject
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SecurableObject
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISecurableObject>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("RoleAssignments", Expandable = true)]
        public IRoleAssignmentCollection RoleAssignments
        {
            get
            {
                if (!HasValue(nameof(RoleAssignments)))
                {
                    var collection = new RoleAssignmentCollection(this.PnPContext, this, nameof(RoleAssignments));
                    SetValue(collection);
                }
                return GetValue<IRoleAssignmentCollection>();
            }
        }

        #endregion

    }
}
