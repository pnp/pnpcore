using PnP.Core.Provisioning.Utilities;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Class responsible for transforming source page urls to target page urls via url rewriting
    /// </summary>
    public class UrlTransformator : BaseTransform
    {
        private PnPContext sourceContext;
        private PnPContext targetContext;
        private string sourceSiteUrl;
        private string sourceWebUrl;
        private string targetWebUrl;
        private string pagesLibrary;
        private bool skipDefaultUrlRewrite;
        private List<UrlMapping> urlMapping;

        #region Construction
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="baseTransformationInformation">Page transformation information object</param>
        /// <param name="sourceContext">ClientContext of the source page</param>
        /// <param name="targetContext">ClientContext of the target page</param>
        /// <param name="logObservers">Connected loggers</param>
        public UrlTransformator(BaseTransformationInformation baseTransformationInformation, PnPContext sourceContext, PnPContext targetContext, IList<ILogObserver> logObservers = null)
        {
            // Hookup logging
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            // Ensure source and target context are set
            if (sourceContext == null && targetContext != null)
            {
                sourceContext = targetContext;
            }

            if (targetContext == null && sourceContext != null)
            {
                targetContext = sourceContext;
            }

            // Grab the needed information to drive url rewrite
            this.sourceContext = sourceContext;
            this.targetContext = targetContext;

            sourceContext.Site.EnsureProperties(p => p.ServerRelativeUrl, p => p.Url);

            this.sourceSiteUrl = sourceContext.Site.Url.AbsoluteUri;
            this.sourceWebUrl = sourceContext.Web.GetUrl();

            if (CacheManager.Instance.IsPublishingWeb(this.sourceWebUrl))
            {
                this.pagesLibrary = CacheManager.Instance.GetPublishingPagesLibraryName(this.sourceContext);
            }
            else
            {
                this.pagesLibrary = "sitepages";
            }            

            this.targetWebUrl = targetContext.Web.GetUrl();
            // Load the URL mapping file
            if (!string.IsNullOrEmpty(baseTransformationInformation?.UrlMappingFile))
            {
                this.urlMapping = CacheManager.Instance.GetUrlMapping(baseTransformationInformation.UrlMappingFile, logObservers);
            }

            this.skipDefaultUrlRewrite = baseTransformationInformation.SkipDefaultUrlRewrite;
        }
        #endregion

        /// <summary>
        /// Rewrites the url inside the passed input string
        /// </summary>
        /// <param name="input">String to url rewrite</param>
        /// <returns>Url rewritten string</returns>
        public string Transform(string input)
        {
            // Do we need to rewrite?
            if (this.urlMapping == null && this.sourceWebUrl.Equals(this.targetWebUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return input;
            }

            return ReWriteUrls(input, this.sourceSiteUrl, this.sourceWebUrl, this.targetWebUrl, this.pagesLibrary);
        }

        internal string ReWriteUrls(string input, string sourceSiteUrl, string sourceWebUrl, string targetWebUrl, string pagesLibrary)
        {
            //TODO: find a solution for managed navigation links as they're returned as "https://bertonline.sharepoint.com/sites/ModernizationTarget/_layouts/15/FIXUPREDIRECT.ASPX?WebId=b710de6c-ff13-41f2-b119-0e7ad57269d2&TermSetId=c6eba345-eaf4-4e17-9c3e-c8436e017326&TermId=c2d20b8f-e70b-417d-8aa3-d5e3b59f6167"

            string origSourceSiteUrl = sourceSiteUrl;
            string origTargetWebUrl = targetWebUrl;

            bool isSubSite = !sourceSiteUrl.Equals(sourceWebUrl, StringComparison.InvariantCultureIgnoreCase);

            // ********************************************************
            // Custom URL rewriting logic (if URL mapping was provided)
            // ********************************************************            

            if (this.urlMapping != null && this.urlMapping.Count > 0)
            {
                foreach (var urlMapping in this.urlMapping)
                {
                    input = RewriteUrl(input, urlMapping.SourceUrl, urlMapping.TargetUrl);
                }
            }

            if (!this.skipDefaultUrlRewrite)
            {
                // ********************************************
                // Default URL rewriting logic
                // ********************************************            
                //
                // Root site collection URL rewriting:
                // http://contoso.com/sites/portal -> https://contoso.sharepoint.com/sites/hr
                // http://contoso.com/sites/portal/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
                // /sites/portal -> /sites/hr
                // /sites/portal/pages -> /sites/hr/sitepages
                //
                // If site is a sub site then we also by rewrite the sub URL's
                // http://contoso.com/sites/portal/hr -> https://contoso.sharepoint.com/sites/hr
                // http://contoso.com/sites/portal/hr/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
                // /sites/portal/hr -> /sites/hr
                // /sites/portal/hr/pages -> /sites/hr/sitepages


                // Rewrite url's from pages library to sitepages
                if (!string.IsNullOrEmpty(pagesLibrary))
                {
                    string pagesSourceWebUrl = UrlUtility.Combine(sourceWebUrl, pagesLibrary);
                    string sitePagesTargetWebUrl = UrlUtility.Combine(targetWebUrl, "sitepages");

                    if (pagesSourceWebUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || pagesSourceWebUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        input = RewriteUrl(input, pagesSourceWebUrl, sitePagesTargetWebUrl);

                        // Make relative for next replacement attempt
                        pagesSourceWebUrl = MakeRelative(pagesSourceWebUrl);
                        sitePagesTargetWebUrl = MakeRelative(sitePagesTargetWebUrl);
                    }

                    input = RewriteUrl(input, pagesSourceWebUrl, sitePagesTargetWebUrl);
                }

                //Ensure the trailing slash
                if (input != sourceSiteUrl)
                {
                    sourceWebUrl = $"{sourceWebUrl.TrimEnd('/')}/";
                    targetWebUrl = $"{targetWebUrl.TrimEnd('/')}/";
                }

                // Rewrite web urls
                if (sourceWebUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || sourceWebUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    input = RewriteUrl(input, sourceWebUrl, targetWebUrl);

                    // Make relative for next replacement attempt
                    sourceWebUrl = $"{MakeRelative(sourceWebUrl).TrimEnd('/')}/";
                    targetWebUrl = $"{MakeRelative(targetWebUrl).TrimEnd('/')}/";
                }

                input = RewriteUrl(input, sourceWebUrl, targetWebUrl);

                if (isSubSite)
                {
                    // reset URLs
                    sourceSiteUrl = origSourceSiteUrl;
                    targetWebUrl = origTargetWebUrl;

                    // Rewrite url's from pages library to sitepages
                    if (!string.IsNullOrEmpty(pagesLibrary))
                    {
                        string pagesSourceSiteUrl = UrlUtility.Combine(sourceSiteUrl, pagesLibrary);
                        string sitePagesTargetWebUrl = UrlUtility.Combine(targetWebUrl, "sitepages");

                        if (pagesSourceSiteUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || pagesSourceSiteUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                        {
                            input = RewriteUrl(input, pagesSourceSiteUrl, sitePagesTargetWebUrl);

                            // Make relative for next replacement attempt
                            pagesSourceSiteUrl = MakeRelative(pagesSourceSiteUrl);
                            sitePagesTargetWebUrl = MakeRelative(sitePagesTargetWebUrl);
                        }

                        input = RewriteUrl(input, pagesSourceSiteUrl, sitePagesTargetWebUrl);
                    }

                    // Rewrite root site urls
                    if (sourceSiteUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || sourceSiteUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        input = RewriteUrl(input, sourceSiteUrl, targetWebUrl);

                        // Make relative for next replacement attempt
                        sourceSiteUrl = $"{MakeRelative(sourceSiteUrl).TrimEnd('/')}/";
                        targetWebUrl = $"{MakeRelative(targetWebUrl).TrimEnd('/')}/";
                    }

                    input = RewriteUrl(input, sourceSiteUrl, targetWebUrl);
                }
            }

            return input;
        }

        internal string RewriteUrl(string input, string from, string to)
        {
            //Do not replace this character - breaks HTML
            if (from != "/" && !IsRoot(from))
            {
                var regex = new Regex($"{Regex.Escape(from)}", RegexOptions.IgnoreCase);
                if (regex.IsMatch(input))
                {
                    string before = input;
                    input = regex.Replace(input, to);
                    //LogDebug(string.Format(LogStrings.UrlRewritten, before, input), LogStrings.Heading_UrlRewriter);
                }
            }

            return input;
        }

        internal bool IsRoot(string url)
        {
            var baseUrl = url.GetBaseUrl();
            if(baseUrl.Equals(url, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        internal string MakeRelative(string url)
        {
            Uri uri = new Uri(url);
            return uri.AbsolutePath;
        }

    }
}
