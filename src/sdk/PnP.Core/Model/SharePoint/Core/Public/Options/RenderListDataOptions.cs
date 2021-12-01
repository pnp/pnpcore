using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure the rendering of list data via the RenderListDataAsStream method of IList
    /// See https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/working-with-lists-and-list-items-with-rest#renderlistdataasstream-body-parameter-properties
    /// </summary>
    public class RenderListDataOptions
    {
        /// <summary>
        /// Specifies if required fields should be returned or not
        /// </summary>
        public bool? AddRequiredFields { get; set; }

        /// <summary>
        /// Specifies if multi value filtering is allowed for taxonomy fields or not
        /// </summary>
        public bool? AllowMultipleValueFilterForTaxonomyFields { get; set; }

        /// <summary>
        /// Audience to use while processing this request
        /// </summary>
        public bool? AudienceTarget { get; set; }

        /// <summary>
        /// Specifies if we return DateTime field in UTC or local time
        /// </summary>
        public bool? DatesInUtc { get; set; }

        /// <summary>
        /// Use a deferred render?
        /// </summary>
        public bool? DeferredRender { get; set; }

        /// <summary>
        /// Specifies if the grouping should be expanded or not
        /// </summary>
        public bool? ExpandGroups { get; set; }

        /// <summary>
        /// Specifies if only the first group should be returned or not (regardless of view schema)
        /// </summary>
        public bool? FirstGroupOnly { get; set; }

        /// <summary>
        /// Specifies the url to the folder from which to return items
        /// </summary>
        public string FolderServerRelativeUrl { get; set; }

        /// <summary>
        /// Comma-separated list of field names whose values should be rewritten to CDN URLs
        /// </summary>
        public string ImageFieldsToTryRewriteToCdnUrls { get; set; }

        /// <summary>
        /// Merge with the default view?
        /// </summary>
        public bool? MergeDefaultView { get; set; }

        /// <summary>
        /// Return the original date?
        /// </summary>
        public bool? OriginalDate { get; set; }

        /// <summary>
        /// Specifies the override XML to be combined with the View CAML. Applies only to the Query/Where part of the View CAML
        /// </summary>
        public string OverrideViewXml { get; set; }

        /// <summary>
        /// Specifies the paging information
        /// </summary>
        public string Paging { get; set; }

        /// <summary>
        /// Specifies if the grouping should be replaced or not to deal with GroupBy throttling
        /// </summary>
        public bool? ReplaceGroup { get; set; }

        /// <summary>
        /// Specifies the type of output to return
        /// </summary>
        public RenderListDataOptionsFlags? RenderOptions { get; set; }

        /// <summary>
        /// Specifies the CAML view XML
        /// </summary>
        public string ViewXml { get; set; }

        /// <summary>
        /// Populates the needed ViewXml based upon the passed field names
        /// </summary>
        /// <param name="fieldInternalNames">List of fields specified via their internal name</param>
        public void SetViewXmlFromFields(List<string> fieldInternalNames)
        {
            if (fieldInternalNames == null)
            {
                throw new ArgumentNullException(nameof(fieldInternalNames));
            }

            if (fieldInternalNames.Any())
            {
                bool fieldAdded = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<View Scope='Recursive'>");
                sb.AppendLine("<ViewFields>");

                foreach (var field in fieldInternalNames)
                {
                    if (!string.IsNullOrEmpty(field))
                    {
                        sb.AppendLine($"<FieldRef Name='{field}' />");
                        fieldAdded = true;
                    }
                }

                sb.AppendLine("</ViewFields>");
                sb.AppendLine("</View>");

                if (fieldAdded)
                {
                    ViewXml = sb.ToString();
                }
            }
        }

    }
}
