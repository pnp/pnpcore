using PnP.Core.Modernization.Transform;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Publishing
{
    /// <summary>
    /// Information used to configure the publishing page transformation process
    /// </summary>
    public class PublishingPageTransformationInformation : BaseTransformationInformation
    {
        #region Construction
        /// <summary>
        /// For internal use only
        /// </summary>
        public PublishingPageTransformationInformation() 
        {
            // constructor used exclusively for mocking
        }

        /// <summary>
        /// Instantiates the page transformation class
        /// </summary>
        /// <param name="sourcePage">Page we want to transform</param>
        public PublishingPageTransformationInformation(IListItem sourcePage) : this(sourcePage, false)
        {
        }

        /// <summary>
        /// Instantiates the page transformation class
        /// </summary>
        /// <param name="sourcePage">Page we want to transform</param>
        /// <param name="overwrite">Do we overwrite the target page if it already exists</param>
        public PublishingPageTransformationInformation(IListItem sourcePage, bool overwrite)
        {
            SourcePage = sourcePage;
            Overwrite = overwrite;
            HandleWikiImagesAndVideos = true;
            AddTableListImageAsImageWebPart = true;
            KeepPageSpecificPermissions = true;
            SkipTelemetry = false;
            RemoveEmptySectionsAndColumns = true;
            PublishCreatedPage = true;
            KeepPageCreationModificationInformation = false;
            PostAsNews = false;
            DisablePageComments = false;
            SkipUserMapping = false;
            TargetPageFolderOverridesDefaultFolder = false;
            // Populate with OOB mapping properties
            MappingProperties = new Dictionary<string, string>(5)
            {
                { Constants.UseCommunityScriptEditorMappingProperty, "false" },
                { Constants.SummaryLinksToQuickLinksMappingProperty, "true" }
            };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Given a collection of field names will return the first one which contains a value which is not null or empty.
        /// </summary>
        /// <param name="fieldNames">A array of Internal field names to check</param>
        /// <returns>An Internal field name</returns>
        public string GetFirstNonEmptyFieldName(string[] fieldNames)
        {
            // trim each entry before continuing
            // and ignore empty or null values
            fieldNames = fieldNames.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

            string fieldNameToReturn = string.Empty;

            foreach(string fieldName in fieldNames)
            {
                if(IsFieldUsed(fieldName))
                {
                    // use this field
                    fieldNameToReturn = fieldName;
                    break;
                }
            }

            return fieldNameToReturn;
        }

        /// <summary>
        /// Checks if a field is in use
        /// </summary>
        /// <param name="fieldName">Internal field name</param>
        /// <returns>True or False</returns>
        public virtual bool IsFieldUsed(string fieldName)
        {
            return this.SourcePage.FieldExistsAndUsed(fieldName);
        }
        #endregion

        #region Page Properties
        /// <summary>
        /// Enables Enterprise Wiki destination preparation, same-name category/rating mappings
        /// and optional sidebar insertion. Defaults to false, preserving legacy transformation
        /// behavior for all destinations. Required site columns and term sets must already exist.
        /// The destination Site Pages feature must already be enabled; preparation does not activate it,
        /// whether transforming in-place or across site collections.
        /// Rating fields omitted from a partial source item are loaded before migrating rating history.
        /// </summary>
        public bool EnableEnterpriseWikiCapabilities { get; set; }

        /// <summary>
        /// Adds the native Enterprise Wiki sidebar when EnableEnterpriseWikiCapabilities is true
        /// and the source is an Enterprise Wiki page.
        /// The destination must support the modernized control host; this option does not deploy it.
        /// Set to false for environments without that renderer.
        /// </summary>
        public bool IncludeEnterpriseWikiSidebar { get; set; } = true;

        internal bool IsEnterpriseWikiPage { get; set; }
        internal bool ShouldIncludeEnterpriseWikiSidebar =>
            EnableEnterpriseWikiCapabilities && IsEnterpriseWikiPage && IncludeEnterpriseWikiSidebar;
        #endregion
    }
}
