# Using the "social" features of a page

A SharePoint Page can have comments and likes. Currently the pages API only supports disabling/enabling comments, but support to add comments and like/unlike a page will come in the future.

## Disabling/Enabling page comments

By default commenting is enabled on article pages and for the majority of use cases this default is fine. You can however also turn off commenting by calling the [DisableCommentsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_DisableCommentsAsync). Getting the current commenting status can be done using the [AreCommentsDisabledAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_AreCommentsDisabledAsync) and turning on commenting again is done via the [EnableCommentsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_EnableCommentsAsync).

> [!Note]
> A page needs to be saved before you can configure it's commenting settings.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Are comments disabled?
var commentsDisabled = await newPage.AreCommentsDisabledAsync();

// disable comments
await page.DisableCommentsAsync();

// enabled comments again
await page.EnableCommentsAsync();
```
