using System.Runtime.Serialization;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    public enum ShareType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "view")]
        View,
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "edit")]
        Edit
    }
}
