using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RoleAssignment object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RoleAssignment : BaseDataModel<IRoleAssignment>, IRoleAssignment
    {

        #region New properties

        public int PrincipalId { get => GetValue<int>(); set => SetValue(value); }

        public IPrincipal Member
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Principal
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPrincipal>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("RoleDefinitionBindings", Expandable = true)]
        public IRoleDefinitionCollection RoleDefinitionBindings
        {
            get
            {
                if (!HasValue(nameof(RoleDefinitionBindings)))
                {
                    var collection = new RoleDefinitionCollection(this.PnPContext, this, nameof(RoleDefinitionBindings));
                    SetValue(collection);
                }
                return GetValue<IRoleDefinitionCollection>();
            }
        }

        #endregion

    }
}
