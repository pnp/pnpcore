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
        /// German environment, see https://docs.microsoft.com/en-us/office365/servicedescriptions/office-365-platform-service-description/office-365-germany
        /// </summary>
        Germany = 3,

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
        /// Custom cloud configuration, specify the endpoints manually
        /// </summary>
        Custom = 100
    }
}
