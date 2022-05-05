using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamTagTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTeamTagsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Tags);
                Assert.IsNotNull(team.Tags);
            }
        }

        [TestMethod]
        public async Task AddTeamTagTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Tags, x => x.Members);

                var userId = team.Members.AsRequested().First().Id;

                var teamTagOptions = new TeamTagOptions
                {
                    DisplayName = "PnP Tag",
                    Members = new List<TeamTagUserOptions>
                    {
                        new TeamTagUserOptions
                        { 
                            UserId = userId
                        }
                    }
                };

                var addedTag = await team.Tags.AddAsync(teamTagOptions);

                Assert.IsNotNull(addedTag);
                Assert.AreEqual(addedTag.DisplayName, "PnP Tag");
                Assert.AreEqual(addedTag.MemberCount, 1);
                await addedTag.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddTeamTagMultipleMembersTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Tags, x => x.Members);

                var firstUserId = team.Members.AsRequested().First().Id;
                var secondUserId = team.Members.AsRequested().Last().Id;

                var teamTagOptions = new TeamTagOptions
                {
                    DisplayName = "PnP Tag",
                    Members = new List<TeamTagUserOptions>
                    {
                        new TeamTagUserOptions
                        {
                            UserId = firstUserId
                        },
                        new TeamTagUserOptions
                        {
                            UserId = secondUserId
                        }
                    }
                };

                var addedTag = await team.Tags.AddAsync(teamTagOptions);

                Assert.IsNotNull(addedTag);
                Assert.AreEqual(addedTag.DisplayName, "PnP Tag");
                Assert.AreEqual(addedTag.MemberCount, 2);
                await addedTag.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateTeamTagAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Tags, x => x.Members);

                var userId = team.Members.AsRequested().First().Id;

                var teamTagOptions = new TeamTagOptions
                {
                    DisplayName = "PnP Tag",
                    Members = new List<TeamTagUserOptions>
                    {
                        new TeamTagUserOptions
                        {
                            UserId = userId
                        }
                    }
                };

                var addedTag = await team.Tags.AddAsync(teamTagOptions);
                Assert.AreEqual(addedTag.DisplayName, "PnP Tag");

                addedTag.DisplayName = "PnP Tag - Updated";
                await addedTag.UpdateAsync();

                await context.Team.LoadAsync(y => y.Tags);
                var updatedTag = context.Team.Tags.FirstOrDefault(y => y.Id == addedTag.Id);
                Assert.IsNotNull(updatedTag);
                Assert.AreEqual(updatedTag.DisplayName, "PnP Tag - Updated");

                await addedTag.DeleteAsync();
            }
        }
    }
}
