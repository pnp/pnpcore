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
        bool AcceptMessagesOnlyFromSendersOrMembers { get; set; }
        string AccessType { get; set; }
        string AllowAccessFromUnmanagedDevice { get; set; }
        bool AutoDelete { get; set; }
        bool BlockDelete { get; set; }
        bool BlockEdit { get; set; }
        bool ContainsSiteLabel { get; set; }
        string DisplayName { get; set; }
        string EncryptionRMSTemplateId { get; set; }
        bool HasRetentionAction { get; set; }
        bool IsEventTag { get; set; }
        string Notes { get; set; }
        bool RequireSenderAuthenticationEnabled { get; set; }
        string ReviewerEmail { get; set; }
        string SharingCapabilities { get; set; }
        bool SuperLock { get; set; }
        int TagDuration { get; set; }
        Guid TagId { get; set; }
        string TagName { get; set; }
        string TagRetentionBasedOn { get; set; }
    }
#pragma warning restore CS1591
}