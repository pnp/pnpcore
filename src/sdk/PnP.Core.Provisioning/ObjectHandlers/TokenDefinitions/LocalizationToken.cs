using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{localization:[key]}",
     Description = "Returns a value from a resource file provided in the template, given the locale of the site the template is applied to",
     Example = "{localization:MyListTitle}",
     Returns = "My List Title")]
    internal class LocalizationToken : TokenDefinition
    {
        private readonly int _webLanguage;
        private readonly int? _defaultLcid;
        private readonly Dictionary<int, ResourceEntry> _entriesByLanguage;

        public IReadOnlyList<ResourceEntry> ResourceEntries { get; }

        public LocalizationToken(PnPContext context, int webLanguage, string key, List<ResourceEntry> resourceEntries, int? defaultLcid)
            : base(context,
                  $"{{loc:{Regex.Escape(key)}}}",
                  $"{{localize:{Regex.Escape(key)}}}",
                  $"{{localization:{Regex.Escape(key)}}}",
                  $"{{resource:{Regex.Escape(key)}}}",
                  $"{{res:{Regex.Escape(key)}}}")
        {
            ResourceEntries = resourceEntries;
            _defaultLcid = defaultLcid;

            // The web language is passed in rather than read off the context: the parser has
            // already loaded it, and a constructor cannot await.
            _webLanguage = webLanguage;
            _entriesByLanguage = new Dictionary<int, ResourceEntry>(capacity: resourceEntries.Count + 1);

            for (var index = 0; index < resourceEntries.Count; index++)
            {
                ResourceEntry entry = resourceEntries[index];
                _entriesByLanguage[entry.LCID] = entry;
            }
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (_entriesByLanguage.TryGetValue(_webLanguage, out ResourceEntry entry)
                // Fallback to the default LCID.
                || (_defaultLcid.HasValue && _entriesByLanguage.TryGetValue(_defaultLcid.Value, out entry)))
            {
                return Task.FromResult(entry.Value);
            }

            // Final fallback: the first entry supplied, whatever its language.
            return Task.FromResult(ResourceEntries[0].Value);
        }
    }
}