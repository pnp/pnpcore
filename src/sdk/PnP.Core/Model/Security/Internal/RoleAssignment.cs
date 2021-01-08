using PnP.Core.Model.SharePoint;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.RoleAssignment", Target = typeof(Web), Uri = "_api/Web/RoleAssignments/GetByPrincipal({PrincipalId})", Get = "_api/web/RoleAssignments", LinqGet = "_api/web/RoleAssignments")]
    [SharePointType("SP.RoleAssignment", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments/GetByPrincipal({PrincipalId})", Get = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments")]
    internal class RoleAssignment : BaseDataModel<IRoleAssignment>, IRoleAssignment
    {
        public int PrincipalId { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("RoleDefinitionBindings")]
        public IRoleDefinitionCollection RoleDefinitions { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }

        [KeyProperty(nameof(PrincipalId))]
        public override object Key { get => PrincipalId; set => PrincipalId = int.Parse(value.ToString()); }

    }
}
