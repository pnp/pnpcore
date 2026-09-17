using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Base class for the object handlers - the units the provisioning engine is built from.
    /// Each handler owns one area of a template (fields, lists, navigation, ...) and knows how to
    /// write it to a site and read it back.
    /// </summary>
    internal abstract class ObjectHandlerBase
    {
        internal bool? _willExtract;
        internal bool? _willProvision;

        private bool _reportProgress = true;

        /// <summary>
        /// The handler's display name, shown in progress reporting.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The handler's stable identifier, used in webhook payloads.
        /// </summary>
        public abstract string InternalName { get; }

        /// <summary>
        /// Whether this handler counts towards the progress total.
        /// </summary>
        public bool ReportProgress
        {
            get { return _reportProgress; }
            set { _reportProgress = value; }
        }

        /// <summary>
        /// Callback the handler reports messages and sub-progress through.
        /// </summary>
        public ProvisioningMessagesDelegate MessagesDelegate { get; set; }

        /// <summary>
        /// Whether this handler has anything to do for the given template.
        /// </summary>
        public abstract bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration);

        /// <summary>
        /// Whether this handler has anything to extract from the given site.
        /// </summary>
        public abstract bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration);

        /// <summary>
        /// Writes this handler's part of the template to the site.
        /// </summary>
        /// <returns>The token parser, with any tokens this handler registered</returns>
        public abstract Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration);

        /// <summary>
        /// Reads this handler's part of the site into the template.
        /// </summary>
        /// <returns>The template, with this handler's contribution added</returns>
        public abstract Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration);

        internal void WriteMessage(string message, ProvisioningMessageType messageType)
        {
            MessagesDelegate?.Invoke(message, messageType);
        }

        internal void WriteSubProgress(string title, string message, int step, int total)
        {
            MessagesDelegate?.Invoke($"{title}|{message}|{step}|{total}", ProvisioningMessageType.Progress);
        }

        #region Tokenization helpers

        /// <summary>
        /// Replaces the term store and term set ids in a taxonomy field's XML with tokens, so the
        /// extracted template is portable between tenants.
        /// </summary>
        protected async Task<string> TokenizeTaxonomyFieldAsync(PnPContext context, XElement element)
        {
            XElement sspIdElement = element.XPathSelectElement("./Customization/ArrayOfProperty/Property[Name = 'SspId']/Value");
            if (sspIdElement != null)
            {
                sspIdElement.Value = "{sitecollectiontermstoreid}";
            }

            XElement termSetIdElement = element.XPathSelectElement("./Customization/ArrayOfProperty/Property[Name = 'TermSetId']/Value");
            if (termSetIdElement != null && Guid.TryParse(termSetIdElement.Value, out Guid termSetId) && termSetId != Guid.Empty)
            {
                try
                {
                    (ITermGroup group, ITermSet termSet) = await TaxonomyLookup.FindTermSetAsync(context, termSetId.ToString()).ConfigureAwait(false);
                    string termSetName = termSet?.LocalizedNames?.FirstOrDefault()?.Name;

                    if (!string.IsNullOrEmpty(termSetName))
                    {
                        string groupToken = group.Scope == TermGroupScope.SiteCollection
                            ? "{sitecollectiontermgroupname}"
                            : group.Name;

                        termSetIdElement.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "{{termsetid:{0}:{1}}}", groupToken, termSetName);
                    }
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, PnPCoreProvisioningResources.TermGroup_No_Access);
                }
            }

            return element.ToString();
        }

        /// <summary>
        /// Checks whether every token in a field's XML was replaced, and - for taxonomy fields -
        /// that the term store and term set it points at actually exist.
        /// </summary>
        protected static async Task<bool> IsFieldXmlValidAsync(string fieldXml, TokenParser parser, PnPContext context)
        {
            IEnumerable<string> leftOverTokens = parser.GetLeftOverTokens(fieldXml);
            if (leftOverTokens.Any())
            {
                return false;
            }

            XElement fieldElement = XElement.Parse(fieldXml);
            if (fieldElement.Attribute("Type")?.Value != "TaxonomyFieldType")
            {
                return true;
            }

            XElement termStoreIdElement = fieldElement.XPathSelectElement("//ArrayOfProperty/Property[Name='SspId']/Value");
            if (termStoreIdElement == null)
            {
                return false;
            }

            try
            {
                XElement termSetIdElement = fieldElement.XPathSelectElement("//ArrayOfProperty/Property[Name='TermSetId']/Value");
                if (termSetIdElement == null)
                {
                    return true;
                }

                (_, ITermSet termSet) = await TaxonomyLookup.FindTermSetAsync(context, termSetIdElement.Value).ConfigureAwait(false);
                return termSet != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces the content type ids in a list view's XML with <c>{listcontenttypeid:...}</c>
        /// tokens, and the site urls with <c>{site}</c>, <c>{themecatalog}</c> and
        /// <c>{masterpagecatalog}</c>.
        /// </summary>
        protected string TokenizeListView(string xml, IList list, IWeb web)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            string newDocumentTemplatesJson = null;
            if (xml.Contains("NewDocumentTemplates"))
            {
                XDocument viewSchema = XDocument.Parse(xml);
                XElement templateElement = viewSchema.Root.Elements().FirstOrDefault(element => element.Name.LocalName == "NewDocumentTemplates");
                if (templateElement != null)
                {
                    newDocumentTemplatesJson = templateElement.Value;
                }
            }

            foreach (IContentType contentType in list.ContentTypes.AsRequested())
            {
                string contentTypeReplacement = ListContentTypeIdToken.CreateToken(list.Title, contentType.StringId);
                if (!string.IsNullOrWhiteSpace(newDocumentTemplatesJson))
                {
                    string contentTypeReplacementJson = contentTypeReplacement.Replace("\\ ", " ").Replace("\\", "\\\\");
                    newDocumentTemplatesJson = Regex.Replace(newDocumentTemplatesJson, contentType.StringId, contentTypeReplacementJson, RegexOptions.IgnoreCase);
                }
                xml = Regex.Replace(xml, contentType.StringId, contentTypeReplacement, RegexOptions.IgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(newDocumentTemplatesJson))
            {
                newDocumentTemplatesJson = TokenizeNewDocumentTemplateUrls(newDocumentTemplatesJson, web);
            }

            string tokenizedXml = TokenizeXml(xml, web);
            if (!string.IsNullOrWhiteSpace(newDocumentTemplatesJson))
            {
                XDocument viewSchema = XDocument.Parse(tokenizedXml);
                XElement templateElement = viewSchema.Root.Elements().FirstOrDefault(element => element.Name.LocalName == "NewDocumentTemplates");
                if (templateElement != null)
                {
                    templateElement.Value = newDocumentTemplatesJson;
                    tokenizedXml = viewSchema.ToString();
                }
            }

            return tokenizedXml;
        }

        /// <summary>
        /// Rewrites the "url" values inside a NewDocumentTemplates JSON array to be site relative.
        /// </summary>
        private static string TokenizeNewDocumentTemplateUrls(string json, IWeb web)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    if (document.RootElement.ValueKind != JsonValueKind.Array)
                    {
                        return json;
                    }

                    var buffer = new System.IO.MemoryStream();
                    using (var writer = new Utf8JsonWriter(buffer))
                    {
                        writer.WriteStartArray();
                        foreach (JsonElement item in document.RootElement.EnumerateArray())
                        {
                            WriteTokenizedTemplate(writer, item, web);
                        }
                        writer.WriteEndArray();
                    }

                    return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
                }
            }
            catch (JsonException)
            {
                return json;
            }
        }

        private static void WriteTokenizedTemplate(Utf8JsonWriter writer, JsonElement element, IWeb web)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                element.WriteTo(writer);
                return;
            }

            writer.WriteStartObject();
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.NameEquals("url") && property.Value.ValueKind == JsonValueKind.String)
                {
                    string original = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(original))
                    {
                        writer.WriteString("url", web.ServerRelativeUrl == "/"
                            ? $"{{site}}/{original.TrimStart('/')}"
                            : Regex.Replace(original, web.ServerRelativeUrl.TrimEnd('/'), "{site}", RegexOptions.IgnoreCase));
                        continue;
                    }
                }

                writer.WritePropertyName(property.Name);
                WriteTokenizedTemplate(writer, property.Value, web);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Replaces site, theme catalog and master page catalog urls in an XML snippet with tokens.
        /// </summary>
        protected string TokenizeXml(string xml, IWeb web)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            bool subsite = IsSubSite(web);

            var themeRegex = new Regex(@"(?<theme>\/_catalogs\/theme)");
            xml = themeRegex.Replace(xml, subsite ? "{sitecollection}/_catalogs/theme" : "{themecatalog}");

            var masterPageRegex = new Regex(@"(?<masterpage>\/_catalogs\/masterpage)");
            xml = masterPageRegex.Replace(xml, subsite ? "{sitecollection}/_catalogs/masterpage" : "{masterpagecatalog}");

            string siteRegexReplacement = "{site}";
            if (web.ServerRelativeUrl == "/")
            {
                siteRegexReplacement += "/";
            }

            xml = Regex.Replace(xml, "(\"" + web.ServerRelativeUrl + ")(?!&)", "\"" + siteRegexReplacement, RegexOptions.IgnoreCase);
            xml = Regex.Replace(xml, "'" + web.ServerRelativeUrl, "'" + siteRegexReplacement, RegexOptions.IgnoreCase);
            xml = Regex.Replace(xml, ">" + web.ServerRelativeUrl, ">" + siteRegexReplacement, RegexOptions.IgnoreCase);

            return xml;
        }

        /// <summary>
        /// The server relative path of a web, from either an absolute url or a path.
        /// </summary>
        internal static string ServerRelativePathOf(string webUrl)
        {
            if (string.IsNullOrWhiteSpace(webUrl))
            {
                return null;
            }

            if (Uri.TryCreate(webUrl, UriKind.Absolute, out Uri absolute))
            {
                return Uri.UnescapeDataString(absolute.PathAndQuery);
            }

            string path = Uri.UnescapeDataString(webUrl);

            return path.StartsWith("/", StringComparison.Ordinal) ? path : null;
        }

        /// <summary>
        /// Replaces a url with the token form the engine can re-resolve on another site.
        /// </summary>
        /// <param name="url">The url to tokenize</param>
        /// <param name="webUrl">
        /// The web's url, <b>either</b> absolute or server relative - see
        /// <see cref="ServerRelativePathOf"/> for why both have to work.
        /// </param>
        /// <param name="web">The web, when the caller has it, so sub site scoping can be decided</param>
        protected string Tokenize(string url, string webUrl, IWeb web = null)
        {
            string result = null;

            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            url = Uri.UnescapeDataString(url);

            bool subsite = web != null && IsSubSite(web);

            if (url.IndexOf("/_catalogs/theme", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = url.Substring(url.IndexOf("/_catalogs/theme", StringComparison.InvariantCultureIgnoreCase))
                    .Replace("/_catalogs/theme", subsite ? "{sitecollection}/_catalogs/theme" : "{themecatalog}");
            }

            if (url.IndexOf("/_catalogs/masterpage", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = url.Substring(url.IndexOf("/_catalogs/masterpage", StringComparison.InvariantCultureIgnoreCase))
                    .Replace("/_catalogs/masterpage", subsite ? "{sitecollection}/_catalogs/masterpage" : "{masterpagecatalog}");
            }

            if (result != null)
            {
                url = result;
            }

            string webPath = ServerRelativePathOf(webUrl);

            if (!string.IsNullOrEmpty(webPath))
            {
                if (url.IndexOf(webPath, StringComparison.InvariantCultureIgnoreCase) > -1
                    && url.IndexOf("{masterpagecatalog}", StringComparison.Ordinal) == -1
                    && url.IndexOf("{themecatalog}", StringComparison.Ordinal) == -1)
                {
                    result = (webPath.Equals("/", StringComparison.Ordinal) && url.StartsWith(webPath, StringComparison.Ordinal))
                        ? "{site}" + url // needed for the DocumentTemplate attribute of pnp:ListInstance on a root site ("/") without a managed path
                        : url.Replace(webPath, "{site}");
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = url;
            }

            return result;
        }

        /// <summary>
        /// Whether the web is a sub site rather than the root web of its site collection.
        /// </summary>
        internal static bool IsSubSite(IWeb web)
        {
            if (web == null
                || !web.IsPropertyAvailable(w => w.ServerRelativeUrl)
                || web.ServerRelativeUrl == null)
            {
                return false;
            }

            string siteUrl = web.PnPContext.Site.IsPropertyAvailable(s => s.ServerRelativeUrl)
                ? web.PnPContext.Site.ServerRelativeUrl
                : null;

            if (siteUrl == null)
            {
                return false;
            }

            return !web.ServerRelativeUrl.TrimEnd('/').Equals(siteUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Whether the context's web is a sub site, loading what the answer depends on first.
        /// </summary>
        internal static async Task<bool> IsSubSiteAsync(PnPContext context)
        {
            await context.Site.LoadAsync(s => s.RootWeb.QueryProperties(w => w.Id)).ConfigureAwait(false);
            await context.Web.LoadAsync(w => w.Id).ConfigureAwait(false);

            return context.Site.RootWeb.Id != context.Web.Id;
        }

        #endregion
    }
}
