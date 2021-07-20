using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a mapping provider
    /// </summary>
    public class MappingProviderOutput: BaseMappingProviderOutput
    {
        /// <summary>
        /// Defines if the the page to create should be the Home Page of the site
        /// </summary>
        public bool IsHomePage { get; set; }

        //public PageLayoutType PageLayoutType { get; set; }

        ///// <summary>
        ///// The list of Web Parts to transform
        ///// </summary>
        //public List<WebPartEntity> WebParts { get; set; }

        ///// <summary>
        ///// The taxonomy terms to transform
        ///// </summary>
        //public List<TaxonomyTerm> Terms { get; set; }

        /// <summary>
        /// The Author of the page
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The last Editor of the page
        /// </summary>
        public string Editor { get; set; }

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
        /// Configuration of the page header to apply
        /// </summary>
        public IPageHeader PageHeader { get; set; }

        /// <summary>
        /// The title of the target page
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// Sets the page author in the page header similar to the original page author
        /// </summary>
        public bool SetAuthorInPageHeader { get; set; }

        /// <summary>
        /// The sections to render in the target modern page
        /// </summary>
        public List<CanvasSectionTemplate> Sections { get; set; }
    }
}
