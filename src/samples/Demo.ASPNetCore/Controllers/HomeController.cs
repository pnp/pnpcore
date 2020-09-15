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

namespace Demo.ASPNetCore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPnPContextFactory _pnpContextFactory;

        public HomeController(IPnPContextFactory pnpContextFactory, ILogger<HomeController> logger)
        {
            _pnpContextFactory = pnpContextFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SiteInfo()
        {
            var model = new SiteInfoViewModel();

            using (var context = await _pnpContextFactory.CreateAsync("DemoSite"))
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

            using (var context = await _pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving lists of the target web using Microsoft Graph
                var web = await context.Web.GetAsync(w => w.Title);

                var lists = (from l in context.Web.Lists
                             orderby l.Title descending
                             select l)
                            .Load(l => l.Id, l => l.Title, l => l.Description);

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

            using (var context = await _pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving lists of the target web
                var items = (from i in context.Web.Lists.GetByTitle(listTitle).Items
                             select i).Load(i => i.Id, i => i.Title);

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

            using (var context = await _pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving lists of the target web
                var team = await context.Team.GetAsync(t => t.DisplayName,
                    t => t.Description,
                    t => t.Channels.Include(c => c.Id, c => c.DisplayName));

                model.DisplayName = team.DisplayName;
                model.Description = team.Description;
                model.Channels = new List<ChannelViewModel>(from c in team.Channels
                                                            select new ChannelViewModel
                                                            {
                                                                DisplayName = c.DisplayName,
                                                                Id = c.Id
                                                            });
            }

            return View("TeamInfo", model);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
