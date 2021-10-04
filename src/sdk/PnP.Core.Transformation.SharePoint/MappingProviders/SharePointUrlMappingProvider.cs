using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.Extensions;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IUrlMappingProvider"/>
    /// </summary>
    public class SharePointUrlMappingProvider : IUrlMappingProvider
    {
        private ILogger<SharePointUrlMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointUrlMappingProvider(ILogger<SharePointUrlMappingProvider> logger,
            IOptions<SharePointTransformationOptions> options,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps a URL from classic to modern
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public async Task<UrlMappingProviderOutput> MapUrlAsync(UrlMappingProviderInput input, CancellationToken token = default)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapUrlAsync");

            // Try cast
            var sharePointSourceItem = input.Context.SourceItem as SharePointSourceItem;
            if (sharePointSourceItem == null)
            {
                throw new ArgumentException($"Only source item of type {typeof(SharePointSourceItem)} is supported");
            }

            string pagesLibrary = await ResolveSitePagesLibraryAsync(sharePointSourceItem.SourceContext);

            sharePointSourceItem.SourceContext.Load(sharePointSourceItem.SourceContext.Web, w => w.Url);
            sharePointSourceItem.SourceContext.Load(sharePointSourceItem.SourceContext.Site, w => w.Url);
            await sharePointSourceItem.SourceContext.ExecuteQueryAsync().ConfigureAwait(false);

            // Prepare result variable
            var resultText = input?.Text ?? string.Empty;

            Uri sourceWebUrl = new Uri(sharePointSourceItem.SourceContext.Web.Url);
            Uri sourceSiteUrl = new Uri(sharePointSourceItem.SourceContext.Site.Url);

            Uri origTargetWebUrl = sourceWebUrl;
            Uri origSourceSiteUrl = sourceSiteUrl;
            Uri targetWebUrl = input.Context.Task.TargetContext.Web.Url;

            bool isSubSite = origSourceSiteUrl.IsBaseOf(origTargetWebUrl);

            if (this.options.Value.UrlMappings != null && this.options.Value.UrlMappings.Count > 0)
            {
                foreach (var urlMapping in this.options.Value.UrlMappings)
                {
                    resultText = RewriteUrl(resultText,
                        urlMapping.SourceUrl,
                        urlMapping.TargetUrl);
                }
            }

            if (!this.options.Value.SkipUrlRewrite)
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
                    var pagesSourceWebUrl = sourceWebUrl.Combine(pagesLibrary);
                    var sitePagesTargetWebUrl = targetWebUrl.Combine("sitepages");

                    if (pagesSourceWebUrl.Scheme == "https" || pagesSourceWebUrl.Scheme == "http")
                    {
                        resultText = RewriteUrl(resultText,
                            pagesSourceWebUrl.ToString(),
                            sitePagesTargetWebUrl.ToString());

                        // Make relative for next replacement attempt
                        pagesSourceWebUrl = new Uri(pagesSourceWebUrl.AbsolutePath, UriKind.Relative);
                        sitePagesTargetWebUrl = new Uri(sitePagesTargetWebUrl.AbsolutePath, UriKind.Relative);
                    }

                    resultText = RewriteUrl(resultText, pagesSourceWebUrl.ToString(), sitePagesTargetWebUrl.ToString());
                }

                // Rewrite web urls
                if (sourceWebUrl.Scheme == "https" || sourceWebUrl.Scheme == "http")
                {
                    resultText = RewriteUrl(resultText, sourceWebUrl.ToString(), targetWebUrl.ToString());
                }

                // Make relative for next replacement attempt
                resultText = RewriteUrl(resultText, 
                    sourceWebUrl.AbsolutePath.TrimEnd('/'), 
                    targetWebUrl.AbsolutePath.TrimEnd('/'));

                if (isSubSite)
                {
                    // reset URLs
                    sourceSiteUrl = origSourceSiteUrl;
                    targetWebUrl = origTargetWebUrl;

                    // Rewrite url's from pages library to sitepages
                    if (!string.IsNullOrEmpty(pagesLibrary))
                    {
                        var pagesSourceSiteUrl = UrlUtility.Combine(sourceSiteUrl, pagesLibrary);
                        var sitePagesTargetWebUrl = UrlUtility.Combine(targetWebUrl, "sitepages");

                        if (pagesSourceSiteUrl.Scheme == "https" || pagesSourceSiteUrl.Scheme == "http")
                        {
                            resultText = RewriteUrl(resultText, 
                                pagesSourceSiteUrl.ToString(), 
                                sitePagesTargetWebUrl.ToString());
                        }

                        // Make relative for next replacement attempt
                        resultText = RewriteUrl(resultText, 
                            pagesSourceSiteUrl.AbsolutePath.TrimEnd('/'), 
                            sitePagesTargetWebUrl.AbsolutePath.TrimEnd('/'));
                    }

                    // Rewrite root site urls
                    if (sourceSiteUrl.Scheme == "https" || sourceSiteUrl.Scheme == "http")
                    {
                        resultText = RewriteUrl(resultText, 
                            sourceSiteUrl.ToString(), 
                            targetWebUrl.ToString());
                    }

                    // Make relative for next replacement attempt
                    resultText = RewriteUrl(resultText, 
                        sourceSiteUrl.AbsolutePath.TrimEnd('/'), 
                        targetWebUrl.AbsolutePath.TrimEnd('/'));
                }
            }

            return new UrlMappingProviderOutput(resultText);
        }

        private async Task<string> ResolveSitePagesLibraryAsync(ClientContext context)
        {
            var lists = context.Web.Lists;
            context.Load(lists, lists => lists.Include(
                l => l.Title, 
                l => l.BaseTemplate,
                l => l.RootFolder,
                l => l.Fields.Include(f => f.InternalName)));
            await context.ExecuteQueryRetryAsync().ConfigureAwait(false);

            foreach (var list in lists)
            {
                if (list.BaseTemplate == (int)ListTemplateType.WebPageLibrary)
                {
                    // The site pages library has the CanvasContent1 column,
                    // using that to distinguish between Site Pages and other wiki page libraries
                    if (list.Fields.FirstOrDefault(f => f.InternalName == "CanvasContent1") != null)
                    {
                        return list.RootFolder.Name;
                    }
                }
            }

            // Fallback to the default "sitepages" value
            return "sitepages";
        }

        private string RewriteUrl(string input, string from, string to)
        {
            // Do not replace this character - breaks HTML
            if (from != "/" && !IsRoot(from))
            {
                var regex = new Regex($"{Regex.Escape(from)}", RegexOptions.IgnoreCase);
                if (regex.IsMatch(input))
                {
                    string before = input;
                    input = regex.Replace(input, to);
                }
            }

            return input;
        }

        private bool IsRoot(string url)
        {
            var baseUrl = url.GetBaseUrl();
            if (baseUrl.Equals(url, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
