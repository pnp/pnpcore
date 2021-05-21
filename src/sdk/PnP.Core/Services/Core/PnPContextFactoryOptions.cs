using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the options to configure the PnPContextFactory object
    /// </summary>
    public class PnPContextFactoryOptions
    {
        /// <summary>
        /// Controls whether the library will try to use Microsoft Graph over REST whenever that's defined in the model
        /// </summary>
        public bool GraphFirst { get; set; } = true;

        /// <summary>
        /// If true than the Graph beta endpoint is used when there's no other option, default approach stays using the v1 endpoint
        /// </summary>
        public bool GraphCanUseBeta { get; set; } = true;

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        public bool GraphAlwaysUseBeta { get; set; }

        /// <summary>
        /// Additional <seealso cref="ISite"/> properties to load when creating a new <seealso cref="PnPContext"/>.
        /// </summary>
        public IEnumerable<Expression<Func<ISite, object>>> DefaultSitePropertiesOnCreate { get; set; }

        /// <summary>
        /// Additional <seealso cref="IWeb"/> properties to load when creating a new <seealso cref="PnPContext"/>.
        /// </summary>
        public IEnumerable<Expression<Func<IWeb, object>>> DefaultWebPropertiesOnCreate { get; set; }

        /// <summary>
        /// The Default Authentication Provider configuration
        /// </summary>
        public IAuthenticationProvider DefaultAuthenticationProvider { get; set; }

        /// <summary>
        /// Collection of configurations for PnPContextFactory
        /// </summary>
        public List<PnPContextFactoryOptionsConfiguration> Configurations { get; } = new List<PnPContextFactoryOptionsConfiguration>();
    }
}
