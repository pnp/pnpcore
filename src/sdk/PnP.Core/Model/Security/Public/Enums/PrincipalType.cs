using System;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Specifies the type of a principal.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
#pragma warning disable CA1714 // Flags enums should have plural names
    public enum PrincipalType
#pragma warning restore CA1714 // Flags enums should have plural names
    {
        /// <summary>
        /// Enumeration whose value specifies no principal type. Value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enumeration whose value specifies a user as the principal type. Value = 1.
        /// </summary>
        User = 1,
        /// <summary>
        /// Enumeration whose value specifies a distribution list as the principal type. Value = 2.
        /// </summary>
        DistributionList = 2,
        /// <summary>
        /// Enumeration whose value specifies a security group as the principal type. Value = 4.
        /// </summary>
        SecurityGroup = 4,
        /// <summary>
        /// Enumeration whose value specifies a group (2) as the principal type. Value = 8.
        /// </summary>
        SharePointGroup = 8,
        /// <summary>
        /// Enumeration whose value specifies all principal types. Value = 15.
        /// </summary>
        All = 15
    }
}
