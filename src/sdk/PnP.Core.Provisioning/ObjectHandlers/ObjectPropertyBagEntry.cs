using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyBagEntryModel = PnP.Core.Provisioning.Model.PropertyBagEntry;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts web property bag entries, including their search-indexed flag.
    /// </summary>
    internal class ObjectPropertyBagEntry : ObjectHandlerBase
    {
        /// <summary>
        /// The property bag key holding the search-indexed property list.
        /// </summary>
        private const string IndexedPropertyKeysName = "vti_indexedpropertykeys";

        /// <summary>
        /// Prefixes marking a property SharePoint owns.
        /// </summary>
        private static readonly string[] SystemPropertyPrefixes =
        {
            "_", "vti_", "dlc_", "ecm_", "profileschemaversion", "DesignPreview",
        };

        public override string Name => "Property bag entries";

        public override string InternalName => "PropertyBagEntries";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.PropertyBagEntries != null && template.PropertyBagEntries.Count > 0;
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.AllProperties, w => w.ServerRelativeUrl).ConfigureAwait(false);

                List<string> indexedProperties = GetIndexedPropertyKeys(web);

                var entries = new List<PropertyBagEntryModel>();
                foreach (KeyValuePair<string, object> property in web.AllProperties.Values)
                {
                    entries.Add(new PropertyBagEntryModel
                    {
                        Key = property.Key,
                        Value = property.Value?.ToString(),
                        Indexed = indexedProperties.Contains(property.Key),
                    });
                }

                template.PropertyBagEntries.Clear();
                template.PropertyBagEntries.AddRange(entries);


                foreach (PropertyBagEntryModel entry in template.PropertyBagEntries)
                {
                    entry.Value = Tokenize(entry.Value, web.ServerRelativeUrl, web);
                }

                return template;
            }
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
                {
                    string message = "This is a NoScript site, so property bag entries cannot be written. Skipping them.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    return parser;
                }

                IWeb web = await context.Web.GetAsync(w => w.AllProperties).ConfigureAwait(false);

                bool overwriteSystemValues = configuration?.ToApplyingInformation()?.OverwriteSystemPropertyBagValues ?? false;
                var indexedKeysToAdd = new List<string>();
                bool dirty = false;

                foreach (PropertyBagEntryModel entry in template.PropertyBagEntries)
                {
                    bool exists = web.AllProperties.Values.ContainsKey(entry.Key);

                    if (!entry.Overwrite && exists)
                    {
                        continue;
                    }

                    bool isSystemProperty = SystemPropertyPrefixes.Any(
                        p => entry.Key.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                    if (isSystemProperty && !overwriteSystemValues)
                    {
                        context.Logger?.LogInformation(
                            "{Source}: skipping system property bag entry {Key} - set OverwriteSystemPropertyBagValues to write it",
                            Constants.LOGGING_SOURCE, entry.Key);
                        continue;
                    }

                    web.AllProperties[entry.Key] = parser.ParseString(entry.Value);
                    dirty = true;

                    if (entry.Indexed)
                    {
                        indexedKeysToAdd.Add(entry.Key);
                    }
                }

                if (dirty)
                {
                    await web.AllProperties.UpdateAsync().ConfigureAwait(false);
                }

                foreach (string key in indexedKeysToAdd)
                {
                    bool indexed = await web.AddIndexedPropertyAsync(key).ConfigureAwait(false);

                    if (!indexed)
                    {
                        string message = $"Could not mark property bag entry '{key}' as indexed.";
                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                        WriteMessage(message, ProvisioningMessageType.Warning);
                    }
                }

                return parser;
            }
        }

        /// <summary>
        /// Reads the search-indexed property keys back out of the property bag.
        /// </summary>
        private static List<string> GetIndexedPropertyKeys(IWeb web)
        {
            var keys = new List<string>();

            string raw = web.AllProperties.GetString(IndexedPropertyKeysName, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return keys;
            }

            foreach (string encoded in raw.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    keys.Add(Encoding.Unicode.GetString(Convert.FromBase64String(encoded)));
                }
                catch (FormatException)
                {
                }
            }

            return keys;
        }
    }
}
