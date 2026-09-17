using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{termstoreid:[storename]}",
      Description = "Returns the id of a term store given its name",
      Example = "{termstoreid:MyTermStore}",
      Returns = "9188a794-cfcf-48b6-9ac5-df2048e8aa5d")]
    internal class TermStoreIdToken : TokenDefinition
    {
        private readonly string _value = null;

        public TermStoreIdToken(PnPContext context, string storeName, Guid id)
            : this(context, storeName, id.ToString())
        {
        }

        /// <summary>
        /// Creates the token from a term store id already in string form - the shape the Graph
        /// term store returns. See <see cref="SiteCollectionTermSetIdToken"/> for why both
        /// overloads exist.
        /// </summary>
        public TermStoreIdToken(PnPContext context, string storeName, string id)
            : base(context, $"{{termstoreid:{Regex.Escape(storeName)}}}")
        {
            _value = id;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _value;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}