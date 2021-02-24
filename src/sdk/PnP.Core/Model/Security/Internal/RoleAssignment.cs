using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.RoleAssignment", Target = typeof(Web), Uri = "_api/Web/RoleAssignments/GetByPrincipal({PrincipalId})", Get = "_api/web/RoleAssignments", LinqGet = "_api/web/RoleAssignments")]
    [SharePointType("SP.RoleAssignment", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments/GetByPrincipal({PrincipalId})", Get = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/RoleAssignments")]
    [SharePointType("SP.RoleAssignment", Target = typeof(ListItem), Uri = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments/GetByPrincipal({PrincipalId})", Get = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments", LinqGet = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments", Delete = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/RoleAssignments/removeroleassignment(principalId={PrincipalId,roleDefId={RoleDefId})")]
    internal class RoleAssignment : BaseDataModel<IRoleAssignment>, IRoleAssignment
    {
        public int PrincipalId { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("RoleDefinitionBindings")]
        public IRoleDefinitionCollection RoleDefinitions { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }

        [KeyProperty(nameof(PrincipalId))]
        public override object Key { get => PrincipalId; set => PrincipalId = int.Parse(value.ToString()); }

        public RoleAssignment()
        {
            AddApiCallHandler = async (additionalInfo) =>
            {
                var principalId = additionalInfo["principalId"];
                var roleDefId = additionalInfo["roleDefId"];

                // get the API stub url to make sure we're adding a role assignment onto the right securable object
                string stubUrl = "";

                var attrs = this.GetType().GetCustomAttributes(true)
                    .Where(c => c.GetType() == typeof(SharePointTypeAttribute));

                // RoleAssignment Parents are the collection, so skip a parent when looking for the securable object
                var parentType = Parent.Parent.GetType();
                foreach (var attr in attrs)
                {
                    if (attr is SharePointTypeAttribute)
                    {
                        var spTypeAttribute = attr as SharePointTypeAttribute;
                        if (spTypeAttribute.Target == parentType)
                        {
                            stubUrl = spTypeAttribute.Get;
                            break;
                        }
                    }
                }

                // If this is a list item having it's permissions set, we need to set the ListID of the API call
                if (parentType == typeof(ListItem))
                {
                    // Go up 4 levels to get the List (?)
                    var containingList = Parent.Parent.Parent.Parent as IList;
                    if (containingList != null)
                    {
                        stubUrl = stubUrl.Replace("{List.Id}", containingList.Id.ToString());
                    }
                }

                return new ApiCall($"{stubUrl}/addroleassignment(principalId={principalId},roleDefId={roleDefId})", ApiType.SPORest);
            };
        }
    }
}
