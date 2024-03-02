using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Services
{
    /// <summary>
    /// Options used when a <see cref="PnPContext"/> is created
    /// </summary>
    public class PnPContextOptions
    {
        /// <summary>
        /// Additional <seealso cref="ISite"/> properties to load when creating a new <seealso cref="PnPContext"/>.
        /// </summary>
        public IEnumerable<Expression<Func<ISite, object>>> AdditionalSitePropertiesOnCreate { get; set; }

        /// <summary>
        /// Additional <seealso cref="IWeb"/> properties to load when creating a new <seealso cref="PnPContext"/>.
        /// </summary>
        public IEnumerable<Expression<Func<IWeb, object>>> AdditionalWebPropertiesOnCreate { get; set; }

        /// <summary>
        /// Properties to set on the context during creation
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        /// By default <see cref="PnPContext"/> initialization requires two roundtrips, the first one is done interactively as that allows
        /// PnP Core SDK to pickup the correct casing for the site URI as batching in SharePoint requires exact casing. The second roundtrip 
        /// is used to load the remaining initialization data. If you set this property to true then both requests are executed as a single 
        /// batch request, resulting in only 1 roundtrip. This can be useful in scenarios where you want to reduce the number of requests.
        /// </summary>
        public bool SiteUriCasingIsCorrect { get; set; } = false;
    }
}
