# Working with multilingual pages

SharePoint does support multilingual pages in communication sites and the page API has some handy methods to help with that.

> [!Important]
> Multilingual pages only work on SharePoint Communication sites.

## Ensuring your site is correctly configured for multilingual pages

Before you use multilingual pages on a site you need to ensure the site is configured to support multilingual pages. There are 2 pre-requisites:

- The page needs to support the needed languages
- The multilingual page feature must have been activated

The easiest approach to ensuring a site is ready for multilingual is calling the [EnsureMultilingualAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_EnsureMultilingualAsync_PnP_Core_Model_SharePoint_List_System_Int32__) on the [web object](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html).

```csharp
// Enable this site for multilingual pages
await context.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });
```

Above sample enables the site for Dutch (1043) and French (1036) and enables the multilingual page feature if it was not yet enabled.

> [!Note]
> Paul Bullock's [blog article on SharePoint Online language ids](https://capacreative.co.uk/resources/reference-sharepoint-online-languages-ids/) is a useful resource to pick the right id for the language you need.

## Getting the available translations of an existing page

If you load a page on a site you do not always know which translated versions are available and in what state those translations are. Calling the [GetPageTranslationsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_GetPageTranslationsAsync) will get you a [IPageTranslationStatusCollection object](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPageTranslationStatusCollection.html) that contains a list of languages for which the page was not yet translated and a collection of [IPageTranslationStatus objects](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPageTranslationStatus.html) giving you information about the existing page translations.

> [!Note]
> A page needs to be saved before you can use the multilingual API.

```csharp
// Enable this site for multilingual pages
await context.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });

// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Page should not yet have translations
var pageTranslations = await newPage.GetPageTranslationsAsync();
```

## Create translations of a given page

If you've created a new page or when you've added a new language for your site you might want to also create the page translations for the new page or backfill the existing pages with a translation for the added language. Both of these tasks can be achieved by using the [TranslatePagesAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_TranslatePagesAsync_PnP_Core_Model_SharePoint_PageTranslationOptions_). When you call this method without input it will automatically create page translations for each site language for which there was not yet a translated page. You can also pass in a [PageTranslationOptions object](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.PageTranslationOptions.html) specifying the languages to generate a translation for.

> [!Note]
> A page needs to be saved before you can use the multilingual API.

```csharp
// Enable this site for multilingual pages
await context.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });

// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Generate page translations: will create a page translations in the respective nl and fr folders
pageTranslations = await page.TranslatePagesAsync();
```
