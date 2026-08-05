using System;

namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Basic interface for all the resolver types
    /// </summary>
    public interface IResolver
    {
        // TODO: Consider adding something like IsReusable property

        /// <summary>
        /// Provides the name of the Resolver
        /// </summary>
        String Name { get; }
    }
}
