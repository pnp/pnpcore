using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Pages.Public;
using PnP.Core.Model.SharePoint.Pages.Public.Viva;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions;
using PnP.Core.Model.SharePoint.Viva;
using PnP.Core.Test.SharePoint.Viva.MockData;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint.Viva
{
    [TestClass]
    public class VivaDashboardTests
    {
        [TestMethod]
        public async Task GetDashboard()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                TeamsACE ace = dashboard.ACEs.OfType<TeamsACE>().FirstOrDefault();
                Assert.IsNotNull(dashboard);

            }
        }
        [TestMethod]
        public async Task AddTasksACE()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                dashboard.AddACE(new AssignedTasksACE());
                dashboard.Save();
            }
        }
        [TestMethod]
        public async Task AddCardDesignerACE()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                dashboard.AddACE(new CardDesignerACE()
                {
                    Properties = new CardDesignerProps()
                    {
                        AceData = new ACEData()
                        {
                            CardSize = "Large"
                        },
                        DataType = "Static",
                        TemplateType = "primaryText",
                        CardIconSourceType = 1,
                        CardImageSourceType = 1,
                        CardSelectionAction = new ExternalLinkAction()
                        {
                            Parameters = new ExternalLinkActionParameter()
                            {
                                Target = "https://bing.com"
                            }
                        },
                        NumberCardButtonActions = 2,
                        CardButtonActions = new List<ButtonAction>()
                        {
                            new ButtonAction()
                            {
                                Title = "Test 1",
                                Style = "positive",
                                Action = new ExternalLinkAction()
                                {
                                    Parameters = new ExternalLinkActionParameter()
                                    {
                                        Target = "https://google.com"
                                    }
                                }
                            },
                            new ButtonAction()
                            {
                                Title = "Test 2",
                                Style = "default",
                                Action = new QuickViewAction()
                                {
                                    Parameters = new QuickViewActionParameter()
                                    {
                                        View = "quickView"
                                    }
                                }
                            }
                        },
                        QuickViews = new List<QuickView>()
                        {
                            new QuickView()
                            {
                                Data = "{\n  \"Url\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"Text\": \"Hello, World!\"\n}",
                                Template = "{\n  \"type\": \"AdaptiveCard\",\n  \"body\": [\n    {\n      \"type\": \"TextBlock\",\n      \"size\": \"Medium\",\n      \"weight\": \"Bolder\",\n      \"text\": \"${Text}\",\n      \"wrap\": true\n    }\n  ],\n  \"actions\": [\n    {\n      \"type\": \"Action.OpenUrl\",\n      \"title\": \"View\",\n      \"url\": \"${Url}\"\n    }\n  ],\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"version\": \"1.2\"\n}",
                                Id = "quickView",
                                DisplayName = "Test Quick View"
                            }
                        },
                        QuickViewConfigured = false,
                        Title = "Test Card Designer ACE",
                        PrimaryText = "Test heading",
                        Description = "Test description",
                        CustomImageSettings = new CustomImageSettings()
                        {
                            Type = 1,
                            AltText = "There should be an image here",
                            ImageUrl = "https://cdn.hubblecontent.osi.office.net/m365content/publish/06faab94-9fb9-43b2-a274-9f0f51fedc3c/982488044.jpg"
                        }
                    }
                });
                dashboard.Save();
            }
        }
        [TestMethod]
        public async Task AddCardDesignerACE_Graph()
        {
            //Requires killswitch ef22b66c-aa6d-4865-900a-4805773a3e4c
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                dashboard.AddACE(new CardDesignerACE()
                {
                    Properties = new CardDesignerProps()
                    {
                        AceData = new ACEData()
                        {
                            CardSize = "Large"
                        },
                        DataType = "MSGraph",
                        RequestUrl = "https://graph.microsoft.com/v1.0/me",
                        TemplateType = "primaryText",
                        CardIconSourceType = 1,
                        CardImageSourceType = 1,
                        CardSelectionAction = new ExternalLinkAction()
                        {
                            Parameters = new ExternalLinkActionParameter()
                            {
                                Target = "https://bing.com"
                            }
                        },
                        NumberCardButtonActions = 2,
                        CardButtonActions = new List<ButtonAction>()
                        {
                            new ButtonAction()
                            {
                                Title = "Test 1",
                                Style = "positive",
                                Action = new ExternalLinkAction()
                                {
                                    Parameters = new ExternalLinkActionParameter()
                                    {
                                        Target = "https://google.com"
                                    }
                                }
                            },
                            new ButtonAction()
                            {
                                Title = "Test 2",
                                Style = "default",
                                Action = new QuickViewAction()
                                {
                                    Parameters = new QuickViewActionParameter()
                                    {
                                        View = "quickView"
                                    }
                                }
                            }
                        },
                        QuickViews = new List<QuickView>()
                        {
                            new QuickView()
                            {
                                Data = "{\n  \"Url\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"Text\": \"Hello, World!\"\n}",
                                Template = "{\n  \"type\": \"AdaptiveCard\",\n  \"body\": [\n    {\n      \"type\": \"TextBlock\",\n      \"size\": \"Medium\",\n      \"weight\": \"Bolder\",\n      \"text\": \"${displayName}\",\n      \"wrap\": true\n    }\n  ],\n  \"actions\": [\n    {\n      \"type\": \"Action.OpenUrl\",\n      \"title\": \"View\",\n      \"url\": \"${email}\"\n    }\n  ],\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"version\": \"1.2\"\n}",
                                Id = "quickView",
                                DisplayName = "Test Quick View"
                            }
                        },
                        QuickViewConfigured = false,
                        Title = "Test Graph",
                        PrimaryText = "Current User",
                        Description = "Test description",
                        CustomImageSettings = new CustomImageSettings()
                        {
                            Type = 1,
                            AltText = "There should be an image here",
                            ImageUrl = "https://cdn.hubblecontent.osi.office.net/m365content/publish/06faab94-9fb9-43b2-a274-9f0f51fedc3c/982488044.jpg"
                        }
                    }
                });
                dashboard.Save();
            }
        }
        [TestMethod]
        public async Task AddTeamsAppACE()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                dashboard.AddACE(new TeamsACE()
                {
                    Properties = new TeamsACEProperties()
                    {
                        IconProperty = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1",
                        SelectedApp = new TeamsACEApp()
                        {
                            AppId = "852a6067-4fec-4895-a3ab-a776c77be161",
                            Description = "Get real-time stock quotes",
                            Title = "Stocks",
                            DistributionMethod = "store",
                            IconProperties = new TeamsACEAppIconProperties()
                            {
                                OutlineIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_smallimage.png?v=0.1",
                                ColorIconWebUrl = "https://statics.teams.cdn.office.net/evergreen-assets/apps/stocks_largeimage.png?v=0.1"
                            },
                            IsBot = false
                        },
                        Title = "Stocks app",
                        Description = "Stocks app description",
                    }
                });
                dashboard.Save();
            }
        }
        [TestMethod]
        public async Task RemoveACE()
        {
            //TestCommon.Instance.Mocking = true;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                dashboard.RemoveACE(Guid.Parse("5d07de1a-79b8-41d2-ac10-dd9f041b995e"));
                dashboard.Save();
            }
        }
        [TestMethod]
        public async Task GetCustomACE()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();
                dashboard.RegisterCustomACEFactory(new CustomAsyncCardFactory());
                CustomAsyncCard customAce = dashboard.ACEs.OfType<CustomAsyncCard>().FirstOrDefault();
                Assert.IsNotNull(customAce);

            }
        }
        [TestMethod]
        public async Task GetManifest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IVivaDashboard dashboard = context.Web.GetVivaDashboard();

                var manifest = dashboard.LoadACEManifest<object>(Guid.Parse("749d8ca7-0821-4e96-be16-db7b0bcf1a9e"));
                Assert.IsNotNull(manifest);
            }
        }
    }
}
