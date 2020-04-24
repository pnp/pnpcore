namespace PnP.M365.DomainModelGenerator
{
    public class EdmxProviderOptions
    {
        /// <summary>
        /// The Name of the EDMX provider
        /// </summary>
        public string EdmxProviderName { get; set; }

        /// <summary>
        /// The URI to get the metadata file
        /// </summary>
        public string MetadataUri { get; set; }

        /// <summary>
        /// The Name of the Authentication Provider configuration, if any
        /// </summary>
        public string AuthenticationProviderName { get; set; }
    }
}
