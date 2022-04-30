using System;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(ShareLinkRequestOptions))]
    public interface IShareLinkRequestOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public ShareType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ShareScope Scope { get; set; }
    }
}
