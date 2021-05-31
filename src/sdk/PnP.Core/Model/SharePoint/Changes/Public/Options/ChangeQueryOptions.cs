namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines a query that is performed against the change log.
    /// </summary>
    public class ChangeQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeQueryOptions"/> class.
        /// </summary>
        public ChangeQueryOptions()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeQueryOptions"/> class.
        /// </summary>
        /// <param name="allChangeObjectTypes">If <c>true</c>, get changes for all change object types.</param>
        /// <param name="allChangeTypes">If <c>true</c>, get all change types.</param>
        public ChangeQueryOptions(bool allChangeObjectTypes, bool allChangeTypes)
        {
            if (allChangeObjectTypes)
            {
                Item = true;
                List = true;
                Web = true;
                Site = true;
                File = true;
                Folder = true;
                Alert = true;
                User = true;
                Group = true;
                ContentType = true;
                Field = true;
                SecurityPolicy = true;
                View = true;
            }

            if (allChangeTypes)
            {
                Add = true;
                Update = true;
                DeleteObject = true;
                Rename = true;
                Move = true;
                Restore = true;
                RoleDefinitionAdd = true;
                RoleDefinitionDelete = true;
                RoleDefinitionUpdate = true;
                RoleAssignmentAdd = true;
                RoleAssignmentDelete = true;
                GroupMembershipAdd = true;
                GroupMembershipDelete = true;
                SystemUpdate = true;
                Navigation = true;
            }
        }

        /// <summary>
        /// Specifies whether activity is included in the query.
        /// </summary>
        public bool Activity { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether add changes are included in the query.
        /// </summary>
        public bool Add { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to alerts are included in the query.
        /// </summary>
        public bool Alert { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the end date and end time for changes that are returned through the query.
        /// </summary>
        public IChangeToken ChangeTokenEnd { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the start date and start time for changes that are returned through the query.
        /// Changes after this change token are returned.
        /// </summary>
        public IChangeToken ChangeTokenStart { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to content types are included in the query.
        /// </summary>
        public bool ContentType { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether delete changes are included in the query.
        /// </summary>
        public bool DeleteObject { get; set; }

        /// <summary>
        /// The maximum number of items to return (defaults to 1000).
        /// </summary>
        public long FetchLimit { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to site columns are included in the query.
        /// </summary>
        public bool Field { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to files are included in the query.
        /// </summary>
        public bool File { get; set; }

        /// <summary>
        /// Gets or sets value that specifies whether changes to folders are included in the query.
        /// </summary>
        public bool Folder { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to groups are included in the query.
        /// </summary>
        public bool Group { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether adding users to groups is included in the query.
        /// </summary>
        public bool GroupMembershipAdd { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether deleting users from the groups is included in the query.
        /// </summary>
        public bool GroupMembershipDelete { get; set; }

        /// <summary>
        /// Ignore errors when calling GetChanges if the caller's start ChangeToken
        /// is not found in the database (exact match) or if there are no changes in
        /// the database.
        /// </summary>
        public bool IgnoreStartTokenNotFoundError { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether general changes to list items are included in the query.
        /// </summary>
        public bool Item { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether to order the results by Modified By date, most recent first.
        /// </summary>
        public bool LatestFirst { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to lists are included in the query.
        /// </summary>
        public bool List { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether move changes are included in the query.
        /// </summary>
        public bool Move { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to the navigation structure of a site collection are included in the query.
        /// </summary>
        public bool Navigation { get; set; }

        /// <summary>
        /// specifies whether we return all the change logs for folder scoped query.
        /// The flag will only work for Folder query. If true, all changes in the current folder
        /// and all sub folders will be returned. Otherwise, only changes in the current folder
        /// will be returned.
        /// </summary>
        public bool RecursiveAll { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether renaming changes are included in the query.
        /// </summary>
        public bool Rename { get; set; }

        /// <summary>
        /// Specifies whether we return log with security trimming. If true, we'll return the events with security trimming.
        /// </summary>
        public bool RequireSecurityTrim { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether restoring items from the recycle bin or from backups is included in the query.
        /// </summary>
        public bool Restore { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether adding role assignments is included in the query.
        /// </summary>
        public bool RoleAssignmentAdd { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether deleting role assignments is included in the query.
        /// </summary>
        public bool RoleAssignmentDelete { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether adding role definitions is included in the query.
        /// </summary>
        public bool RoleDefinitionAdd { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether deleting role definitions is included in the query.
        /// </summary>
        public bool RoleDefinitionDelete { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether modifying role definitions is included in the query.
        /// </summary>
        public bool RoleDefinitionUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether modifications to security policies are included in the query.
        /// </summary>
        public bool SecurityPolicy { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to site collections are included in the query.
        /// </summary>
        public bool Site { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether updates made using the item SystemUpdate method are included in the query.
        /// </summary>
        public bool SystemUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether update changes are included in the query.
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to users are included in the query.
        /// </summary>
        public bool User { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to views are included in the query.
        /// </summary>
        public bool View { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether changes to Web sites are included in the query.
        /// </summary>
        public bool Web { get; set; }
    }
}
