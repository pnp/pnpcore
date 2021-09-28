using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Model
{
    /// <summary>
    /// Class to hold information about the source SharePoint page to transform
    /// </summary>
    internal class SourcePageInformation
    {
        /// <summary>
        /// SharePoint version of the source 
        /// </summary>
        internal SPVersion SourceVersion { get; set; }

        /// <summary>
        /// SharePoint version number of the source 
        /// </summary>
        internal string SourceVersionNumber { get; set; }

        /// <summary>
        /// The Author of the page
        /// </summary>
        public FieldUserValue Author { get; set; }

        /// <summary>
        /// The last Editor of the page
        /// </summary>
        public FieldUserValue Editor { get; set; }

        /// <summary>
        /// The Creation date time of the page
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The last Update date time of the page
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// The parent Folder of the page, if any
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Defines the Type of the source page
        /// </summary>
        public SourcePageType PageType { get; set; }

        /// <summary>
        /// Defines whether the source page is a root page for the source site
        /// </summary>
        public bool IsRootPage { get; set; }

        /// <summary>
        /// Defines whether the source page is the Home Page for the source site
        /// </summary>
        public bool IsHomePage { get; set; }

        /// <summary>
        /// Defines the Page Layout for the publishing page, if any
        /// </summary>
        public string PageLayout { get; set; }
    }
}
