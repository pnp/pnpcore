using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{sitecollectiontermsetid:[termsetname]}",
        Description = "Returns the id of the given termset name located in the site collection term group",
        Example = "{sitecollectiontermsetid:MyTermset}",
        Returns = "9188a794-cfcf-48b6-9ac5-df2048e8aa5d")]
    internal class SiteCollectionTermSetIdToken : TokenDefinition
    {
        private readonly string _value;

        public SiteCollectionTermSetIdToken(PnPContext context, string termsetName, Guid id)
            : this(context, termsetName, id.ToString())
        {
        }

        /// <summary>
        /// Creates the token from a term set id already in string form.
        /// </summary>
        public SiteCollectionTermSetIdToken(PnPContext context, string termsetName, string id)
            : base(context, $"{{sitecollectiontermsetid:{Regex.Escape(termsetName)}}}")
        {
            _value = id;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _value;
            }
            return Task.FromResult(CacheValue);
        }
    }
}
