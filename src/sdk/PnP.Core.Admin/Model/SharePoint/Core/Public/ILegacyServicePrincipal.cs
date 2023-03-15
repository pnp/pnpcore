using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Service principal
    /// </summary>
    public interface ILegacyServicePrincipal
    {
        /// <summary>
        /// Azure App Id for this principal
        /// </summary>
        Guid AppId { get; }

        /// <summary>
        /// Identifier of the legacy principal
        /// </summary>
        string AppIdentifier { get; }

        /// <summary>
        /// Name of the legacy principal
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When does this principal expire
        /// </summary>
        DateTime ValidUntil { get; }
    }
}
