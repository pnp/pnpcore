using System.Runtime.Serialization;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    public enum ShareScope
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "anonymous")]
        Anonymous,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "organization")]
        Organization,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "users")]
        Users
    }
}
