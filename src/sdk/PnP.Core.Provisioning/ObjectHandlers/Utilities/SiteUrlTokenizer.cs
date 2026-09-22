using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Replaces the urls and ids of the site a value was read from with the tokens that resolve them again on the
    /// site a template is applied to.
    /// </summary>
    /// <remarks>
    /// An absolute url becomes <c>{hosturl}{site}</c> rather than <c>{site}</c>, which resolves to a server relative
    /// path, so that a value that pointed at another host still does on the target. A url only matches up to a
    /// path boundary, so the site <c>/sites/team</c> leaves <c>/sites/teamwork</c> alone.
    /// </remarks>
    internal sealed class SiteUrlTokenizer
    {
        /// <summary>
        /// What may follow a url or path for it to be the one being replaced, rather than the start of a longer one.
        /// </summary>
        private const string End = @"(?=$|[/?#""'\s,;<>)\]|&])";

        /// <summary>
        /// What may precede a server relative path, so that the path of another host is left alone.
        /// </summary>
        private const string Start = @"(?<=^|[\s""'(=,;>\[|])";

        private readonly List<(Regex Pattern, string Token)> replacements = new List<(Regex, string)>();

        /// <summary>
        /// Creates a tokenizer for one site.
        /// </summary>
        /// <param name="webUrl">The absolute url of the web</param>
        /// <param name="webServerRelativeUrl">The server relative url of the web</param>
        /// <param name="webId">The id of the web</param>
        /// <param name="siteUrl">The absolute url of the site collection</param>
        /// <param name="siteServerRelativeUrl">The server relative url of the site collection</param>
        /// <param name="siteId">The id of the site collection</param>
        internal SiteUrlTokenizer(Uri webUrl, string webServerRelativeUrl, Guid webId,
            Uri siteUrl, string siteServerRelativeUrl, Guid siteId)
        {
            // Most specific first: a sub site's url starts with its site collection's.
            AddUrl(webUrl, "{hosturl}{site}");
            AddUrl(siteUrl, "{hosturl}{sitecollection}");
            AddPath(webServerRelativeUrl, "{site}");
            AddPath(siteServerRelativeUrl, "{sitecollection}");
            AddId(webId, "{siteid}");
            AddId(siteId, "{sitecollectionid}");
        }

        /// <summary>
        /// Creates a tokenizer for the site a context points at.
        /// </summary>
        internal static async Task<SiteUrlTokenizer> CreateAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Url, w => w.ServerRelativeUrl, w => w.Id).ConfigureAwait(false);
            await context.Site.LoadAsync(s => s.Url, s => s.ServerRelativeUrl, s => s.Id).ConfigureAwait(false);

            return new SiteUrlTokenizer(context.Web.Url, context.Web.ServerRelativeUrl, context.Web.Id,
                context.Site.Url, context.Site.ServerRelativeUrl, context.Site.Id);
        }

        /// <summary>
        /// Replaces the site's urls and ids in a value with tokens.
        /// </summary>
        internal string Tokenize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            foreach ((Regex pattern, string token) in replacements)
            {
                value = pattern.Replace(value, token);
            }

            return value;
        }

        private void AddUrl(Uri url, string token)
        {
            string text = url?.ToString().TrimEnd('/');

            if (!string.IsNullOrEmpty(text))
            {
                Add(Regex.Escape(text) + End, token);
            }
        }

        private void AddPath(string path, string token)
        {
            string text = path?.TrimEnd('/');

            // The root site collection's path is "/", which is part of every url there is.
            if (!string.IsNullOrEmpty(text))
            {
                Add(Start + Regex.Escape(text) + End, token);
            }
        }

        private void AddId(Guid id, string token)
        {
            if (id != Guid.Empty)
            {
                Add(Regex.Escape(id.ToString("D")), token);
            }
        }

        private void Add(string pattern, string token)
        {
            replacements.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), token));
        }
    }
}
