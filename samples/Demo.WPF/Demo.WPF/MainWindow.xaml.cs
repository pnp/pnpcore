using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace Demo.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IPnPContextFactory pnpContextFactory;

        public MainWindow(IPnPContextFactory pnpFactory)
        {
            this.pnpContextFactory = pnpFactory;

            InitializeComponent();

            var environmentName = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            if (!string.IsNullOrEmpty(environmentName))
            {
                this.Title = this.Title + " (" + environmentName + ")";
            }                
        }

        internal async Task SiteInfoAsync()
        {
            using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving web with lists and masterpageurl loaded ==> SharePoint REST query
                var web = await context.Web.GetAsync(w => w.Title, w => w.Description, w => w.MasterUrl);
                this.txtResults.Text = $"SiteInfo (async) - Title: {web.Title} Description: {web.Description} MasterPageUrl: {web.MasterUrl}";
            }
        }

        void SiteInfoSync()
        {
            using (var context = pnpContextFactory.Create("DemoSite"))
            {
                // Retrieving web with lists and masterpageurl loaded ==> SharePoint REST query
                var web = context.Web.Get(w => w.Title, w => w.Description, w => w.MasterUrl);
                this.txtResults.Text = $"SiteInfo (sync) - Title: {web.Title} Description: {web.Description} MasterPageUrl: {web.MasterUrl}";
            }
        }

        async Task AddPageAsync()
        {
            using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
            {
                var newPage = await context.Web.NewPageAsync();
                
                newPage.PageTitle = $"NewPageAsync {DateTime.Now:yyyyMMddHHmmss}";

                var newPageName = $"NewPageAsync{DateTime.Now:yyyyMMddHHmmss}";
                await newPage.SaveAsync(newPageName);

                this.txtResults.Text = $"Page added (async) - PageTitle: {newPage.PageTitle} Name: {newPage.Name}";
            }
        }

        void AddPageSync()
        {
            using (var context = pnpContextFactory.Create("DemoSite"))
            {
                var newPage = context.Web.NewPage();                
                newPage.PageTitle = $"NewPage {DateTime.Now:yyyyMMddHHmmss}";

                var newPageName = $"NewPage{DateTime.Now:yyyyMMddHHmmss}";
                newPage.Save(newPageName);

                this.txtResults.Text = $"Page added (sync) - PageTitle: {newPage.PageTitle} Name: {newPage.Name}";
            }
        }

        

        internal async Task ListInfoAsync()
        {
            using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving lists of the target web using Microsoft Graph
                var web = await context.Web.GetAsync(w => w.Title);

                var lists = (from l in context.Web.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Description)
                             orderby l.Title descending
                             select l);

                var sb = new StringBuilder();
                // Need to use Async here to avoid getting deadlocked
                foreach(var list in await lists.ToListAsync())
                {
                    sb.AppendLine($"Id: {list.Id} Title: {list.Title} Description: {list.Description}");
                }
                this.txtResults.Text = sb.ToString();
            }
        }

        internal async Task TeamInfoAsync()
        {
            using (var context = await pnpContextFactory.CreateAsync("DemoSite"))
            {
                // Retrieving lists of the target web
                var team = await context.Team.GetAsync(t => t.DisplayName, t => t.Description, t => t.Channels);

                var sb = new StringBuilder();
                sb.AppendLine($"Name: {team.DisplayName} Description: {team.Description}");
                sb.AppendLine();

                foreach (var channel in team.Channels)
                {
                    sb.AppendLine($"Id: {channel.Id} Name: {channel.DisplayName}");
                }
                this.txtResults.Text = sb.ToString();
            }
        }

        private async void btnSiteInfoAsync_Click(object sender, RoutedEventArgs e)
        {
            await SiteInfoAsync();
        }

        private void btnSiteInfoSync_Click(object sender, RoutedEventArgs e)
        {
            SiteInfoSync();
        }

        private async void btnAddPageAsync_Click(object sender, RoutedEventArgs e)
        {
          await AddPageAsync();
        }

        private void btnAddPageSync_Click(object sender, RoutedEventArgs e)
        {
          AddPageSync();
        }

        private async void btnList_Click(object sender, RoutedEventArgs e)
        {
            await ListInfoAsync();
        }

        private async void btnTeam_Click(object sender, RoutedEventArgs e)
        {
            await TeamInfoAsync();
        }
    }
}
