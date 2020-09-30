using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.User", Uri = "_api/Web/GetUserById({Id})", LinqGet = "_api/Web/SiteUsers")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class SharePointUser
    {
        public SharePointUser()
        {
            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case nameof(PrincipalType): return JsonMappingHelper.ToEnum<PrincipalType>(input.JsonElement);
                }

                input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };
        }

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
