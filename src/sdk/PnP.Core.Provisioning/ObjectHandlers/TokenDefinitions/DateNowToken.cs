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
            // The value moves on every use, so it can never be cached.
            IsCacheable = false;
        }

        public override string GetReplaceValue()
        {
            // ToUniversalTime yields a UTC DateTime, so the K specifier renders "Z" - the "+02:00"
            // in the documented example is what the original comment claimed rather than what the
            // original code produced. Behaviour is reproduced exactly; only the doc is honest now.
            // CultureInfo.InvariantCulture is explicit here because the engine sets the thread
            // culture from the template while applying.
            return DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture);
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult(GetReplaceValue());
        }
    }
}