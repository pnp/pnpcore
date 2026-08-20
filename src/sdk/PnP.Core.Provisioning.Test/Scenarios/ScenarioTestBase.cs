using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.Providers.Xml;
using PnP.Core.Provisioning.Test.Live;
using PnP.Core.Provisioning.Test.Scenarios.Validation;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Test.Scenarios
{
    /// <summary>
    /// The shared machinery behind phase 9's five end-to-end scenarios.
    /// </summary>
    public abstract class ScenarioTestBase : LiveTestBase
    {
        /// <summary>
        /// Runs one scenario end to end.
        /// </summary>
        /// <param name="name">Short name, used in the site url and in the console output</param>
        /// <param name="template">The template to apply</param>
        /// <param name="handlers">The handlers to extract with - the same ones the scenario exercises</param>
        /// <param name="assert">
        /// Called with the re-extracted template and a context bound to the live site.
        /// </param>
        /// <param name="configureExtract">
        /// Optional. Adjusts the extract configuration before it runs - for the scenarios whose
        /// artefacts the default extract deliberately leaves out.
        /// </param>
        /// <param name="allowScripting">
        /// Turns NoScript off on the scenario's site. Opt in, and only for scenarios that need it -
        /// see <see cref="AllowScriptingAsync"/>.
        /// </param>
        protected async Task RunScenarioAsync(string name, ProvisioningTemplate template,
            IEnumerable<ConfigurationHandler> handlers,
            Func<ProvisioningTemplate, PnPContext, Task> assert,
            Action<ExtractConfiguration> configureExtract = null,
            bool allowScripting = false)
        {
            string fixture = Guid.NewGuid().ToString("N").Substring(0, 12);
            Uri siteUrl = null;

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdkscenario{name.ToLowerInvariant()}{fixture}");

                    try
                    {
                        await CreateSiteAsync(admin, context, siteUrl, name).ConfigureAwait(false);

                        if (allowScripting)
                        {
                            await AllowScriptingAsync(admin, siteUrl).ConfigureAwait(false);
                        }

                        using (PnPContext site = await context.CloneAsync(siteUrl).ConfigureAwait(false))
                        {
                            await ApplyAsync(site, template).ConfigureAwait(false);
                        }

                        ProvisioningTemplate extracted;

                        using (PnPContext reader = await context.CloneAsync(siteUrl).ConfigureAwait(false))
                        {
                            extracted = await ExtractAsync(reader, handlers, configureExtract).ConfigureAwait(false);
                        }

                        using (PnPContext asserting = await context.CloneAsync(siteUrl).ConfigureAwait(false))
                        {
                            await assert(extracted, asserting).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await DeleteSiteAsync(siteUrl).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task CreateSiteAsync(PnPContext admin, PnPContext seed, Uri siteUrl, string name)
        {
            Console.WriteLine($"Creating {siteUrl}");

            string owner = await SiteOwnerAsync(seed).ConfigureAwait(false);

            var options = new CommunicationSiteOptions(siteUrl, $"{TestPrefix}Scenario_{name}")
            {
                Description = "Created by the PnP Core provisioning scenario tests",
                Language = Language.English,
                Owner = owner,
            };

            using (PnPContext created = await admin.GetSiteCollectionManager()
                .CreateSiteCollectionAsync(options, CreationOptions(seed)).ConfigureAwait(false))
            {
                Console.WriteLine($"Created {created.Uri}");
            }
        }

                  /// <summary>
        /// Applies the template and fails on anything it reported.
        /// </summary>
        private static async Task ApplyAsync(PnPContext site, ProvisioningTemplate template)
        {
            var problems = new List<string>();

            await site.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    Console.WriteLine($"[{type}] {message}");

                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        problems.Add(message);
                    }
                },
            }).ConfigureAwait(false);

            Assert.AreEqual(0, problems.Count,
                $"Applying the template reported problems:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems));
        }

        private static async Task<ProvisioningTemplate> ExtractAsync(PnPContext site,
            IEnumerable<ConfigurationHandler> handlers, Action<ExtractConfiguration> configure)
        {
            var configuration = new ExtractConfiguration();

            foreach (ConfigurationHandler handler in handlers)
            {
                configuration.Handlers.Add(handler);
            }

            configure?.Invoke(configuration);

            ProvisioningTemplate extracted = await site.GetProvisioningManager()
                .GetTemplateAsync(configuration).ConfigureAwait(false);

            Console.WriteLine($"Extracted: {extracted.Lists.Count} list(s), {extracted.SiteFields.Count} field(s), " +
                $"{extracted.ContentTypes.Count} content type(s), {extracted.TermGroups.Count} term group(s)");

            return extracted;
        }

        /// <summary>
        /// Serialises a template to XML, for diffing.
        /// </summary>
        protected static string ToXml(ProvisioningTemplate template)
        {
            using (Stream stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Compares two templates' XML, ignoring what SharePoint assigns rather than the template.
        /// </summary>
        protected static XmlEqualityResultWrapper CompareIgnoringServerValues(string expectedXml, string actualXml,
            params string[] additionalAttributesToIgnore)
        {
            var ignore = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ID", "Version", "Author", "Generator", "Created", "Modified", "SiteUrl", "Url",
                "SourceID", "StaticName", "ColName", "RowOrdinal", "WebId", "ListId",
            };

            foreach (string extra in additionalAttributesToIgnore)
            {
                ignore.Add(extra);
            }

            XElement expected = Normalise(XDocument.Parse(expectedXml).Root, ignore);
            XElement actual = Normalise(XDocument.Parse(actualXml).Root, ignore);

            XmlEqualityResult result = XmlComparer.AreEqual(expected, actual);

            return new XmlEqualityResultWrapper
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                FailObject = result.FailObject?.ToString(),
            };
        }

        private static XElement Normalise(XElement element, HashSet<string> ignore)
        {
            var copy = new XElement(element);

            foreach (XElement descendant in copy.DescendantsAndSelf().ToList())
            {
                foreach (XAttribute attribute in descendant.Attributes()
                    .Where(a => !a.IsNamespaceDeclaration && ignore.Contains(a.Name.LocalName))
                    .ToList())
                {
                    attribute.Remove();
                }
            }

            return copy;
        }

        /// <summary>
        /// Deletes the scenario's site, recycle bin included, without ever throwing.
        /// </summary>
        private static async Task DeleteSiteAsync(Uri siteUrl)
        {
            if (siteUrl == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await GetContextAsync(3).ConfigureAwait(false))
                {
                    ISiteCollectionManager manager = context.GetSiteCollectionManager();

                    if (await manager.SiteExistsAsync(siteUrl).ConfigureAwait(false))
                    {
                        await manager.DeleteSiteCollectionAsync(siteUrl).ConfigureAwait(false);
                        Console.WriteLine($"Deleted {siteUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE {siteUrl} - delete it by hand.{Environment.NewLine}{Describe(ex)}");
            }
        }

        /// <summary>
        /// A comparison result that does not hold on to the parsed document.
        /// </summary>
        protected sealed class XmlEqualityResultWrapper
        {
            public bool Success { get; set; }

            public string ErrorMessage { get; set; }

            public string FailObject { get; set; }

            public override string ToString()
            {
                return Success ? "Success" : $"{ErrorMessage}{Environment.NewLine}{FailObject}";
            }
        }
    }
}
