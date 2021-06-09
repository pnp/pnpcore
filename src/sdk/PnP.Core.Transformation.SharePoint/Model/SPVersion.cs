using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Model
{
    /// <summary>
    /// Enum listing the SharePoint versions as used by the transformation engine
    /// </summary>
    public enum SPVersion
    {
        /// <summary>
        /// SharePoint Online
        /// </summary>
        SPO = 0,
        /// <summary>
        /// SharePoint 2019 (on-premises)
        /// </summary>
        SP2019 = 1,
        /// <summary>
        /// SharePoint 2016 (on-premises)
        /// </summary>
        SP2016 = 2,
        /// <summary>
        /// SharePoint 2016 legacy (on-premises)
        /// </summary>
        SP2016Legacy = 3,
        /// <summary>
        /// SharePoint 2013 (on-premises)
        /// </summary>
        SP2013 = 4,
        /// <summary>
        /// SharePoint 2013 legacy (on-premises)
        /// </summary>
        SP2013Legacy = 5,
        /// <summary>
        /// SharePoint 2010 (on-premises)
        /// </summary>
        SP2010 = 6,
        /// <summary>
        /// Unknown version
        /// </summary>
        Unknown = 100
    }
}
