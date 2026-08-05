namespace PnP.Core.Provisioning.Services.Core.CSOM
{
    /// <summary>
    /// CSOM server type ids - the GUIDs a <c>StaticMethod</c> or <c>Constructor</c> object path is
    /// addressed by.
    /// </summary>
    internal static class CsomTypeIds
    {
        #region Taxonomy - Microsoft.SharePoint.Client.Taxonomy

        /// <summary>SP.Taxonomy.TaxonomySession</summary>
        internal const string TaxonomySession = "{981cbc68-9edc-4f8d-872f-71146fcbb84f}";

        /// <summary>SP.Taxonomy.TermStore</summary>
        internal const string TermStore = "{9d8a8884-b1dc-4dbc-81c5-ddea8ad3184c}";

        /// <summary>SP.Taxonomy.TermGroup</summary>
        internal const string TermGroup = "{65d76872-0b65-42de-8ebd-d76f6d3491c6}";

        /// <summary>SP.Taxonomy.TermSet</summary>
        internal const string TermSet = "{e26feb13-2940-4db9-a52b-12b160113a80}";

        /// <summary>SP.Taxonomy.TermSetItem - the base of both TermSet and Term, and where
        /// <c>CreateTerm</c>, <c>ReuseTerm</c> and <c>CustomSortOrder</c> live.</summary>
        internal const string TermSetItem = "{a99e4a8f-010b-4e56-9b29-b7bd6ec51263}";

        /// <summary>SP.Taxonomy.Term</summary>
        internal const string Term = "{5b8c81b7-7cd2-40dc-8525-5eca12a4eb73}";

        /// <summary>SP.Taxonomy.Label</summary>
        internal const string Label = "{81503ae1-8747-4684-a172-163c7e009ef9}";

        #endregion

        #region Lists - Microsoft.SharePoint.Client

        /// <summary>
        /// SP.ListCreationInformation - the value object <c>Web.Lists.Add</c> takes.
        /// </summary>
        internal const string ListCreationInformation = "{e247b7fc-095e-4ea4-a4c9-c5d373723d8c}";

        /// <summary>SP.ViewCreationInformation</summary>
        internal const string ViewCreationInformation = "{a3547807-7266-42f3-b055-afa6e840e458}";

        /// <summary>SP.ListItemCreationInformation</summary>
        internal const string ListItemCreationInformation = "{54cdbee5-0897-44ac-829f-411557fa11be}";

        #endregion

        #region Publishing - Microsoft.SharePoint.Client.Publishing

        /// <summary>SP.Publishing.SiteImageRenditions</summary>
        internal const string SiteImageRenditions = "{324675a4-aa0d-47db-a937-c2e5dc53457e}";

        /// <summary>SP.Publishing.ImageRendition</summary>
        internal const string ImageRendition = "{cb63161f-1f15-446f-9ba9-af89ae03cd45}";

        #endregion

        #region Workflow services - Microsoft.SharePoint.Client.WorkflowServices

        /// <summary>SP.WorkflowServices.WorkflowServicesManager</summary>
        internal const string WorkflowServicesManager = "{4ccc7f0e-bf7e-4477-999c-6458a73d0039}";

        /// <summary>SP.WorkflowServices.WorkflowDeploymentService</summary>
        internal const string WorkflowDeploymentService = "{3573a52f-3a27-4700-a08e-822c191c2c5d}";

        /// <summary>SP.WorkflowServices.WorkflowSubscriptionService</summary>
        internal const string WorkflowSubscriptionService = "{fc956693-2419-4950-8963-52ebc3e46501}";

        /// <summary>SP.WorkflowServices.WorkflowInstanceService</summary>
        internal const string WorkflowInstanceService = "{71252277-2470-4022-bcaf-c4657aa118c3}";

        /// <summary>SP.WorkflowServices.WorkflowDefinition</summary>
        internal const string WorkflowDefinition = "{60320d36-4b4d-4bac-a092-8f8b5610edcd}";

        /// <summary>SP.WorkflowServices.WorkflowSubscription</summary>
        internal const string WorkflowSubscription = "{d185ede6-c3c3-4d37-9e8c-2382deb37708}";

        #endregion

        #region Web parts - Microsoft.SharePoint.Client.WebParts

        /// <summary>SP.WebParts.LimitedWebPartManager</summary>
        internal const string LimitedWebPartManager = "{ac641ade-62df-49c9-af8e-abda6278e920}";

        /// <summary>SP.WebParts.WebPartDefinition</summary>
        internal const string WebPartDefinition = "{44bf1024-6127-432a-8e3d-fb317fb4541e}";

        /// <summary>SP.WebParts.WebPart</summary>
        internal const string WebPart = "{612a6bd9-6c99-43c9-813a-8d7e19702118}";

        #endregion

        #region Core - Microsoft.SharePoint.Client

        /// <summary>SP.Audit</summary>
        internal const string Audit = "{1307502c-2a94-4c1e-8ba6-30da4b0391f1}";

        /// <summary>SP.UserResource</summary>
        internal const string UserResource = "{2b2affeb-3ccd-4996-9864-211c960e647c}";

        /// <summary>SP.Utilities.Utility</summary>
        internal const string Utility = "{16f43e7e-bf35-475d-b677-9dc61e549339}";

        /// <summary>
        /// SP.RegionalSettings.
        /// </summary>
        internal const string RegionalSettings = "{84c424a9-a1d6-46ba-8398-c46257ecd25b}";

        /// <summary>SP.TimeZone</summary>
        internal const string TimeZone = "{5519d02c-ce37-4b91-b61d-a1cefe0fc85e}";

        #endregion

        #region Information policy - Microsoft.Office.Client.Policy

        /// <summary>
        /// SP.InformationPolicy.ProjectPolicy.
        /// </summary>
        internal const string ProjectPolicy = "{ec5e0a70-0cc3-408f-a4dc-1bb3495aac75}";

        #endregion

        #region Tenant administration - Microsoft.Online.SharePoint.TenantAdministration

        /// <summary>
        /// Microsoft.Online.SharePoint.TenantAdministration.Tenant.
        /// </summary>
        internal const string Tenant = "{268004ae-ef6b-4e9b-8425-127220d84719}";

        #endregion
    }
}
