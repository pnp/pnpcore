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
    }
}
