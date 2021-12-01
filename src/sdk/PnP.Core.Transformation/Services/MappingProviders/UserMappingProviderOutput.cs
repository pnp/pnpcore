namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a user mapping provider
    /// </summary>
    public class UserMappingProviderOutput : BaseMappingProviderOutput
    {
        /// <summary>
        /// Defines the target UPN from the mapping
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}
