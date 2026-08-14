using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web
{
    /// <summary>
    /// Writes the web's regional settings.
    /// </summary>
    internal sealed class SetRegionalSettingsRequest : IRequest<object>
    {
        /// <summary>
        /// The CSOM type name each settable property expects.
        /// </summary>
        private static readonly Dictionary<string, string> PropertyTypes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AdjustHijriDays"] = "Int16",
            ["AlternateCalendarType"] = "Int16",
            ["CalendarType"] = "Int16",
            ["Collation"] = "Int16",
            ["FirstDayOfWeek"] = "UInt32",
            ["FirstWeekOfYear"] = "Int16",
            ["LocaleId"] = "UInt32",
            ["ShowWeeks"] = "Boolean",
            ["Time24"] = "Boolean",
            ["WorkDayEndHour"] = "Int16",
            ["WorkDayStartHour"] = "Int16",
            ["WorkDays"] = "Int16",
        };

        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly IDictionary<string, object> properties;

        internal SetRegionalSettingsRequest(Guid siteId, Guid webId, IDictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                throw new ArgumentException("At least one regional setting is required.", nameof(properties));
            }

            foreach (string name in properties.Keys)
            {
                if (!PropertyTypes.ContainsKey(name))
                {
                    throw new ArgumentException(
                        $"'{name}' is not a settable regional setting. Settable: {string.Join(", ", PropertyTypes.Keys)}.",
                        nameof(properties));
                }
            }

            this.siteId = siteId;
            this.webId = webId;
            this.properties = properties;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int webIdentityId = idProvider.GetActionId();
            int regionalSettingsId = idProvider.GetActionId();

            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = webIdentityId,
                    Name = CsomIdentity.Web(siteId, webId)
                }
            });

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = regionalSettingsId.ToString()
                },
                ObjectPath = new Property
                {
                    Id = regionalSettingsId,
                    ParentId = webIdentityId,
                    Name = "RegionalSettings"
                }
            });

            foreach (KeyValuePair<string, object> property in properties)
            {
                paths.Add(new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = regionalSettingsId.ToString(),
                        Name = property.Key,
                        SetParameter = new Parameter
                        {
                            Type = PropertyTypes[property.Key],
                            Value = property.Value
                        }
                    }
                });
            }

            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = regionalSettingsId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter>()
                }
            });

            return paths;
        }

        /// <summary>
        /// The raw CSOM response body, kept for diagnosis.
        /// </summary>
        internal string RawResponse { get; private set; }

        public void ProcessResponse(string response)
        {
            RawResponse = response;
        }
    }
}
