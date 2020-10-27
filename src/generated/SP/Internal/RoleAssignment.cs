using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RoleAssignment class, write your custom code here
    /// </summary>
    [SharePointType("SP.RoleAssignment", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RoleAssignment : BaseDataModel<IRoleAssignment>, IRoleAssignment
    {
        #region Construction
        public RoleAssignment()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int PrincipalId { get => GetValue<int>(); set => SetValue(value); }

        public IPrincipal Member { get => GetModelValue<IPrincipal>(); }


        public IRoleDefinitionCollection RoleDefinitionBindings { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
