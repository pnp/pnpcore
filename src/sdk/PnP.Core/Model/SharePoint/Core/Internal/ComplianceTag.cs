using System;

namespace PnP.Core.Model.SharePoint
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class ComplianceTag : IComplianceTag
    {
        public bool AcceptMessagesOnlyFromSendersOrMembers { get; set; }
        public string AccessType { get; set; }
        public string AllowAccessFromUnmanagedDevice { get; set; }

        public bool AutoDelete { get; set; }

        public bool BlockDelete { get; set; }

        public bool BlockEdit { get; set; }

        public bool ContainsSiteLabel { get; set; }

        public string DisplayName { get; set; }
        public string EncryptionRMSTemplateId { get; set; }

        public bool HasRetentionAction { get; set; }

        public bool IsEventTag { get; set; }
        public string Notes { get; set; }

        public bool RequireSenderAuthenticationEnabled { get; set; }

        public string ReviewerEmail { get; set; }

        public string SharingCapabilities { get; set; }

        public bool SuperLock { get; set; }

        public Guid TagId { get; set; }

        public string TagName { get; set; }

        public string TagRetentionBasedOn { get; set; }

        public int TagDuration { get; set; }
    }
}