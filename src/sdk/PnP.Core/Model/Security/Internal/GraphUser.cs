using PnP.Core.Services;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "users/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class GraphUser
    {
        public GraphUser()
        {
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal override async Task RestToGraphMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        #region Extension methods

        public async Task<ISharePointUser> AsSharePointUserAsync()
        {
            if (!this.IsPropertyAvailable(p=>p.UserPrincipalName) || string.IsNullOrEmpty(UserPrincipalName))
            {
                throw new ClientException(ErrorType.Unsupported, "You can't call AsSharePointUserAsync on a Graph user without the UserPrincipalProperty requested and populated");
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
