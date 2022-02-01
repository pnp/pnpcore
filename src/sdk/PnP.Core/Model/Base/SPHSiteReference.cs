using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Base
{
    /// <summary>
    /// Represents data transfer object for SPHSiteReference
    /// </summary>
    public class SPHSiteReference
    {
        /// <summary>
        /// Home site id
        /// </summary>
        public string SiteId { get; set; }
        /// <summary>
        /// Home site title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Home site url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Home site web id
        /// </summary>
        public string WebId { get; set; }
    }
}
