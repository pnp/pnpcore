using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using PnP.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.BaseTemplates
{
    /// <summary>
    /// Provides the out-of-the-box template for a given site template, so that extraction can
    /// report only what actually differs from a fresh site of the same kind.
    /// </summary>
    public static class BaseTemplateManager
    {
        private const string ResourcePrefix = "PnP.Core.Provisioning.BaseTemplates.SPO.";

        /// <summary>
        /// Gets the out-of-the-box template matching the web's own site template.
        /// </summary>
        /// <param name="web">The web whose base template is wanted</param>
        /// <returns>The base template, or <c>null</c> when none ships for that site template</returns>
        public static async Task<ProvisioningTemplate> GetBaseTemplateAsync(this IWeb web)
        {
            if (web == null)
            {
                throw new ArgumentNullException(nameof(web));
            }

            await web.LoadAsync(w => w.WebTemplate, w => w.WebTemplateConfiguration).ConfigureAwait(false);

            return GetBaseTemplate(web.PnPContext, web.WebTemplate, GetConfiguration(web.WebTemplateConfiguration));
        }

        /// <summary>
        /// Gets the out-of-the-box template for a named web template and configuration.
        /// </summary>
        /// <param name="context">Context used for logging</param>
        /// <param name="webTemplate">The web template name, e.g. <c>STS</c></param>
        /// <param name="configuration">The web template configuration, e.g. <c>0</c></param>
        /// <returns>The base template, or <c>null</c> when none ships for that combination</returns>
        public static ProvisioningTemplate GetBaseTemplate(PnPContext context, string webTemplate, short configuration)
        {
            try
            {
                string resourceName = $"{ResourcePrefix}{webTemplate}{configuration}Template.xml";

                using (Stream stream = typeof(BaseTemplateManager).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        // No base template ships for this site template. Extraction still works;
                        // it simply reports everything rather than only the differences.
                        return null;
                    }

                    // Figure out which schema version the template was written against, so the
                    // matching formatter is used.
                    XDocument document = XDocument.Load(stream);
                    XNamespace pnpNamespace = document.Root.Attributes()
                        .Where(a => a.IsNamespaceDeclaration)
                        .GroupBy(
                            a => a.Name.Namespace == XNamespace.None ? string.Empty : a.Name.LocalName,
                            a => XNamespace.Get(a.Value))
                        .ToDictionary(g => g.Key, g => g.First())["pnp"];

                    stream.Seek(0, SeekOrigin.Begin);

                    ITemplateFormatter formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(pnpNamespace.NamespaceName);
                    return formatter.ToProvisioningTemplate(stream);
                }
            }
            catch (Exception ex)
            {
                context?.Logger?.LogError(ex, "{Source}: Error occurred while retrieving the base template for {WebTemplate}{Configuration}",
                    Constants.LOGGING_SOURCE, webTemplate, configuration);
                return null;
            }
        }

        /// <summary>
        /// Splits a web template configuration string such as <c>STS#0</c> into its numeric part.
        /// </summary>
        internal static short GetConfiguration(string webTemplateConfiguration)
        {
            if (string.IsNullOrEmpty(webTemplateConfiguration))
            {
                return 0;
            }

            int separator = webTemplateConfiguration.IndexOf('#');
            string configuration = separator > -1
                ? webTemplateConfiguration.Substring(separator + 1)
                : webTemplateConfiguration;

            return short.TryParse(configuration, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out short result)
                ? result
                : (short)0;
        }
    }
}
