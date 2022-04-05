namespace PnP.Core.Model.SharePoint
{
    internal sealed class CopyMigrationOptions : ICopyMigrationOptions
    {
        public bool AllowSchemaMismatch { get; set; }
        public bool AllowSmallerVersionLimitOnDestination { get; set; }
        public bool IgnoreVersionHistory { get; set; }
        public bool IsMoveMode { get; set; }
        public string TypeId { get; set; }
        public SPMigrationNameConflictBehavior NameConflictBehavior { get; set; }
        public bool BypassSharedLock { get; set; }
        public string[] ClientEtags { get; set; }
        public bool MoveButKeepSource { get; set; }
        public bool ExcludeChildren { get; set; }
    }
}
