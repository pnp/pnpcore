using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "users/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class GraphUser : BaseDataModel<IGraphUser>, IGraphUser
    {
        #region Construction
        public GraphUser()
        {
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string OfficeLocation { get => GetValue<string>(); set => SetValue(value); }

        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal override async Task RestToGraphMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }
        #endregion

        #region Extension methods

        public async Task<ISharePointUser> AsSharePointUserAsync()
        {
            if (!IsPropertyAvailable(p => p.UserPrincipalName) || string.IsNullOrEmpty(UserPrincipalName))
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_Unsupported_SharePointUserOnGraph);
            }

            return await PnPContext.Web.EnsureUserAsync(UserPrincipalName).ConfigureAwait(false);
        }

        public ISharePointUser AsSharePointUser()
        {
            return AsSharePointUserAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}
