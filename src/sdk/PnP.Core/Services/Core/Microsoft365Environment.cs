namespace PnP.Core.Services
{
    /// <summary>
    /// Microsoft 365 environments
    /// </summary>
    public enum Microsoft365Environment
    {
        /// <summary>
        /// Production environment
        /// </summary>
        Production = 0,

        /// <summary>
        /// Pre-production environment
        /// </summary>
        PreProduction = 1,

        /// <summary>
        /// China environment, see https://docs.microsoft.com/en-us/office365/servicedescriptions/office-365-platform-service-description/office-365-operated-by-21vianet
        /// </summary>
        China = 2,
        
        /// <summary>
        /// GCC US Government environment, see https://docs.microsoft.com/en-us/office365/servicedescriptions/office-365-platform-service-description/office-365-us-government/office-365-us-government
        /// </summary>
        USGovernment = 4,

        /// <summary>
        /// GCC High US Government environment, see https://docs.microsoft.com/en-us/office365/servicedescriptions/office-365-platform-service-description/office-365-us-government/gcc-high-and-dod
        /// </summary>
        USGovernmentHigh = 5,

        /// <summary>
        /// DOD US Government environment, see https://docs.microsoft.com/en-us/office365/servicedescriptions/office-365-platform-service-description/office-365-us-government/gcc-high-and-dod
        /// </summary>
        USGovernmentDoD = 6,

        /// <summary>
        /// French sovereign cloud environment. A joint venture between Orange and Capgemini, designed to meet SecNumCloud requirements. See https://learn.microsoft.com/en-us/industry/sovereign-cloud/national-partner-clouds/overview-national-partner-clouds
        /// </summary>
        GovFr = 7,

        /// <summary>
        /// Represents the GovDe Cloud environment option. Operated by an SAP subsidiary and aligned with German Cloud Platform Requirements. See https://learn.microsoft.com/en-us/industry/sovereign-cloud/national-partner-clouds/overview-national-partner-clouds
        /// </summary>
        GovDe = 8,

        /// <summary>
        /// Represents the GovSG Cloud environment option. Operated by a Singaporean government-owned company and aligned with Singapore's Sovereign Cloud Framework. See https://learn.microsoft.com/en-us/industry/sovereign-cloud/national-partner-clouds/overview-national-partner-clouds
        /// </summary>
        GovSg = 9,

        /// <summary>
        /// Custom cloud configuration, specify the endpoints manually
        /// </summary>
        Custom = 100
    }
}
