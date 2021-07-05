# Publishing and promoting pages as news

Once a page has been created it sits in draft status and it will not be seen as news on the site's home page. You can publish a page and promote it by posting the page as news or you can make the page the home page of your site.

> [!Note]
> A page needs to be saved before you can use any of the "promotion" APIs.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

## Publishing a page

After a page has been created, publishing it is as simple as calling the [PublishAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_PublishAsync).

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Configure the page

// Save the page
await page.SaveAsync("PageA.aspx");

// Publish the page
await page.PublishAsync();
```

## Scheduling the publishing a page

After a page has been created you can immediately publish the page as described above, but you can also opt to schedule the page publication for a certain date. To do so you need to use one of the `SchedulePublish` methods. This method will verify the pages library does have scheduled publishing turned on via calling the `EnsurePageScheduling` method on the connected `IWeb`. To verify if a page has a pending publication scheduled you can verify the `ScheduledPublishDate` property. This property is a nullable property, if there was no publication scheduled this property will not contain a value. If you want to remove the scheduled publication of a page you need to use one of the `RemoveSchedulePublish` methods.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Configure the page

// Save the page
await page.SaveAsync("PageA.aspx");

// Schedule the publication of the page for 24 hours later
var scheduleDate = DateTime.Now + new TimeSpan(24, 0, 0);
await page.SchedulePublishAsync(scheduleDate);

// Verify the set the page publication date
if (ScheduledPublishDate.HasValue)
{
    var pagePublicationDate = page.ScheduledPublishDate.Value;
}

// Removed the scheduled publication again
await page.RemoveSchedulePublishAsync();
```

## Posting a page as news article

A page can get more visibility by posting it as a news post. Calling the [PromoteAsNewsArticleAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_PromoteAsNewsArticleAsync) is all you need to do.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Configure the page

// Save the page
await page.SaveAsync("PageA.aspx");

// Post as news
await page.PromoteAsNewsArticleAsync();

// Publish the page (recommended after posting as news but not required)
await page.PublishAsync();
```

## Demoting a news article back to a regular page

Demoting an existing news post to a regular page can be done with the [DemoteNewsArticleAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_DemoteNewsArticleAsync).

```csharp
// demote as news article
await page.DemoteNewsArticleAsync();
```

## Promoting a page as site home page

If you want to set your page as the site's home page you have two options: you can use the convenient [PromoteAsHomePageAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_PromoteAsHomePageAsync) on the page object or you can load the web's `RootFolder` and set the `WelcomePage` property to the page you want to set as home page. The first approach is the recommended manner.

```csharp

// Promote as home page of the site

// OPTION 1
await page.PromoteAsHomePageAsync();

// OPTION 2
var web = await context.Web.GetAsync(p => p.RootFolder);
web.RootFolder.WelcomePage = "SitePages/PageA.aspx";
await web.RootFolder.UpdateAsync();
```
