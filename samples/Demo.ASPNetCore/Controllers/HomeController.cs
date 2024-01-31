using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Demo.ASPNetCore.Models;
using PnP.Core.Services;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using Microsoft.Identity.Web;
using PnP.Core.Auth;
using Microsoft.Extensions.Options;
using PnP.Core.Services.Builder.Configuration;

namespace Demo.ASPNetCore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPnPContextFactory _pnpContextFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly PnPCoreOptions _pnpCoreOptions;

        public HomeController(IPnPContextFactory pnpContextFactory, 
            ILogger<HomeController> logger,
            ITokenAcquisition tokenAcquisition,
            IOptions<PnPCoreOptions> pnpCoreOptions)
        {
            _pnpContextFactory = pnpContextFactory;
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
            _pnpCoreOptions = pnpCoreOptions?.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] { "https://officedevpnp.sharepoint.com/.default" })]
        public async Task<IActionResult> SiteInfo()
        {
            var model = new SiteInfoViewModel();

            using (var context = await createSiteContext())
            {
                // Retrieving web with lists and masterpageurl loaded ==> SharePoint REST query
                var web = await context.Web.GetAsync(w => w.Title, w => w.Description, w => w.MasterUrl);

                model.Title = web.Title;
                model.Description = web.Description;
                model.MasterUrl = web.MasterUrl;
            }

            return View("SiteInfo", model);
        }

        public async Task<IActionResult> ListInfo()
        {
            var model = new ListInfoViewModel();

            using (var context = await createSiteContext())
            {
                // Retrieving lists of the target web using Microsoft Graph
                var web = await context.Web.GetAsync(w => w.Title, 
                    w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Description));

                var lists = (from l in context.Web.Lists
                             orderby l.Title descending
                             select l);

                // Retrieving lists of the target web using SPO REST
                // var web = await context.Web.GetAsync(w => w.Title, w => w.Lists.Include(l => l.Title, l => l.Id));
                // var lists = web.Lists;

                model.SiteTitle = web.Title;
                model.Lists = lists.ToList();
            }

            return View("ListInfo", model);
        }

        public async Task<IActionResult> ListItems(string listTitle)
        {
            var model = new ListItemsViewModel();

            using (var context = await createSiteContext())
            {
                // Retrieving lists of the target web
                var items = (from i in context.Web.Lists.GetByTitle(listTitle).Items.QueryProperties(i => i.Id, i => i.Title)
                             select i);

                model.ListTitle = listTitle;
                model.Items = new List<ListItemViewModel>(from i in items.ToList()
                                        select
                                          new ListItemViewModel
                                          {
                                              Id = i.Id,
                                              Title = i.Title
                                          });
            }

            return View("ListItems", model);
        }

        public async Task<IActionResult> TeamInfo()
        {
            var model = new TeamInfoViewModel();

            using (var context = await createSiteContext())
            {
                // Retrieving lists of the target web
                var team = await context.Team.GetAsync(t => t.DisplayName, t => t.Description, 
                    t => t.Channels.QueryProperties(p => p.Id, p => p.DisplayName));

                model.DisplayName = team.DisplayName;
                model.Description = team.Description;
                model.Channels = new List<ChannelViewModel>(from c in team.Channels.ToList()
                                                            select new ChannelViewModel
                                                            {
                                                                DisplayName = c.DisplayName,
                                                                Id = c.Id
                                                            });
            }

            return View("TeamInfo", model);
        }

        private async Task<PnPContext> createSiteContext()
        {
            var siteUrl = new Uri(_pnpCoreOptions.Sites["DemoSite"].SiteUrl);

            return await _pnpContextFactory.CreateAsync(siteUrl,
                            new ExternalAuthenticationProvider((resourceUri, scopes) =>
                            {
                                return _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                            }
                            ));
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
