using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{termsetid:[groupname]:[termsetname]}",
      Description = "Returns the id of a term set given its name and its parent group",
      Example = "{termsetid:MyGroup:MyTermset}",
      Returns = "9188a794-cfcf-48b6-9ac5-df2048e8aa5d")]
    internal class TermSetIdToken : TokenDefinition
    {
        private readonly string _value = null;

        public TermSetIdToken(PnPContext context, string groupName, string termsetName, Guid id)
            : this(context, groupName, termsetName, id.ToString())
        {
        }

        /// <summary>
        /// Creates the token from a term set id already in string form - the shape the Graph
        /// term store returns. See <see cref="SiteCollectionTermSetIdToken"/> for why both
        /// overloads exist.
        /// </summary>
        public TermSetIdToken(PnPContext context, string groupName, string termsetName, string id)
            : base(context, $"{{termsetid:{Regex.Escape(groupName)}:{Regex.Escape(termsetName)}}}")
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