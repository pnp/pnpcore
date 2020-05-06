using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Test.Utilities;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class GraphV1BetaTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetV1vsBetaPropertyViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Default config must be to use v1.0 endpoint
                Assert.IsFalse(context.GraphAlwaysUseBeta);

                // Get the primary channel without expression...should default to v1.0 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // Are the expected properties populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Id));

                // Are other properties still not available: membershiptype is a beta property on a teamchannel, so it should not be available at this point
                Assert.IsFalse(team.PrimaryChannel.IsPropertyAvailable(p => p.MembershipType));

                // get the primary channel again, but now explicitely request the beta properties MembershipType and IsFavoriteByDefault
                await team.PrimaryChannel.GetAsync(p => p.MembershipType, p => p.DisplayName, p => p.IsFavoriteByDefault);
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.MembershipType));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.DisplayName));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.IsFavoriteByDefault));
            }
        }

        [TestMethod]
        public async Task GetV1vsBetaPropertyViaGraphForceBeta()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Force beta
                context.GraphAlwaysUseBeta = true;

                // Get the primary channel without expression...should default to v1 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // All the beta properties should already be loaded
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.MembershipType));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.DisplayName));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.IsFavoriteByDefault));
            }
        }

        [TestMethod]
        public async Task GetV1vsBetaPropertyViaGraphNoBeta()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Prevent beta
                context.GraphCanUseBeta = false;

                // Get the primary channel without expression...should default to v1.0 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // Are the expected properties populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Id));

                // Are other properties still not available: membershiptype is a beta property on a teamchannel, so it should not be available at this point
                Assert.IsFalse(team.PrimaryChannel.IsPropertyAvailable(p => p.MembershipType));

                // get the primary channel again, but now explicitely request the beta properties MembershipType and IsFavoriteByDefault
                await team.PrimaryChannel.GetAsync(p => p.MembershipType, p => p.DisplayName, p => p.IsFavoriteByDefault);
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                
                // Beta props should still be unavailable as we're not allowed to use the beta endpoint
                Assert.IsFalse(team.PrimaryChannel.IsPropertyAvailable(p => p.MembershipType));
                Assert.IsFalse(team.PrimaryChannel.IsPropertyAvailable(p => p.IsFavoriteByDefault));
                // v1 props should be loaded
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.DisplayName));
            }
        }

        [TestMethod]
        public async Task GetV1vsBetaEntityViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Default config must be to use v1.0 endpoint
                Assert.IsFalse(context.GraphAlwaysUseBeta);

                // Get the primary channel without expression...should default to v1.0 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // Are the expected properties populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Id));

                // Get the beta property messages
                await team.PrimaryChannel.GetAsync(p => p.Messages);
                
                // messages collection should be available and loaded
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Messages));
                Assert.IsTrue(team.PrimaryChannel.Messages.Requested);

                // Add message, this should work if we allow beta to be used
                var addedMessage = await team.PrimaryChannel.Messages.AddAsync("PnP Rocks!");

                // Verify to see if the id property is available on the created message, this is a sign that the add went well
                Assert.IsTrue(addedMessage.IsPropertyAvailable(p => p.Id));

            }
        }

        [TestMethod]
        public async Task GetV1vsBetaEntityViaGraphNoBeta()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Prevent beta
                context.GraphCanUseBeta = false;

                // Get the primary channel without expression...should default to v1.0 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // Are the expected properties populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Id));

                // Try to get the beta property messages, this is a collection
                await team.PrimaryChannel.GetAsync(p => p.Messages);
                
                // collection should not be available
                Assert.IsFalse(team.PrimaryChannel.IsPropertyAvailable(p => p.Messages));
                Assert.IsFalse(team.PrimaryChannel.Messages.Requested);

                bool exceptionThrown = false;
                try
                {
                    // Add message, this shouldn't work if we don't allow beta to be used
                    var addedMessage = await team.PrimaryChannel.Messages.AddAsync("PnP Rocks!");
                }
                catch(Exception)
                {
                    exceptionThrown = true;
                }

                // Verify to see if the id property is available on the created message, this is a sign that the add went well
                Assert.IsTrue(exceptionThrown);

            }
        }
    }
}
