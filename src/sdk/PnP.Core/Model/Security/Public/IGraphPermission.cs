using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(GraphPermission))]
    public interface IGraphPermission : IDataModel<IGraphPermission>
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ISharePointIdentitySet GrantedToV2 { get; }

        /// <summary>
        /// 
        /// </summary>
        List<ISharePointIdentitySet> GrantedToIdentitiesV2 { get; }

        // Currently not needed
        //IItemReference InheritedFrom { get; }

        /// <summary>
        /// 
        /// </summary>
        ISharingInvitation Invitation { get; }

        /// <summary>
        /// 
        /// </summary>
        ISharingLink Link { get; }

        /// <summary>
        /// 
        /// </summary>
        List<PermissionRole> Roles { get; }

        /// <summary>
        /// 
        /// </summary>
        string ShareId { get; }

        /// <summary>
        /// 
        /// </summary>
        DateTimeOffset ExpirationDateTime { get; }

        /// <summary>
        /// 
        /// </summary>
        bool HasPassword { get; }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        Task DeletePermissionAsync();

        #endregion
    }
}
