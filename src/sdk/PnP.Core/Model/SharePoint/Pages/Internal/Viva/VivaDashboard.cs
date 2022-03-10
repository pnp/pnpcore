using PnP.Core.Model.SharePoint.Pages.Public;
using PnP.Core.Model.SharePoint.Pages.Public.Viva;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory;
using PnP.Core.Model.SharePoint.Viva;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint.Pages.Internal.Viva
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
        internal List<IPageWebPart> Controls
        {
            get
            {
                return Section.Controls.OfType<IPageWebPart>().ToList();
            }
        }
        private IEnumerable<IPageComponent> _pageComponents;
        /// <summary>
        /// Page components available in the page.
        /// Is used by LoadACEManifest
        /// </summary>
        public IEnumerable<IPageComponent> PageComponents
        {
            get
            {
                if (_pageComponents == null)
                {
                    _pageComponents = GetACEComponents();
                }
                return _pageComponents;
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
        /// List of Adaptive Card Extensions proviosned to the Dashboard
        /// </summary>
        public List<AdaptiveCardExtension> ACEs
        {
            get
            {
                return Controls.Select(control =>
                {
                    ACEFactory factory = RegisteredACEFactory.FirstOrDefault(factory => factory.ACEId == control.WebPartId);
                    if (factory != null)
                    {
                        return factory.BuildACEFromWebPart(control);
                    }
                    //Default "generic" ACE model
                    return DefaultACEFactory.BuildACEFromWebPart(control);
                }
                ).ToList();
            }
        }
        public VivaDashboard(IPage dashboardPage)
        {
            this.DashboardPage = dashboardPage;
            Section = DashboardPage.Sections.OfType<ICanvasSection>().FirstOrDefault();
            RegisteredACEFactory.Add(new TeamsACEFactory());
            RegisteredACEFactory.Add(new CardDesignerACEFactory());
            RegisteredACEFactory.Add(new AssignedTasksACEFactory());
        }

        public void AddACE(AdaptiveCardExtension ace)
        {
            DashboardPage.AddControl((new PageWebPart()
            {
                WebPartId = ace.Id,
                Description = ace.Description,
                InstanceId = ace.InstanceId,
                Title = ace.Title,
                PropertiesJson = JsonSerializer.Serialize(new
                {
                    properties = ace.Properties
                }),
                section = Section
            }));
        }
        public PageComponentManifest<T> LoadACEManifest<T>(Guid aceId)
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
            DashboardPage.Save();
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
    }
}
