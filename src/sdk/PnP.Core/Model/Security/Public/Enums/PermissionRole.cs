using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    public enum PermissionRole
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value="read")]
        Read,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value="write")]
        Write,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "owner")]
        Owner
    }
}
