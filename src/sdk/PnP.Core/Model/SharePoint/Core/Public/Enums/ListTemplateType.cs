namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enumeration to define the list template types for SharePoint
    /// </summary>
    public enum ListTemplateType
    {
        /// <summary>
        /// InvalidType list type
        /// </summary>
        InvalidType = -1,

        /// <summary>
        /// NoListTemplate list type
        /// </summary>
        NoListTemplate = 0,

        /// <summary>
        /// GenericList list type
        /// </summary>
        GenericList = 100,

        /// <summary>
        /// DocumentLibrary list type
        /// </summary>
        DocumentLibrary = 101,

        /// <summary>
        /// Survey list type
        /// </summary>
        Survey = 102,

        /// <summary>
        /// Links list type
        /// </summary>
        Links = 103,

        /// <summary>
        /// Announcements list type
        /// </summary>
        Announcements = 104,

        /// <summary>
        /// Contacts list type
        /// </summary>
        Contacts = 105,

        /// <summary>
        /// Events list type
        /// </summary>
        Events = 106,

        /// <summary>
        /// Tasks list type
        /// </summary>
        Tasks = 107,

        /// <summary>
        /// DiscussionBoard list type
        /// </summary>
        DiscussionBoard = 108,

        /// <summary>
        /// PictureLibrary list type
        /// </summary>
        PictureLibrary = 109,

        /// <summary>
        /// DataSources list type
        /// </summary>
        DataSources = 110,

        /// <summary>
        /// WebTemplateCatalog list type
        /// </summary>
        WebTemplateCatalog = 111,

        /// <summary>
        /// UserInformation list type
        /// </summary>
        UserInformation = 112,

        /// <summary>
        /// WebPartCatalog list type
        /// </summary>
        WebPartCatalog = 113,

        /// <summary>
        /// ListTemplateCatalog list type
        /// </summary>
        ListTemplateCatalog = 114,

        /// <summary>
        /// XMLForm list type
        /// </summary>
        XMLForm = 115,

        /// <summary>
        /// MasterPageCatalog list type
        /// </summary>
        MasterPageCatalog = 116,

        /// <summary>
        /// NoCodeWorkflows list type
        /// </summary>
        NoCodeWorkflows = 117,

        /// <summary>
        /// WorkflowProcess list type
        /// </summary>
        WorkflowProcess = 118,

        /// <summary>
        /// WebPageLibrary list type
        /// </summary>
        WebPageLibrary = 119,

        /// <summary>
        /// CustomGrid list type
        /// </summary>
        CustomGrid = 120,

        /// <summary>
        /// SolutionCatalog list type
        /// </summary>
        SolutionCatalog = 121,

        /// <summary>
        /// NoCodePublic list type
        /// </summary>
        NoCodePublic = 122,

        /// <summary>
        /// ThemeCatalog list type
        /// </summary>
        ThemeCatalog = 123,

        /// <summary>
        /// DesignCatalog list type
        /// </summary>
        DesignCatalog = 124,

        /// <summary>
        /// AppDataCatalog list type
        /// </summary>
        AppDataCatalog = 125,

        /// <summary>
        /// AppFilesCatalog list type
        /// </summary>
        AppFilesCatalog = 126,

        /// <summary>
        /// DataConnectionLibrary list type
        /// </summary>
        DataConnectionLibrary = 130,

        /// <summary>
        /// WorkflowHistory list type
        /// </summary>
        WorkflowHistory = 140,

        /// <summary>
        /// GanttTasks list type
        /// </summary>
        GanttTasks = 150,

        /// <summary>
        /// HelpLibrary list type
        /// </summary>
        HelpLibrary = 151,

        /// <summary>
        /// AccessRequest list type
        /// </summary>
        AccessRequest = 160,

        /// <summary>
        /// PromotedLinks list type
        /// </summary>
        PromotedLinks = 170,

        /// <summary>
        /// TasksWithTimelineAndHierarchy list type
        /// </summary>
        TasksWithTimelineAndHierarchy = 171,

        /// <summary>
        /// MaintenanceLogs list type
        /// </summary>
        MaintenanceLogs = 175,

        /// <summary>
        /// Meetings list type
        /// </summary>
        Meetings = 200,

        /// <summary>
        /// Agenda list type
        /// </summary>
        Agenda = 201,

        /// <summary>
        /// MeetingUser list type
        /// </summary>
        MeetingUser = 202,

        /// <summary>
        /// Decision list type
        /// </summary>
        Decision = 204,

        /// <summary>
        /// MeetingObjective list type
        /// </summary>
        MeetingObjective = 207,

        /// <summary>
        /// TextBox list type
        /// </summary>
        TextBox = 210,

        /// <summary>
        /// ThingsToBring list type
        /// </summary>
        ThingsToBring = 211,

        /// <summary>
        /// HomePageLibrary list type
        /// </summary>
        HomePageLibrary = 212,

        /// <summary>
        /// Posts list type
        /// </summary>
        Posts = 301,

        /// <summary>
        /// Comments list type
        /// </summary>
        Comments = 302,

        /// <summary>
        /// Categories list type
        /// </summary>
        Categories = 303,

        /// <summary>
        /// App catalog
        /// </summary>
        CorporateCatalog = 330,

        /// <summary>
        /// Apps for Office
        /// </summary>
        OfficeExtensionCatalog = 332,

        /// <summary>
        /// App requests
        /// </summary>
        AppRequestsList = 333,

        /// <summary>
        /// Site Collection App catalog
        /// </summary>
        SiteCollectionAppCatalog = 336,

        /// <summary>
        /// App catalog tenant wide extensions
        /// </summary>
        CorporateCatalogTenantWideExtensions = 337,

        /// <summary>
        /// Access Services USys appplication log
        /// </summary>
        AccSrvUSysAppLog = 398,

        /// <summary>
        /// Access Services MSys ASO
        /// </summary>
        AccSrvMSysAso = 399,

        /// <summary>
        /// Facility list type
        /// </summary>
        Facility = 402,

        /// <summary>
        /// Whereabouts list type
        /// </summary>
        Whereabouts = 403,

        /// <summary>
        /// CallTrack list type
        /// </summary>
        CallTrack = 404,

        /// <summary>
        /// Circulation list type
        /// </summary>
        Circulation = 405,

        /// <summary>
        /// Timecard list type
        /// </summary>
        Timecard = 420,

        /// <summary>
        /// Holidays list type
        /// </summary>
        Holidays = 421,

        /// <summary>
        /// Report list template type
        /// </summary>
        ReportListTemplate = 433,

        /// <summary>
        /// BI Center dashboard library
        /// </summary>
        BICenterDashboardsLibrary = 480,

        /// <summary>
        /// IMEDic list type
        /// </summary>
        IMEDic = 499,

        /// <summary>
        /// Community site categories list type
        /// </summary>
        CategoriesList = 500,

        /// <summary>
        /// Visio Repository Site Process Diagrams list
        /// </summary>
        VisioProcessRepositoryList = 506,

        /// <summary>
        /// MicroBlogList (MicroFeed)
        /// </summary>
        MySiteMicroBlogList = 544,

        /// <summary>
        /// ExternalList list type
        /// </summary>
        ExternalList = 600,

        /// <summary>
        /// MySiteDocumentLibrary list type
        /// </summary>
        MySiteDocumentLibrary = 700,

        /// <summary>
        /// Pages library in a classic publishing site
        /// </summary>
        PublishingPagesLibrary = 850,

        /// <summary>
        /// Asset library
        /// </summary>
        AssetLibrary = 851,

        /// <summary>
        /// Community site membership list type
        /// </summary>
        MembershipList = 880,

        /// <summary>
        /// IssueTracking list type
        /// </summary>
        IssueTracking = 1100,

        /// <summary>
        /// AdminTasks list type
        /// </summary>
        AdminTasks = 1200,

        /// <summary>
        /// HealthRules list type
        /// </summary>
        HealthRules = 1220,

        /// <summary>
        /// HealthReports list type
        /// </summary>
        HealthReports = 1221,

        /// <summary>
        /// DeveloperSiteDraftApps list type
        /// </summary>
        DeveloperSiteDraftApps = 1230,

        /// <summary>
        /// In place records list type
        /// </summary>
        InPlaceRecords = 1302,

        /// <summary>
        /// Ediscovery sources
        /// </summary>
        EDiscoverySources = 1305,

        /// <summary>
        /// Ediscovery source instances
        /// </summary>
        EDiscoverySourceInstances = 1306,

        /// <summary>
        /// Ediscovery source groups
        /// </summary>
        EDiscoverySourceGroups = 1307,

        /// <summary>
        /// Ediscovery custodians
        /// </summary>
        EDiscoveryCustodians = 1308,

        /// <summary>
        /// Ediscovery custom queries
        /// </summary>
        EDiscoveryCustomQueries = 1309,

        /// <summary>
        /// Ediscovery exports
        /// </summary>
        EDiscoveryExports = 1310,

        /// <summary>
        /// Model Library for Content Center Site Template. Value = 1328
        /// </summary>
        ContentCenterModelLibrary = 1328,
        
        /// <summary>
        /// ContentCenterPrimeLibrary list type
        /// </summary>
        ContentCenterPrimeLibrary = 1329,

        /// <summary>
        /// ContentCenterSampleLibrary list type
        /// </summary>
        ContentCenterSampleLibrary = 1330,

        /// <summary>
        /// ContentCenterModelUsage list type
        /// </summary>
        ContentCenterModelUsage = 1331,

        /// <summary>
        /// ContentCenterExplanation list type
        /// </summary>
        ContentCenterExplanation = 1332,

        /// <summary>
        /// AccessApp list type
        /// </summary>
        AccessApp = 3100,

        /// <summary>
        /// AlchemyMobileForm list type
        /// </summary>
        AlchemyMobileForm = 3101,

        /// <summary>
        /// AlchemyApprovalWorkflow list type
        /// </summary>
        AlchemyApprovalWorkflow = 3102,

        /// <summary>
        /// SharingLinks list type
        /// </summary>
        SharingLinks = 3300,

        /// <summary>
        /// HashtagStore list type
        /// </summary>
        HashtagStore = 3400,

        /// <summary>
        /// RecipesTable list type
        /// </summary>
        RecipesTable = 3410,

        /// <summary>
        /// FormulasTable list type
        /// </summary>
        FormulasTable = 3411,

        /// <summary>
        /// WebTemplateExtensionsList list type
        /// </summary>
        WebTemplateExtensionsList = 3415,

        /// <summary>
        /// ItemReferenceCollection list type
        /// </summary>
        ItemReferenceCollection = 3500,

        /// <summary>
        /// ItemReferenceReference list type
        /// </summary>
        ItemReferenceReference = 3501,

        /// <summary>
        /// ItemReferenceReferenceCollection list type
        /// </summary>
        ItemReferenceReferenceCollection = 3502
    }
}
