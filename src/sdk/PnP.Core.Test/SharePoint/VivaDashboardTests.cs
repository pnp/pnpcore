using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class VivaDashboardTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task ManageVivaDashboardTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                // Load the current dashboard
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                Assert.IsNotNull(dashboard);

                // Add a new typed OOB ACE
                var tasks = new AssignedTasksACE(CardSize.Large);

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", tasks.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    tasks.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                dashboard.AddACE(tasks);

                // Save the dashboard
                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == tasks.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(tasks.InstanceId);

                // Save the dashboard
                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was removed
                Assert.IsNull(dashboard.ACEs.FirstOrDefault(p=>p.InstanceId == tasks.InstanceId));
            }
        }

        [TestMethod]
        public async Task AddCardDesignerACE()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                var cardDesignerACE = new CardDesignerACE(CardSize.Large)
                {
                    Title = "Test Card Designer ACE",
                    Description = "Test description",
                    Properties = new CardDesignerProps
                    {
                        DataType = "Static",
                        TemplateType = "primaryText",
                        CardIconSourceType = 1,
                        CardImageSourceType = 1,
                        CardSelectionAction = new ExternalLinkAction
                        {
                            Parameters = new ExternalLinkActionParameter
                            {
                                Target = "https://bing.com"
                            }
                        },
                        NumberCardButtonActions = 2,
                        CardButtonActions = new List<ButtonAction>
                        {
                            new ButtonAction
                            {
                                Title = "Test 1",
                                Style = "positive",
                                Action = new ExternalLinkAction
                                {
                                    Parameters = new ExternalLinkActionParameter
                                    {
                                        Target = "https://google.com"
                                    }
                                }
                            },
                            new ButtonAction
                            {
                                Title = "Test 2",
                                Style = "default",
                                Action = new QuickViewAction
                                {
                                    Parameters = new QuickViewActionParameter
                                    {
                                        View = "quickView"
                                    }
                                }
                            }
                        },
                        QuickViews = new List<QuickView>
                        {
                            new QuickView
                            {
                                Data = "{\n  \"Url\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"Text\": \"Hello, World!\"\n}",
                                Template = "{\n  \"type\": \"AdaptiveCard\",\n  \"body\": [\n    {\n      \"type\": \"TextBlock\",\n      \"size\": \"Medium\",\n      \"weight\": \"Bolder\",\n      \"text\": \"${Text}\",\n      \"wrap\": true\n    }\n  ],\n  \"actions\": [\n    {\n      \"type\": \"Action.OpenUrl\",\n      \"title\": \"View\",\n      \"url\": \"${Url}\"\n    }\n  ],\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"version\": \"1.2\"\n}",
                                Id = "quickView",
                                DisplayName = "Test Quick View"
                            }
                        },
                        QuickViewConfigured = false,
                        PrimaryText = "Test heading",
                        CustomImageSettings = new CustomImageSettings
                        {
                            Type = 1,
                            AltText = "There should be an image here",
                            ImageUrl = "https://cdn.hubblecontent.osi.office.net/m365content/publish/06faab94-9fb9-43b2-a274-9f0f51fedc3c/982488044.jpg"
                        }
                    }
                };

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", cardDesignerACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    cardDesignerACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                dashboard.AddACE(cardDesignerACE);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == cardDesignerACE.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(cardDesignerACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }
        
        [TestMethod]
        public async Task AddCardDesignerACE_Graph()
        {
            //Requires killswitch ef22b66c-aa6d-4865-900a-4805773a3e4c
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                var cardDesignerACE = new CardDesignerACE(CardSize.Large)
                {
                    Title = "Test Graph",
                    Description = "Test description",
                    Properties = new CardDesignerProps()
                    {
                        DataType = "MSGraph",
                        RequestUrl = "https://graph.microsoft.com/v1.0/me",
                        TemplateType = "primaryText",
                        CardIconSourceType = 1,
                        CardImageSourceType = 1,
                        CardSelectionAction = new ExternalLinkAction
                        {
                            Parameters = new ExternalLinkActionParameter
                            {
                                Target = "https://bing.com"
                            }
                        },
                        NumberCardButtonActions = 2,
                        CardButtonActions = new List<ButtonAction>
                        {
                            new ButtonAction()
                            {
                                Title = "Test 1",
                                Style = "positive",
                                Action = new ExternalLinkAction
                                {
                                    Parameters = new ExternalLinkActionParameter
                                    {
                                        Target = "https://google.com"
                                    }
                                }
                            },
                            new ButtonAction
                            {
                                Title = "Test 2",
                                Style = "default",
                                Action = new QuickViewAction
                                {
                                    Parameters = new QuickViewActionParameter
                                    {
                                        View = "quickView"
                                    }
                                }
                            }
                        },
                        QuickViews = new List<QuickView>
                        {
                            new QuickView
                            {
                                Data = "{\n  \"Url\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"Text\": \"Hello, World!\"\n}",
                                Template = "{\n  \"type\": \"AdaptiveCard\",\n  \"body\": [\n    {\n      \"type\": \"TextBlock\",\n      \"size\": \"Medium\",\n      \"weight\": \"Bolder\",\n      \"text\": \"${displayName}\",\n      \"wrap\": true\n    }\n  ],\n  \"actions\": [\n    {\n      \"type\": \"Action.OpenUrl\",\n      \"title\": \"View\",\n      \"url\": \"${email}\"\n    }\n  ],\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"version\": \"1.2\"\n}",
                                Id = "quickView",
                                DisplayName = "Test Quick View"
                            }
                        },
                        QuickViewConfigured = false,
                        PrimaryText = "Current User",
                        CustomImageSettings = new CustomImageSettings
                        {
                            Type = 1,
                            AltText = "There should be an image here",
                            ImageUrl = "https://cdn.hubblecontent.osi.office.net/m365content/publish/06faab94-9fb9-43b2-a274-9f0f51fedc3c/982488044.jpg"
                        }
                    }
                };

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", cardDesignerACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    cardDesignerACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                dashboard.AddACE(cardDesignerACE);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == cardDesignerACE.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(cardDesignerACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }
        
        [TestMethod]
        public async Task AddTeamsAppACE()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                var teamsACE = new TeamsACE()
                {
                    Title = "Stocks app",
                    Description = "Stocks app description",
                    IconProperty = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1",
                    Properties = new TeamsACEProperties
                    {                        
                        SelectedApp = new TeamsACEApp
                        {
                            AppId = "852a6067-4fec-4895-a3ab-a776c77be161",
                            Description = "Get real-time stock quotes",
                            Title = "Stocks",
                            DistributionMethod = "store",
                            IconProperties = new TeamsACEAppIconProperties
                            {
                                OutlineIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_smallimage.png?v=0.1",
                                ColorIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1"
                            },
                            IsBot = false
                        },
                    }
                };

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", teamsACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    teamsACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                // Add the teams ace as last in the dashboard
                dashboard.AddACE(teamsACE, 500);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == teamsACE.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(teamsACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }

        [TestMethod]
        public async Task AddAndUpdateACE()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                var teamsACE = new TeamsACE()
                {
                    Title = "Stocks app",
                    Description = "Stocks app description",
                    IconProperty = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1",
                    Properties = new TeamsACEProperties
                    {
                        SelectedApp = new TeamsACEApp
                        {
                            AppId = "852a6067-4fec-4895-a3ab-a776c77be161",
                            Description = "Get real-time stock quotes",
                            Title = "Stocks",
                            DistributionMethod = "store",
                            IconProperties = new TeamsACEAppIconProperties
                            {
                                OutlineIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_smallimage.png?v=0.1",
                                ColorIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1"
                            },
                            IsBot = false
                        },
                    }
                };

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", teamsACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    teamsACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                // Add the teams ace as last in the dashboard
                dashboard.AddACE(teamsACE, 500);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                var addedTeamsACE = dashboard.ACEs.FirstOrDefault(p => p.InstanceId == teamsACE.InstanceId);
                Assert.IsNotNull(addedTeamsACE);
                Assert.IsTrue(addedTeamsACE.Title == "Stocks app");
                Assert.IsTrue(addedTeamsACE.CardSize == CardSize.Medium);
                Assert.IsTrue(addedTeamsACE.Order >= 1);

                // Update the ACE
                addedTeamsACE.Title = "Updated stocks app";
                addedTeamsACE.CardSize = CardSize.Large;

                // Update the ACE, but also move it to be the first card on the dashboard
                dashboard.UpdateACE(addedTeamsACE, 0);

                // Save the dashboard
                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();
                addedTeamsACE = dashboard.ACEs.FirstOrDefault(p => p.InstanceId == teamsACE.InstanceId);

                Assert.IsTrue(addedTeamsACE.Title == "Updated stocks app");
                Assert.IsTrue(addedTeamsACE.CardSize == CardSize.Large);
                Assert.IsTrue(addedTeamsACE.Order == 1);

                // Remove the added ACE again
                dashboard.RemoveACE(teamsACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }

        [TestMethod]
        public async Task AddACEAsGeneric()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                var genericACE = new AdaptiveCardExtension();
                genericACE.Title = "PnP Rocks!";
                genericACE.Description = "Text+image card. Text on second line. We can ";
                genericACE.Id = VivaDashboard.DefaultACEToId(DefaultACE.CardDesigner);

                genericACE.Properties = JsonSerializer.Deserialize<JsonElement>("{\"templateType\":\"image\",\"cardIconSourceType\":2,\"cardImageSourceType\":1,\"cardSelectionAction\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}},\"numberCardButtonActions\":1,\"cardButtonActions\":[{\"title\":\"Button\",\"style\":\"positive\",\"action\":{\"type\":\"ExternalLink\",\"parameters\":{\"target\":\"https://www.bing.com/\"}}},{\"title\":\"Button\",\"style\":\"default\",\"action\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}}}],\"quickViews\":[{\"data\":\"\",\"template\":\"\",\"id\":\"quickView\",\"displayName\":\"Default Quick View\"}],\"isQuickViewConfigured\":true,\"currentQuickViewIndex\":0,\"dataType\":\"Static\",\"spRequestUrl\":\"\",\"requestUrl\":null,\"graphRequestUrl\":\"\",\"primaryText\":\"Text+image card. Text on second line. We can \",\"cardIconCustomImageSettings\":null,\"aceData\":{\"cardSize\":null},\"title\":\"PnP rocks!\",\"description\":\"Description text\",\"iconProperty\":\"https://image.flaticon.com/icons/png/512/747/747055.png\",\"cardImageCustomImageSettings\":{\"type\":1,\"altText\":\"Image thumbnail preview\",\"imageUrl\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"},\"imagePicker\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"}");

                Assert.IsTrue(genericACE.ACEType == DefaultACE.CardDesigner);

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", genericACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    genericACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                // Add the generic ace as last in the dashboard
                dashboard.AddACE(genericACE, 500);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == genericACE.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(genericACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }

        [TestMethod]
        public async Task AddACEAsGenericViaComponentLoad()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                // Instantiate default ACE
                var genericACE = dashboard.NewACE(DefaultACE.CardDesigner, CardSize.Large);
                // Override default properties of this ACE
                genericACE.Properties = JsonSerializer.Deserialize<JsonElement>("{\"templateType\":\"image\",\"cardIconSourceType\":2,\"cardImageSourceType\":1,\"cardSelectionAction\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}},\"numberCardButtonActions\":1,\"cardButtonActions\":[{\"title\":\"Button\",\"style\":\"positive\",\"action\":{\"type\":\"ExternalLink\",\"parameters\":{\"target\":\"https://www.bing.com/\"}}},{\"title\":\"Button\",\"style\":\"default\",\"action\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}}}],\"quickViews\":[{\"data\":\"\",\"template\":\"\",\"id\":\"quickView\",\"displayName\":\"Default Quick View\"}],\"isQuickViewConfigured\":true,\"currentQuickViewIndex\":0,\"dataType\":\"Static\",\"spRequestUrl\":\"\",\"requestUrl\":null,\"graphRequestUrl\":\"\",\"primaryText\":\"Text+image card. Text on second line. We can \",\"cardIconCustomImageSettings\":null,\"aceData\":{\"cardSize\":null},\"title\":\"PnP rocks!\",\"description\":\"Description text\",\"iconProperty\":\"https://image.flaticon.com/icons/png/512/747/747055.png\",\"cardImageCustomImageSettings\":{\"type\":1,\"altText\":\"Image thumbnail preview\",\"imageUrl\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"},\"imagePicker\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"}");

                Assert.IsTrue(genericACE.ACEType == DefaultACE.CardDesigner);

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", genericACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    genericACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                // Add the generic ace as last in the dashboard
                dashboard.AddACE(genericACE, 500);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Check if the ACE was added
                Assert.IsNotNull(dashboard.ACEs.FirstOrDefault(p => p.InstanceId == genericACE.InstanceId));

                // Remove the added ACE again
                dashboard.RemoveACE(genericACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }

        [TestMethod]
        public async Task GetCustomACE()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                // Add custom ACE
                var customACE = dashboard.NewACE(new Guid("9e73ef29-1b62-4084-92b5-207bedea22b8"));
                customACE.Title = "Custom Async ACE";
                customACE.Description = "something";

                if (!TestCommon.Instance.Mocking)
                {
                    TestManager.SaveProperties(context, new Dictionary<string, string>
                    {
                        { "InstanceId", customACE.InstanceId.ToString() }
                    });
                }
                else
                {
                    // Restore the previously used instance id
                    customACE.InstanceId = Guid.Parse(TestManager.GetProperties(context)["InstanceId"]);
                }

                dashboard.AddACE(customACE);

                dashboard.Save();

                // Load the dashboard again
                dashboard = context.Web.GetVivaDashboard();

                // Register custom ACE factory
                dashboard.RegisterCustomACEFactory(new CustomAsyncCardFactory());

                // We should not see the an ACE of our custom ACE type
                CustomAsyncCard customAce = dashboard.ACEs.OfType<CustomAsyncCard>().FirstOrDefault();
                Assert.IsNotNull(customAce);

                // Remove the added ACE again
                dashboard.RemoveACE(customACE.InstanceId);

                // Save the dashboard
                dashboard.Save();
            }
        }

    }
}
