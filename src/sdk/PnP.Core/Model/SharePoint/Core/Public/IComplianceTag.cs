using System;

namespace PnP.Core.Model.SharePoint
{

    /// <summary>
    /// Public interface to define a ComplianceTag / Retention label object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ComplianceTag))]
#pragma warning disable CS1591
    public interface IComplianceTag
    {
        /// <summary>
        /// Specifies AcceptMessagesOnlyFromSendersOrMembers for the tag
        /// </summary>
        bool AcceptMessagesOnlyFromSendersOrMembers { get; }

        /// <summary>
        /// Specifies AccessType for the tag
        /// </summary>
        string AccessType { get; }

        /// <summary>
        /// Specifies AllowAccessFromUnmanagedDevice for the tag
        /// </summary>
        string AllowAccessFromUnmanagedDevice { get; }

        /// <summary>
        /// Specifies "Will we auto Delete after the Retention Period passed"
        /// </summary>
        bool AutoDelete { get; }

        /// <summary>
        /// Specifies ComplianceTag BlockDelete
        /// </summary>
        bool BlockDelete { get; }

        /// <summary>
        /// Specifies ComplianceTag Enable Block edits
        /// </summary>
        bool BlockEdit { get; }

        /// <summary>
        /// Specifies whether this tag contains siteLabeling
        /// </summary>
        bool ContainsSiteLabel { get; }

        /// <summary>
        /// Specifies the Display name for the tag in UI
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Specifies the RMS Template id for the tag
        /// </summary>
        string EncryptionRMSTemplateId { get; }

        /// <summary>
        /// Specifies whether this tag has retention action
        /// </summary>
        bool HasRetentionAction { get;}

        /// <summary>
        /// Specifies if this is an event tag
        /// </summary>
        bool IsEventTag { get; }

        /// <summary>
        /// Specifies  notes for the tag
        /// </summary>
        string Notes { get; }

        /// <summary>
        /// Specifies RequireSenderAuthenticationEnabled for the tag
        /// </summary>
        bool RequireSenderAuthenticationEnabled { get; }

        /// <summary>
        /// Specifies  ReviewerEmail for the tag
        /// </summary>
        string ReviewerEmail { get; }

        /// <summary>
        /// Specifies SharingCapabilities for the tag
        /// </summary>
        string SharingCapabilities { get; }

        /// <summary>
        /// Specifies if this is an event tag
        /// </summary>
        bool SuperLock { get; }

        /// <summary>
        /// Specifies ComplianceTag TagDuration 
        /// </summary>
        int TagDuration { get; }

        /// <summary>
        /// Specifies ComplianceTag ID
        /// </summary>
        Guid TagId { get; }

        /// <summary>
        /// Specifies ComplianceTag Name
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Specifies ComplianceTag Retention Based on Field
        /// </summary>
        string TagRetentionBasedOn { get; }
    }
#pragma warning restore CS1591
}