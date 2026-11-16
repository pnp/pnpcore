using System;

namespace PnP.Core.Modernization.Entities
{
    /// <summary>
    /// Class used to cache a user that was resolved via EnsureUser
    /// </summary>
    [Serializable]
    public class ResolvedUser
    {
        /// <summary>
        /// Loginname of the user
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// Id of the user
        /// </summary>
        public int Id { get; set; }
    }
}
