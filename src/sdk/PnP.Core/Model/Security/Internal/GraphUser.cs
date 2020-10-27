using PnP.Core.Services;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "users/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class GraphUser: BaseDataModel<IGraphUser>, IGraphUser
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
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
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
            if (!this.IsPropertyAvailable(p=>p.UserPrincipalName) || string.IsNullOrEmpty(UserPrincipalName))
            {
                throw new ClientException(ErrorType.Unsupported, 
                    PnPCoreResources.Exception_Unsupported_SharePointUserOnGraph);
            }

            // Build the API call to ensure this graph user as a SharePoint User in this site collection
            var parameters = new
            {
                logonName = $"i:0#.f|membership|{UserPrincipalName}"
            }.AsExpando();

            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));

            var apiCall = new ApiCall("_api/Web/EnsureUser", ApiType.SPORest, body);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext
            };

            await sharePointUser.RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return sharePointUser;
        }

        public ISharePointUser AsSharePointUser()
        {
            return AsSharePointUserAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}
