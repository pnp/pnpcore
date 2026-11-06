using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    /// <summary>
    /// Returns the current date and time in the universal round-trip format.
    /// </summary>
    [TokenDefinitionDescription(
       Token = "{now}",
       Description = "Returns the current date in universal date time format: yyyy-MM-ddTHH:mm:ss.fffK",
       Example = "{now}",
       Returns = "2018-04-18T15:44:45.898+02:00")]
    public class DateNowToken : TokenDefinition
    {
        public DateNowToken(PnPContext context)
            : base(context, "{now}")
        {
            IsCacheable = false;
        }

        public override string GetReplaceValue()
        {
            return DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture);
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult(GetReplaceValue());
        }
    }
}
