using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;

namespace PnP.Core.Provisioning.Model.AzureActiveDirectory
{
    /// <summary>
    /// Defines an AAD User
    /// </summary>
    public partial class UserLicense : BaseModel, IEquatable<UserLicense>
    {
        #region Public Members

        /// <summary>
        /// Defines the SKU of the license
        /// </summary>
        public string SkuId { get; set; }

        /// <summary>
        /// List of disabled plans
        /// </summary>
        public string[] DisabledPlans { get; set; }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|",
                SkuId.GetHashCode(),
                DisabledPlans.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0))
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with UserLicense class
        /// </summary>
        /// <param name="obj">Object that represents UserLicense</param>
        /// <returns>Checks whether object is UserLicense class</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is UserLicense))
            {
                return (false);
            }
            return (Equals((UserLicense)obj));
        }

        /// <summary>
        /// Compares UserLicense object based on SkuId and Disabled Plans
        /// </summary>
        /// <param name="other">UserLicense Class object</param>
        /// <returns>true if the UserLicense object is equal to the current object; otherwise, false.</returns>
        public bool Equals(UserLicense other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.SkuId == other.SkuId &&
                this.DisabledPlans.DeepEquals(other.DisabledPlans)
                );
        }

        #endregion
    }
}
