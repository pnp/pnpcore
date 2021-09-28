using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal static class BuiltInFields
    {
        private static readonly List<string> internalFields = new List<string>()
        {
            "_CheckinComment",
            "_CommentCount",
            "_CommentFlags",
            "_ComplianceFlags",
            "_ComplianceTag",
            "_ComplianceTagUserId",
            "_ComplianceTagWrittenTime",
            "_Dirty",
            "_DisplayName",
            "_HasEncryptedContent",
            "_IpLabelAssignmentMethod",
            "_IpLabelId",
            "_LikeCount",
            "_ListSchemaVersion",
            "_Parsable",
            "_RmsTemplateId",
            "_StubFile",
            "_VirusInfo",
            "_VirusStatus",
            "_VirusVendorID",
            "A2ODMountCount",
            "AccessPolicy",
            "AppAuthor",
            "AppEditor",
            "BSN",
            "CheckedOutTitle",
            "CheckedOutUserId",
            "ContentVersion",
            "Created_x0020_Date",
            "DocConcurrencyNumber",
            "File_x0020_Size",
            "FileDirRef",
            "FileRef",
            "FolderChildCount",
            "FSObjType",
            "IsCheckedoutToLocal",
            "ItemChildCount",
            "Last_x0020_Modified",
            "MetaInfo",
            "NoExecute",
            "OriginatorId",
            "ParentLeafName",
            "ParentUniqueId",
            "ParentVersionString",
            "ProgId",
            "Restricted",
            "ScopeId",
            "SMLastModifiedDate",
            "SMTotalFileCount",
            "SMTotalFileStreamSize",
            "SMTotalSize",
            "SortBehavior",
            "StreamHash",
            "SyncClientId",
            "UniqueId",
            "VirusStatus",
        };

        internal static bool Contains(string fieldName)
        {
            return internalFields.Contains(fieldName);
        }

    }
}
