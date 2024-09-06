using PnP.Core.Services;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class ActivityHandler
    {
        internal static async Task<System.Collections.Generic.List<IActivityStat>> GetAnalyticsAsync(TransientObject caller, AnalyticsOptions options = null)
        {

            string baseUrl;
            if (caller is Site site)
            {
                baseUrl = $"sites/{site.Id}/";
            }
            else if (caller is File file)
            {
                await file.EnsurePropertiesAsync(p => p.VroomDriveID, p => p.VroomItemID).ConfigureAwait(false);

                baseUrl = $"sites/{file.PnPContext.Site.Id}/drives/{file.VroomDriveID}/items/{file.VroomItemID}/";
            }
            else
            {
                return null;
            }

            var apiCall = new ApiCall($"{baseUrl}{OptionsToQuery(options)}", ApiType.Graph);

            var result = await ((caller as IDataModelWithContext).PnPContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return DeserializeAnalytics(result.Json);
            }

            return null;
        }

        private static System.Collections.Generic.List<IActivityStat> DeserializeAnalytics(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            System.Collections.Generic.List<IActivityStat> results = new System.Collections.Generic.List<IActivityStat>();

            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("value", out JsonElement value) && value.ValueKind == JsonValueKind.Array)
            {
                foreach(var activityStat in value.EnumerateArray())
                {
                    results.Add(DeserializedActivityStat(activityStat));
                }
            }
            else
            {
                results.Add(DeserializedActivityStat(json));
            }

            return results;
        }

        private static IActivityStat DeserializedActivityStat(JsonElement activityStat)
        {
            ActivityStat stat = new ActivityStat();

            if (activityStat.TryGetProperty("startDateTime", out JsonElement startDateTime))
            {
                stat.StartDateTime = startDateTime.GetDateTime();
            }
            
            if (activityStat.TryGetProperty("endDateTime", out JsonElement endDateTime))
            {
                stat.EndDateTime = endDateTime.GetDateTime();
            }
            
            if (activityStat.TryGetProperty("isTrending", out JsonElement isTrending))
            {
                stat.IsTrending = isTrending.GetBoolean();
            }

            if (activityStat.TryGetProperty("access", out JsonElement access))
            {
                stat.Access = DeserializeActivityActionStat(access);
            }

            if (activityStat.TryGetProperty("edit", out JsonElement edit))
            {
                stat.Edit = DeserializeActivityActionStat(edit);
            }

            if (activityStat.TryGetProperty("create", out JsonElement create))
            {
                stat.Create = DeserializeActivityActionStat(create);
            }

            if (activityStat.TryGetProperty("delete", out JsonElement delete))
            {
                stat.Delete = DeserializeActivityActionStat(delete);
            }

            if (activityStat.TryGetProperty("move", out JsonElement move))
            {
                stat.Move = DeserializeActivityActionStat(move);
            }

            if (activityStat.TryGetProperty("incompleteData", out JsonElement incompleteData))
            {
                stat.IncompleteData = DeserializeActivityIncomplete(incompleteData);
            }

            return stat;
        }

        private static IActivityActionStat DeserializeActivityActionStat(JsonElement actionStat)
        {
            ActivityActionStat stat = new ActivityActionStat();

            if (actionStat.TryGetProperty("actionCount", out JsonElement actionCount))
            {
                stat.ActionCount = actionCount.GetInt32();
            }

            if (actionStat.TryGetProperty("actorCount", out JsonElement actorCount))
            {
                stat.ActorCount = actorCount.GetInt32();
            }

            if (actionStat.TryGetProperty("timeSpentInSeconds", out JsonElement timeSpentInSeconds))
            {
                stat.TimeSpentInSeconds = timeSpentInSeconds.GetInt32();
            }

            return stat;
        }

        private static IActivityIncomplete DeserializeActivityIncomplete(JsonElement incomplete)
        {
            ActivityIncomplete data = new ActivityIncomplete();

            if (incomplete.TryGetProperty("wasThrottled", out JsonElement wasThrottled))
            {
                data.WasThrottled = wasThrottled.GetBoolean();
            }

            if (incomplete.TryGetProperty("resultsPending", out JsonElement resultsPending))
            {
                data.ResultsPending = resultsPending.GetBoolean();
            }

            if (incomplete.TryGetProperty("notSupported", out JsonElement notSupported))
            {
                data.NotSupported = notSupported.GetBoolean();
            }

            if (incomplete.TryGetProperty("missingDataBeforeDateTime", out JsonElement missingDataBeforeDateTime))
            {
                data.MissingDataBeforeDateTime = missingDataBeforeDateTime.GetDateTime();
            }

            // When there's no incomplete data set the object to null to make it easier for the consumer
            if (!data.WasThrottled && !data.ResultsPending && !data.NotSupported && data.MissingDataBeforeDateTime == DateTime.MinValue)
            {
                return null;
            }
            else
            {
                return data;
            }
        }

        private static string OptionsToQuery(AnalyticsOptions options)
        {
            if (options == null)
            {
                return "analytics/allTime";
            }
            
            if (options.Interval == AnalyticsInterval.AllTime)
            {
                return "analytics/allTime";
            }
            else if (options.Interval == AnalyticsInterval.LastSevenDays)
            {
                return "analytics/lastSevenDays";
            }
            else if (options.Interval == AnalyticsInterval.Custom)
            {
                // Just return last seven days whenever the user selected custom without providing the needed dates
                if (options.CustomEndDate == DateTime.MinValue || options.CustomStartDate == DateTime.MinValue)
                {
                    return "analytics/lastSevenDays";
                }

                return $"getActivitiesByInterval(startDateTime='{options.CustomStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}',endDateTime='{options.CustomEndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}',interval='{options.CustomAggregationInterval.ToString().ToLowerInvariant()}')";
            }

            return "analytics/allTime";
        }

    }

}
