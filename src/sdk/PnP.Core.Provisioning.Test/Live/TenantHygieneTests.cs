using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live
{
    /// <summary>
    /// Checks the live suite has not left anything behind in the tenant.
    /// </summary>
    [TestClass]
    public class TenantHygieneTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Hygiene")]
        public async Task NoTestTermGroupsAreLeftBehind()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name)).ConfigureAwait(false);

                List<ITermGroup> leaked = context.TermStore.Groups.AsRequested()
                    .Where(g => g.Name != null && g.Name.StartsWith(TestPrefix, StringComparison.Ordinal))
                    .ToList();

                if (leaked.Count == 0)
                {
                    Console.WriteLine("Term store is clean - no test groups left behind.");
                    return;
                }

                Console.WriteLine($"Found {leaked.Count} leaked test term group(s); removing:");

                var failures = new List<string>();
                foreach (ITermGroup group in leaked)
                {
                    Console.WriteLine($"  {group.Name} ({group.Id})");
                    try
                    {
                        await DeleteTermGroupDeepAsync(group).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"{group.Name} ({group.Id}): {ex.Message}");
                    }
                }

                Assert.AreEqual(0, failures.Count,
                    "These test term groups could not be removed automatically and need deleting by hand " +
                    $"in the SharePoint admin centre:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");

                Console.WriteLine($"Removed {leaked.Count} leaked test term group(s).");
            }
        }
    }
}
