using PnP.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.User", Uri = "_api/Web/GetUserById({Id})", LinqGet = "_api/Web/SiteUsers")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class SharePointUser: BaseDataModel<ISharePointUser>, ISharePointUser
    {
        #region Construction
        public SharePointUser()
        {
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("AadObjectId", JsonPath = "NameId")]
        public string AadObjectId { get => GetValue<string>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public string Department { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = int.Parse(value.ToString()); }
        #endregion

        #region Extension methods
        public async Task<IGraphUser> AsGraphUserAsync()
        {
            if (!this.IsPropertyAvailable(p => p.PrincipalType) || (PrincipalType != PrincipalType.User && PrincipalType != PrincipalType.SecurityGroup))
            {
                throw new ClientException(ErrorType.Unsupported, 
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePoint);
            }

            if (!this.IsPropertyAvailable(p => p.AadObjectId))
            {
                // Try loading the current user again with the aadobjectid property also loaded next all other user properties
                var apiCall = new ApiCall($"_api/Web/GetUserById({Id})?$select=id,ishiddeninUI,loginname,title,principaltype,aadobjectid,email,expiration,IsEmailAuthenticationGuestUser,IsShareByEmailGuestUser,userprincipalname,issiteadmin,userid", ApiType.SPORest);
                await this.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            }

            // Check again for principal type
            if (PrincipalType != PrincipalType.User && PrincipalType != PrincipalType.SecurityGroup)
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePoint);
            }

            if (string.IsNullOrEmpty(AadObjectId))
            {
                throw new ClientException(ErrorType.Unsupported, 
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePointNoGraphId);
            }

            GraphUser graphUser = new GraphUser
            {
                PnPContext = PnPContext,
                Id = AadObjectId,
                UserPrincipalName = UserPrincipalName,
                Mail = Mail,                
            };

            // Populate the graph metadata
            graphUser.AddMetadata(PnPConstants.MetaDataGraphType, "#microsoft.graph.user");
            graphUser.AddMetadata(PnPConstants.MetaDataGraphId, AadObjectId);

            return graphUser; 
        }

        public IGraphUser AsGraphUser()
        {
            return AsGraphUserAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}
