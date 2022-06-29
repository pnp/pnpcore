using PnP.Core.Services;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "users/{GraphId}")]
    internal sealed class GraphUser : BaseDataModel<IGraphUser>, IGraphUser
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

        public async Task SendMailAsync(MailOptions mailOptions)
        {
            if (!await PnPContext.AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false))
            {
                throw new MicrosoftGraphServiceException(PnPCoreResources.Exception_SendMailDelegated);
            }

            MailHandler.CheckErrors(mailOptions);

            var body = MailHandler.GetMailBody(mailOptions);

            var apiCall = new ApiCall($"users/{Id}/sendMail", ApiType.Graph, JsonSerializer.Serialize(body, PnPConstants.JsonSerializer_IgnoreNullValues));

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SendMail(MailOptions mailOptions)
        {
            SendMailAsync(mailOptions).GetAwaiter().GetResult();
        }
        #endregion
    }
}