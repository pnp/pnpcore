using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class GraphPermission : BaseDataModel<IGraphPermission>, IGraphPermission
    {
        #region Constructor
        public GraphPermission(PnPContext context, IDataModelParent parent)
        {
            PnPContext = context;
            Parent = parent;
        }
        #endregion

        #region Properties

        public string Id { get; set; }

        public ISharePointIdentitySet GrantedToV2 { get; set; }

        public List<ISharePointIdentitySet> GrantedToIdentitiesV2 { get; set; }

        public ISharingInvitation Invitation { get; set; }

        public ISharingLink Link { get; set; }

        public List<PermissionRole> Roles { get; set; }

        public string ShareId { get; set; }

        public DateTimeOffset ExpirationDateTime { get; set; }

        public bool HasPassword { get; set; }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods

        public async Task DeletePermissionAsync()
        {
            var parent = (IFile)Parent;
            var apiCall = new ApiCall($"sites/{parent.SiteId}/drive/items/{parent.UniqueId}/permissions/{Id}", ApiType.Graph);
            var response = await RawRequestAsync(apiCall, HttpMethod.Delete).ConfigureAwait(false);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception("Error occured");
            }
        }

        #endregion
    }
}
