using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;

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
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapUrlAsync");

            // Try cast
            var sharePointSourceItem = input.Context.SourceItem as SharePointSourceItem;
            if (sharePointSourceItem == null)
            {
                throw new ArgumentException($"Only source item of type {typeof(SharePointSourceItem)} is supported");
            }

            // TODO: resolve library path
            string pagesLibrary = "sitepages";

            // TODO: load only if needed
            sharePointSourceItem.SourceContext.Load(sharePointSourceItem.SourceContext.Web, w => w.Url);
            sharePointSourceItem.SourceContext.Load(sharePointSourceItem.SourceContext.Site, w => w.Url);
            await sharePointSourceItem.SourceContext.ExecuteQueryAsync();

            Uri result = input.Url;
            Uri sourceWebUrl = new Uri(sharePointSourceItem.SourceContext.Web.Url);
            Uri sourceSiteUrl = new Uri(sharePointSourceItem.SourceContext.Site.Url);

            Uri origTargetWebUrl = sourceWebUrl;
            Uri origSourceSiteUrl = sourceSiteUrl;
            Uri targetWebUrl = input.Context.Task.TargetContext.Web.Url;

            bool isSubSite = origSourceSiteUrl.IsBaseOf(origTargetWebUrl);

            // TODO: apply custom mappings

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

                result = RewriteUrl(result, pagesSourceWebUrl, sitePagesTargetWebUrl);
            }

            result = RewriteUrl(result, sourceWebUrl, targetWebUrl);

            if (isSubSite)
            {
                // reset URLs
                sourceSiteUrl = origSourceSiteUrl;
                targetWebUrl = origTargetWebUrl;

                // Rewrite url's from pages library to sitepages
                if (!string.IsNullOrEmpty(pagesLibrary))
                {
                    var pagesSourceSiteUrl = sourceSiteUrl.Combine(pagesLibrary);
                    var sitePagesTargetWebUrl = targetWebUrl.Combine("sitepages");
                    
                    result = RewriteUrl(result, pagesSourceSiteUrl, sitePagesTargetWebUrl);
                }

                result = RewriteUrl(result, sourceSiteUrl, targetWebUrl);
            }

            return new UrlMappingProviderOutput(result);
        }

        private Uri RewriteUrl(Uri input, Uri from, Uri to)
        {
            // If not root
            if (from.LocalPath != "/")
            {
                var regex = new Regex($"{Regex.Escape(from.ToString())}", RegexOptions.IgnoreCase);
                if (regex.IsMatch(input.ToString()))
                {
                    input = new Uri(regex.Replace(input.ToString(), to.ToString()));
                }
            }

            return input;
        }


    }
}
