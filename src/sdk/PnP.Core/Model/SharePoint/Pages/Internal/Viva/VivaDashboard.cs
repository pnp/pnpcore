using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal implementation of IVivaDashboard
    /// </summary>
    internal class VivaDashboard : IVivaDashboard
    {
        /// <summary>
        /// IPage object backing the Dashboard
        /// </summary>
        internal IPage DashboardPage { get; private set; }

        /// <summary>
        /// Canvas section to which ACEs are added
        /// </summary>
        internal ICanvasSection Section { get; private set; }

        /// <summary>
        /// Lists of abstract representation of ACE in the Section
        /// Used as a backing field for ACE list
        /// </summary>
        internal IEnumerable<IPageWebPart> Controls
        {
            get
            {
                return Section.Controls.OfType<IPageWebPart>();
            }
        }

        private IEnumerable<IPageComponent> pageComponents;

        /// <summary>
        /// Page components available in the page.
        /// Is used by LoadACEManifest
        /// </summary>
        public IEnumerable<IPageComponent> PageComponents
        {
            get
            {
                if (pageComponents == null)
                {
                    pageComponents = GetACEComponents();
                }
                return pageComponents;
            }
        }

        /// <summary>
        /// List of ACEFactories. Used to map IPageWebPart to Adaptive Card Extension
        /// Can be extended by RegisterCustomACEFactory
        /// </summary>
        protected List<ACEFactory> RegisteredACEFactory { get; set; } = new List<ACEFactory>();

        /// <summary>
        /// Default ACEFactory. Used then there is no specific ACEFactory registered for ACE
        /// </summary>
        protected ACEFactory DefaultACEFactory { get; set; } = new ACEFactory();

        /// <summary>
        /// List of Adaptive Card Extensions provisioned to the Dashboard
        /// </summary>
        public List<AdaptiveCardExtension> ACEs
        {
            get
            {
                return Controls.Select(control =>
                {
                    AdaptiveCardExtension ace = null; 
                    ACEFactory factory = RegisteredACEFactory.FirstOrDefault(factory => factory.ACEId == control.WebPartId);
                    if (factory != null)
                    {
                        ace = factory.BuildACEFromWebPart(control);
                    }
                    else
                    {
                        //Default "generic" ACE model
                        ace = DefaultACEFactory.BuildACEFromWebPart(control);
                    }
                    return ace;
                }).ToList();
                
            }
        }

        public VivaDashboard(IPage dashboardPage)
        {            
            DashboardPage = dashboardPage;
            Section = DashboardPage.Sections.OfType<ICanvasSection>().FirstOrDefault();
            
            // Register the OOB cards
            RegisteredACEFactory.Add(new TeamsACEFactory());
            RegisteredACEFactory.Add(new CardDesignerACEFactory());
            RegisteredACEFactory.Add(new AssignedTasksACEFactory());
        }



        public AdaptiveCardExtension NewACE(DefaultACE defaultACE, CardSize cardSize = CardSize.Medium)
        {
            return NewACE(Guid.Parse(DefaultACEToId(defaultACE)), cardSize);
        }

        public AdaptiveCardExtension NewACE(Guid aceId, CardSize cardSize = CardSize.Medium)
        {
            var pageComponentManifest = LoadACEManifest<JsonElement>(aceId);

            if (pageComponentManifest == null)
            {
                var genericACE = new AdaptiveCardExtension
                {
                    Id = aceId.ToString(),
                    CardSize = cardSize
                };

                return genericACE;
            }
            else
            {
                var genericACE = new AdaptiveCardExtension
                {
                    Id = pageComponentManifest.Id,
                    CardSize = cardSize
                };

                if (pageComponentManifest.PreconfiguredEntries != null && pageComponentManifest.PreconfiguredEntries.Count > 0)
                {
                    var first = pageComponentManifest.PreconfiguredEntries.First();
                    genericACE.IconProperty = first.IconImageUrl;

                    if (!first.Properties.Equals(default(JsonElement)))
                    {
                        genericACE.Properties = first.Properties;
                    }
                }

                return genericACE;
            }
        }

        public void AddACE(AdaptiveCardExtension ace)
        {
            AddACE(ace, 0);
        }

        public void AddACE(AdaptiveCardExtension ace, int order)
        {
            if (ace == null)
            {
                throw new ArgumentNullException(nameof(ace));
            }

            // Sync ace properties to ace propery data 
            SyncAcePropertiesFromAceConfiguration(ace);

            DashboardPage.AddControl(new PageWebPart()
            {
                WebPartId = ace.Id,
                Description = ace.Description,
                InstanceId = ace.InstanceId,
                Title = ace.Title,
                Order = order,
                PropertiesJson = JsonSerializer.Serialize(new
                {
                    properties = ace.Properties
                }),
                section = Section,
                ACEIconProperty = ace.IconProperty,
                ACECardSize = ace.CardSize.ToString(),
            });
        }

        private static void SyncAcePropertiesFromAceConfiguration(AdaptiveCardExtension ace)
        {            
            if (ace.Properties == null)
            {
                ace.Properties = new ACEProperties();
            }

            if (ace.Properties is ACEProperties acePropertyData)
            {
                acePropertyData.AceData.CardSize = ace.CardSize.ToString();

                if (!string.IsNullOrEmpty(ace.Title))
                {
                    acePropertyData.Title = ace.Title;
                }

                if (!string.IsNullOrEmpty(ace.Description))
                {
                    acePropertyData.Description = ace.Description;
                }

                if (!string.IsNullOrEmpty(ace.IconProperty))
                {
                    acePropertyData.IconProperty = ace.IconProperty;
                }
            }
        }

        public void UpdateACE(AdaptiveCardExtension ace)
        {
            UpdateACE(ace, ace.Order);
        }

        public void UpdateACE(AdaptiveCardExtension ace, int order)
        {
            // Sync possibly updated properties back to propertiesJSON
            var control = DashboardPage.Controls.Where(p => p.InstanceId == ace.InstanceId).FirstOrDefault();
            if (control != null && control is PageWebPart aceWebPart)
            {
                aceWebPart.ACECardSize = ace.CardSize.ToString();
                aceWebPart.ACEIconProperty = ace.IconProperty;
                aceWebPart.Title = ace.Title;
                aceWebPart.Description = ace.Description;
                aceWebPart.Order = order;

                // Sync ace properties to ace propery data 
                SyncAcePropertiesFromAceConfiguration(ace);

                aceWebPart.PropertiesJson = JsonSerializer.Serialize(new
                {
                    properties = ace.Properties
                });
            }
        }

        internal PageComponentManifest<T> LoadACEManifest<T>(Guid aceId)
        {
            //Make sure ComponentType is ACE
            IPageComponent component = PageComponents.FirstOrDefault(x => x.Id == aceId.ToString());
            if (component != null)
            {
                return JsonSerializer.Deserialize<PageComponentManifest<T>>(component.Manifest);
            }
            return null;
        }

        public void Save()
        {
            SaveAsync().GetAwaiter().GetResult();
        }

        public Task SaveAsync()
        {
            return DashboardPage.SaveAsync();
        }

        public void RemoveACE(Guid instanceId)
        {
            IPageWebPart wp = Controls.FirstOrDefault(control => control.InstanceId == instanceId);
            if (wp != null)
            {
                wp.Delete();
            }
        }

        public void RegisterCustomACEFactory(ACEFactory factory)
        {
            RegisteredACEFactory.Add(factory);
        }

        protected virtual List<PageComponent> GetACEComponents()
        {
            //Noticed this is a common pattern across the solution
            //Maybe it's worth to expose RawRequestAsync method at interface level
            //Or at PnPContext level?
            Web web = DashboardPage.PnPContext.Web as Web;
            var apiCall = new ApiCall($"_api/web/GetClientSideComponentsByComponentType(componentTypesString='7',supportedHostTypeValue=3)", ApiType.SPORest);

            var response = web.RawRequestAsync(apiCall, HttpMethod.Post).GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(response.Json))
            {
                var root = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("value");

                var clientSideComponents = JsonSerializer.Deserialize<List<PageComponent>>(root.ToString(), PnPConstants.JsonSerializer_IgnoreNullValues);

                if (!clientSideComponents.Any())
                {
                    throw new ClientException(PnPCoreResources.Exception_Page_NoClientSideComponentsRetrieved);
                }

                return clientSideComponents.Where(component => component.ComponentType == 7).ToList();
            }
            else
            {
                throw new ClientException(PnPCoreResources.Exception_Page_NoClientSideComponentsRetrieved);
            }
        }

        public static DefaultACE IdToDefaultACE(string id)
        {
            return id.ToLower() switch
            {
                "9593e615-7320-4b8b-be98-09b97112b12f" => DefaultACE.CardDesigner,
                "749d8ca7-0821-4e96-be16-db7b0bcf1a9e" => DefaultACE.AssignedTasks,
                "3f2506d3-390c-426e-b272-4b4ec0ee4d2d" => DefaultACE.TeamsApp,
                "d5599777-42ec-4755-bcd6-1b3f0f01349e" => DefaultACE.Approvals,
                "c9925e96-32e6-4acb-a012-20e913d03410" => DefaultACE.Shifts,
                "20222d75-541f-4400-8100-1dec7b274cbb" => DefaultACE.WebLink,
                _ => DefaultACE.ThirdParty,
            };
        }

        public static string DefaultACEToId(DefaultACE defaultACE)
        {
            return defaultACE switch
            {
                DefaultACE.CardDesigner => "9593e615-7320-4b8b-be98-09b97112b12f",
                DefaultACE.AssignedTasks => "749d8ca7-0821-4e96-be16-db7b0bcf1a9e",
                DefaultACE.TeamsApp => "3f2506d3-390c-426e-b272-4b4ec0ee4d2d",
                DefaultACE.Approvals => "d5599777-42ec-4755-bcd6-1b3f0f01349e",
                DefaultACE.Shifts => "c9925e96-32e6-4acb-a012-20e913d03410",
                DefaultACE.WebLink => "20222d75-541f-4400-8100-1dec7b274cbb",
                _ => "",
            };
        }

    }
}
