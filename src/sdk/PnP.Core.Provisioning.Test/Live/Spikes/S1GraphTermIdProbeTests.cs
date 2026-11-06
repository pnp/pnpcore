using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Spikes
{
    /// <summary>
    /// Spike <b>S1</b>'s remaining tenant probe: can the Graph term store be made to preserve a
    /// caller-supplied term id?
    /// </summary>
    [TestClass]
    public class S1GraphTermIdProbeTests : LiveTestBase
    {
        /// <summary>
        /// The id we try to impose. Fixed rather than random so a leaked object is recognisable.
        /// </summary>
        private static readonly Guid DesiredId = new Guid("5115f7d0-5115-4f7d-9115-f7d05115f7d0");

        /// <summary>
        /// Builds the site-scoped term store base path Graph actually uses.
        /// </summary>
        private static async Task<string> TermStoreBaseAsync(PnPContext context)
        {
            await context.Site.EnsurePropertiesAsync(s => s.Id).ConfigureAwait(false);
            await context.Web.EnsurePropertiesAsync(w => w.Id).ConfigureAwait(false);

            return $"sites/{context.Uri.DnsSafeHost},{context.Site.Id},{context.Web.Id}/termstore";
        }

        /// <summary>
        /// Proves the probe can reach the term store before anything is concluded from a failure.
        /// </summary>
        private static async Task AssertEndpointIsReachableAsync(PnPContext context, string termStoreBase)
        {
            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(
                    new ApiRequest(ApiRequestType.Graph, $"{termStoreBase}/groups")).ConfigureAwait(false);

                Assert.IsFalse(string.IsNullOrEmpty(response.Response),
                    "The term store GET returned nothing - the probe cannot conclude anything about ids.");

                Console.WriteLine($"S1 probe: reached {termStoreBase}/groups OK - a POST failure from here is meaningful.");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(
                    $"Could not read the term store, so nothing can be concluded about supplied ids. " +
                    $"Most likely the account lacks term store admin rights.{Environment.NewLine}" +
                    $"Path: {termStoreBase}/groups{Environment.NewLine}{ex.Message}");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("S1")]
        public async Task Probe_DoesRawGraphAcceptACallerSuppliedTermGroupId()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string termStoreBase = await TermStoreBaseAsync(context).ConfigureAwait(false);
                await AssertEndpointIsReachableAsync(context, termStoreBase).ConfigureAwait(false);

                await CleanUpLeakedTermGroupsAsync(context).ConfigureAwait(false);

                string groupName = $"{TestPrefix}S1Probe_{DateTime.UtcNow:yyyyMMddHHmmss}";
                string createdId = null;

                try
                {
                    string body = JsonSerializer.Serialize(new
                    {
                        id = DesiredId.ToString(),
                        displayName = groupName,
                        description = "safe to delete",
                    });

                    ApiRequestResponse response = await context.Web.ExecuteRequestAsync(
                        new ApiRequest(HttpMethod.Post, ApiRequestType.Graph, $"{termStoreBase}/groups", body))
                        .ConfigureAwait(false);

                    using (JsonDocument document = JsonDocument.Parse(response.Response))
                    {
                        createdId = document.RootElement.TryGetProperty("id", out JsonElement id) ? id.GetString() : null;
                    }

                    Console.WriteLine($"term GROUP: requested {DesiredId}, Graph ACCEPTED the call and returned {createdId ?? "<none>"}");

                    Assert.AreNotEqual(DesiredId.ToString(), createdId,
                        "Graph PRESERVED a caller-supplied term group id. That contradicts: the taxonomy " +
                        "CSOM fallback may be unnecessary and eight requests could be deleted.");

                    Console.WriteLine("term GROUP: id was IGNORED and reassigned.");
                }
                catch (AssertFailedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    AssertRejectedForTheRightReason("term GROUP", ex);
                }
                finally
                {
                    await DeleteTermGroupDeepAsync(context, createdId).ConfigureAwait(false);
                    await CleanUpLeakedTermGroupsAsync(context).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("S1")]
        public async Task Probe_DoesRawGraphAcceptACallerSuppliedTermSetId()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string termStoreBase = await TermStoreBaseAsync(context).ConfigureAwait(false);
                await AssertEndpointIsReachableAsync(context, termStoreBase).ConfigureAwait(false);

                string groupName = $"{TestPrefix}S1SetProbe_{DateTime.UtcNow:yyyyMMddHHmmss}";
                ITermGroup group = null;
                string createdSetId = null;

                try
                {
                    group = await context.TermStore.Groups.AddAsync(groupName, "safe to delete")
                        .ConfigureAwait(false);

                    string body = JsonSerializer.Serialize(new
                    {
                        id = DesiredId.ToString(),
                        localizedNames = new[] { new { languageTag = "en-US", name = $"{TestPrefix}Set" } },
                        parentGroup = new { id = group.Id },
                    });

                    ApiRequestResponse response = await context.Web.ExecuteRequestAsync(
                        new ApiRequest(HttpMethod.Post, ApiRequestType.Graph, $"{termStoreBase}/sets", body))
                        .ConfigureAwait(false);

                    using (JsonDocument document = JsonDocument.Parse(response.Response))
                    {
                        createdSetId = document.RootElement.TryGetProperty("id", out JsonElement id) ? id.GetString() : null;
                    }

                    Console.WriteLine($"term SET: requested {DesiredId}, Graph ACCEPTED the call and returned {createdSetId ?? "<none>"}");

                    Assert.AreNotEqual(DesiredId.ToString(), createdSetId,
                        "Graph PRESERVED a caller-supplied term set id.");

                    Console.WriteLine("term SET: id was IGNORED and reassigned.");
                }
                catch (AssertFailedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    AssertRejectedForTheRightReason("term SET", ex);
                }
                finally
                {
                    if (group != null)
                    {
                        try
                        {
                            await DeleteTermGroupDeepAsync(group).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("S1")]
        public async Task Probe_ConfirmsTheModelSurfaceCannotExpressASuppliedId()
        {
            Assert.IsFalse(typeof(ITermGroup).GetProperty(nameof(ITermGroup.Id)).CanWrite,
                "ITermGroup.Id became settable - re-open S1.");
            Assert.IsFalse(typeof(ITermSet).GetProperty(nameof(ITermSet.Id)).CanWrite,
                "ITermSet.Id became settable - re-open S1.");
            Assert.IsFalse(typeof(ITerm).GetProperty(nameof(ITerm.Id)).CanWrite,
                "ITerm.Id became settable - re-open S1.");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Confirms Graph refused the supplied id for a reason that actually settles the question.
        /// </summary>
        private static void AssertRejectedForTheRightReason(string what, Exception ex)
        {
            var serviceException = ex as ServiceException;
            var error = serviceException?.Error as ServiceError;

            int httpCode = error?.HttpResponseCode ?? 0;
            string graphCode = error?.Code;
            string detail = error?.Message ?? ex.Message;

            Console.WriteLine($"{what}: rejected. HTTP {httpCode}, code '{graphCode}'{Environment.NewLine}  {detail}");

            Assert.AreNotEqual(401, httpCode,
                $"The probe was not authenticated, so it proved nothing about {what} ids.");

            Assert.AreNotEqual(403, httpCode,
                $"The probe was forbidden - most likely no term store admin rights - so it proved nothing about {what} ids.");

            Assert.IsFalse(detail != null && detail.Contains("Resource not found for the segment", StringComparison.OrdinalIgnoreCase),
                $"The URL was wrong, so this proved nothing about {what} ids. Detail: {detail}");

            Assert.IsTrue(httpCode >= 400 && httpCode < 500,
                $"Expected a 4xx refusal of the payload, got HTTP {httpCode}.");

            Console.WriteLine($"{what}: Graph REFUSED the supplied id - confirms D7.");
        }

        private static async Task DeleteGroupIfPresentAsync(PnPContext context, string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return;
            }

            try
            {
                ITermGroup group = await context.TermStore.Groups.GetByIdAsync(groupId).ConfigureAwait(false);
                if (group != null)
                {
                    await DeleteTermGroupDeepAsync(group).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
