using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;

namespace PnP.Core.Provisioning.Model.AzureActiveDirectory
{
    /// <summary>
    /// Defines an AAD User
    /// </summary>
    public partial class User : BaseModel, IEquatable<User>
    {
        #region Public Members

        /// <summary>
        /// The Password Profile for the user
        /// </summary>
        public PasswordProfile PasswordProfile { get; set; }

        /// <summary>
        /// Declares whether the user's account is enabled or not
        /// </summary>
        public bool AccountEnabled { get; set; }

        /// <summary>
        /// The Display Name of the user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The Mail Nickname of the user
        /// </summary>
        public string MailNickname { get; set; }

        /// <summary>
        /// The Password Policies	for the user
        /// </summary>
        public string PasswordPolicies { get; set; }

        /// <summary>
        /// The UPN for the user
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// The Profile Photo for the user
        /// </summary>
        public string ProfilePhoto { get; set; }

        /// <summary>
        /// The Given Name for the user
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// The Surname for the user
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// The Job Title for the user
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// The Office Location for the user
        /// </summary>
        public string OfficeLocation { get; set; }

        /// <summary>
        /// The Preferred Language for the user
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// The Mobile Phone for the user
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// The Mobile Phone for the user
        /// </summary>
        public string UsageLocation { get; set; }

        /// <summary>
        /// Collection of user's licenses
        /// </summary>
        public UserLicenseCollection Licenses { get; private set; }

        #endregion

        #region Constructors

        public User() : base()
        {
            this.Licenses = new UserLicenseCollection(this.ParentTemplate);
        }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|",
                PasswordProfile.GetHashCode(),
                AccountEnabled.GetHashCode(),
                DisplayName?.GetHashCode() ?? 0,
                MailNickname?.GetHashCode() ?? 0,
                PasswordPolicies?.GetHashCode() ?? 0,
                UserPrincipalName?.GetHashCode() ?? 0,
                ProfilePhoto?.GetHashCode() ?? 0,
                Licenses.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0))
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with User class
        /// </summary>
        /// <param name="obj">Object that represents User</param>
        /// <returns>Checks whether object is User class</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is User))
            {
                return (false);
            }
            return (Equals((User)obj));
        }

        /// <summary>
        /// Compares User object based on PasswordProfile, AccountEnabled, DisplayName, MailNickname, 
        /// PasswordPolicies, UserPrincipalName, ProfilePhoto, and Licenses
        /// </summary>
        /// <param name="other">User Class object</param>
        /// <returns>true if the User object is equal to the current object; otherwise, false.</returns>
        public bool Equals(User other)
        {
            if (other == null)
            {
                return (false);
            }

            return 
                (this.PasswordProfile != null ? this.PasswordProfile.Equals(other.PasswordProfile) : this.PasswordProfile == null && other.PasswordProfile == null ? true : false) &&
                this.AccountEnabled == other.AccountEnabled &&
                this.DisplayName == other.DisplayName &&
                this.MailNickname == other.MailNickname &&
                this.PasswordPolicies == other.PasswordPolicies &&
                this.UserPrincipalName == other.UserPrincipalName &&
                this.ProfilePhoto == other.ProfilePhoto &&
                this.Licenses.DeepEquals(other.Licenses);
        }

        #endregion
    }
}
