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
        public Task<UrlMappingProviderOutput> MapUrlAsync(UrlMappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapUrlAsync");

            //// Try cast
            //var sharePointSourceItem = input.Context.SourceItem as SharePointSourceItem;
            //if (sharePointSourceItem == null)
            //{
            //    throw new ArgumentException($"Only source item of type {typeof(SharePointSourceItem)} is supported");
            //}

            //Uri origSourceSiteUrl = input.Url;
            //Uri origTargetWebUrl = input.Context.Task.TargetContext.Web.Url;

            //bool isSubSite = origSourceSiteUrl.IsBaseOf(origTargetWebUrl);

            //// ********************************************
            //// Default URL rewriting logic
            //// ********************************************            
            ////
            //// Root site collection URL rewriting:
            //// http://contoso.com/sites/portal -> https://contoso.sharepoint.com/sites/hr
            //// http://contoso.com/sites/portal/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
            //// /sites/portal -> /sites/hr
            //// /sites/portal/pages -> /sites/hr/sitepages
            ////
            //// If site is a sub site then we also by rewrite the sub URL's
            //// http://contoso.com/sites/portal/hr -> https://contoso.sharepoint.com/sites/hr
            //// http://contoso.com/sites/portal/hr/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
            //// /sites/portal/hr -> /sites/hr
            //// /sites/portal/hr/pages -> /sites/hr/sitepages


            //// Rewrite url's from pages library to sitepages
            //if (!string.IsNullOrEmpty(pagesLibrary))
            //{
            //    string pagesSourceWebUrl = UrlUtility.Combine(sourceWebUrl, pagesLibrary);
            //    string sitePagesTargetWebUrl = UrlUtility.Combine(targetWebUrl, "sitepages");

            //    if (pagesSourceWebUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || pagesSourceWebUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        input = RewriteUrl(input, pagesSourceWebUrl, sitePagesTargetWebUrl);

            //        // Make relative for next replacement attempt
            //        pagesSourceWebUrl = MakeRelative(pagesSourceWebUrl);
            //        sitePagesTargetWebUrl = MakeRelative(sitePagesTargetWebUrl);
            //    }

            //    input = RewriteUrl(input, pagesSourceWebUrl, sitePagesTargetWebUrl);
            //}

            ////Ensure the trailing slash
            //if (input != sourceSiteUrl)
            //{
            //    sourceWebUrl = $"{sourceWebUrl.TrimEnd('/')}/";
            //    targetWebUrl = $"{targetWebUrl.TrimEnd('/')}/";
            //}

            //// Rewrite web urls
            //if (sourceWebUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || sourceWebUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    input = RewriteUrl(input, sourceWebUrl, targetWebUrl);

            //    // Make relative for next replacement attempt
            //    sourceWebUrl = $"{MakeRelative(sourceWebUrl).TrimEnd('/')}/";
            //    targetWebUrl = $"{MakeRelative(targetWebUrl).TrimEnd('/')}/";
            //}

            //input = RewriteUrl(input, sourceWebUrl, targetWebUrl);

            //if (isSubSite)
            //{
            //    // reset URLs
            //    sourceSiteUrl = origSourceSiteUrl;
            //    targetWebUrl = origTargetWebUrl;

            //    // Rewrite url's from pages library to sitepages
            //    if (!string.IsNullOrEmpty(pagesLibrary))
            //    {
            //        string pagesSourceSiteUrl = UrlUtility.Combine(sourceSiteUrl, pagesLibrary);
            //        string sitePagesTargetWebUrl = UrlUtility.Combine(targetWebUrl, "sitepages");

            //        if (pagesSourceSiteUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || pagesSourceSiteUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            input = RewriteUrl(input, pagesSourceSiteUrl, sitePagesTargetWebUrl);

            //            // Make relative for next replacement attempt
            //            pagesSourceSiteUrl = MakeRelative(pagesSourceSiteUrl);
            //            sitePagesTargetWebUrl = MakeRelative(sitePagesTargetWebUrl);
            //        }

            //        input = RewriteUrl(input, pagesSourceSiteUrl, sitePagesTargetWebUrl);
            //    }

            //    // Rewrite root site urls
            //    if (sourceSiteUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || sourceSiteUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        input = RewriteUrl(input, sourceSiteUrl, targetWebUrl);

            //        // Make relative for next replacement attempt
            //        sourceSiteUrl = $"{MakeRelative(sourceSiteUrl).TrimEnd('/')}/";
            //        targetWebUrl = $"{MakeRelative(targetWebUrl).TrimEnd('/')}/";
            //    }

            //    input = RewriteUrl(input, sourceSiteUrl, targetWebUrl);
            //}

            return Task.FromResult(new UrlMappingProviderOutput());
        }

        //private string RewriteUrl(string input, string from, string to)
        //{
        //    //Do not replace this character - breaks HTML
        //    if (from != "/" && !IsRoot(from))
        //    {
        //        var regex = new Regex($"{Regex.Escape(from)}", RegexOptions.IgnoreCase);
        //        if (regex.IsMatch(input))
        //        {
        //            string before = input;
        //            input = regex.Replace(input, to);
        //        }
        //    }

        //    return input;
        //}

        private bool IsRoot(Uri url)
        {
            return url.LocalPath == "/";
        }

        private string MakeRelative(string url)
        {
            Uri uri = new Uri(url);
            return uri.AbsolutePath;
        }
    }
}
