# Working with sites

In SharePoint there are site collections and inside a site collection you have at least one web (the root web), which on it's own can have other webs (child webs). Quite often when talking about a web the term site or sub site is used. When using the PnP Core SDK a site collection is referred to via the [ISite interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) while a web (root web, child web or sub web) is defined via the [IWeb interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html).

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with sites
}
```

## Getting a site

A `PnPContext` always has a reference to the [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) of the underlying site collection and the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) of the connected site, so getting the current Site is as a simple as using the Site property on the PnPContext. When a context is created the Site and Web properties are loaded with some elementary properties like Id and GroupId.

```csharp
var site = context.site;
var id = site.Id;
```

If you want to load additional site properties you can do that via using one of the Get methods:

```csharp
// Load the root folder 
await context.Web.LoadAsync(p => p.RootFolder);

// Load the hub site id
var site = await context.Site.GetAsync(p => p.HubSiteId);
Console.WriteLine(site.HubSiteId);
```

## Getting changes

You can use the `GetChanges` methods on an `ISite` to list all the changes. See [Enumerating changes that happened in SharePoint](changes-sharepoint.md) to learn more.

## Getting analytics

Using one of the `GetAnalytics` methods on `ISite` gives you back the site analytics for all time, the last seven days or for a custom interval of your choice. The returned `List<IActivityStat>` contains one row for the all time and seven days statistic requests, if you've requested a custom interval you also choose an aggregation interval (day, week, month) and depending on the interval and aggregation you'll get one or more rows with statistics.

```csharp
// Get analytics for all time
var analytics = await context.Site.GetAnalyticsAsync();

// Get analytics for the last 7 days
var analytics = await context.Site.GetAnalyticsAsync(new AnalyticsOptions { Interval = AnalyticsInterval.LastSevenDays });

// Get analytics for a custom interval for 11 days --> you'll see 11 rows with statistic data, one per day
DateTime startDate = DateTime.Now - new TimeSpan(20, 0, 0, 0);
DateTime endDate = DateTime.Now - new TimeSpan(10, 0, 0, 0);

var analytics = await context.Site.GetAnalyticsAsync(
                new AnalyticsOptions
                {
                    Interval = AnalyticsInterval.Custom,
                    CustomStartDate = startDate,
                    CustomEndDate = endDate,
                    CustomAggregationInterval = AnalyticsAggregationInterval.Day
                });
```

> [!Note]
> The value of the `CustomStartDate` and `CustomEndDate` parameters must represent a time range of less than 90 days.
