using PnP.Core.Model.SharePoint;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.RoleAssignment", Target = typeof(Web), Uri = "_api/Web/RoleAssignments/GetByPrincipalId({PrincipalId})", Get = "_api/web/RoleAssignments", LinqGet = "_api/web/RoleAssignments")]
    [SharePointType("SP.RoleAssignment", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments/GetByPrincipalId({PrincipalId})", Get = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments")]
    [SharePointType("SP.RoleAssignment", Target = typeof(ListItem), Uri = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments/GetByPrincipalId({PrincipalId})", Get = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments", LinqGet = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments", Delete = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments/removeroleassignment(principalId={PrincipalId,roleDefId={RoleDefId})")]
    internal sealed class RoleAssignment : BaseDataModel<IRoleAssignment>, IRoleAssignment
    {
        public int PrincipalId { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("RoleDefinitionBindings")]
        public IRoleDefinitionCollection RoleDefinitions { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }

        [KeyProperty(nameof(PrincipalId))]
        public override object Key { get => PrincipalId; set => PrincipalId = int.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        public RoleAssignment()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (Parent != null && Parent.Parent != null)
                {
                    var parentType = Parent.Parent.GetType();
                    if (parentType == typeof(ListItemCollection))
                    {
                        // If this is a List Item's role assignment, we need to grab the parent list item's ID and swap out the token
                        // Go up 3 levels to get the List (?)
                        if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null)
                        {
                            var containingList = Parent.Parent.Parent as IList;
                            if (containingList != null)
                            {
                                var request = api.ApiCall.Request.Replace("{List.Id}", containingList.Id.ToString());
                                api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                            }
                        }
                    }
                }
                return api;
            };
        }
    }
}
