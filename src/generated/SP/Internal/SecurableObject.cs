using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SecurableObject class, write your custom code here
    /// </summary>
    [SharePointType("SP.SecurableObject", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SecurableObject : BaseDataModel<ISecurableObject>, ISecurableObject
    {
        #region Construction
        public SecurableObject()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public ISecurableObject FirstUniqueAncestorSecurableObject { get => GetModelValue<ISecurableObject>(); }


        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
