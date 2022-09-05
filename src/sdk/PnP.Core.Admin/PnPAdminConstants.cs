using System;

namespace PnP.Core.Admin
{
    /// <summary>
    /// Support class to provide all the constants for the admin operations
    /// </summary>
    internal static class PnPAdminConstants
    {
        // site templates

        /// <summary>
        /// Web template of a communication site
        /// </summary>
        internal const string CommunicationSiteTemplate = "SITEPAGEPUBLISHING#0";

        /// <summary>
        /// Web template of a group connected team site
        /// </summary>
        internal const string TeamSiteTemplate = "GROUP#0";

        /// <summary>
        /// Web template of a team site without a group
        /// </summary>
        internal const string TeamSiteWithoutGroupTemplate = "STS#3";

        // CSOM objects

        /// <summary>
        /// Identifier for the CSOM Tenant object
        /// </summary>
        internal const string CsomTenantObject = "{268004ae-ef6b-4e9b-8425-127220d84719}";

        // Communication site design ids

        /// <summary>
        /// Topic site design id for communication sites
        /// </summary>
        internal static Guid CommunicationSiteDesignTopic = Guid.Parse("96c933ac-3698-44c7-9f4a-5fd17d71af9e");

        /// <summary>
        /// Showcase site design id for communication sites
        /// </summary>
        internal static Guid CommunicationSiteDesignShowCase = Guid.Parse("6142d2a0-63a5-4ba0-aede-d9fefca2c767");

        /// <summary>
        /// Blank site design id for communication sites
        /// </summary>
        internal static Guid CommunicationSiteDesignBlank = Guid.Parse("f6cc5403-0d63-442e-96c0-285923709ffc");

    }
}
