# Updating your site's branding

Each [modern SharePoint site has a branding defined](https://support.microsoft.com/en-us/office/change-the-look-of-your-sharepoint-site-06bbadc3-6b04-4a60-9d14-894f6a170818) that consists out of a theme and site header configuration options. Depending on the type of site there are also additional footer and navigation branding options. The site header, navigation and footer branding is also called the site's chrome.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

The branding features shown in this article can be used via the `GetBrandingManager()` method on your `PnPContext` instance.

## List the available themes

SharePoint Online sites do offer a set of out of the box themes (like Teal, Blue,...) but also custom themes can be used if available. Typically [custom themes are added by an administrator](https://docs.microsoft.com/en-us/sharepoint/dev/declarative-customization/site-theming/sharepoint-site-theming-overview) and can be used to provide a company specific branding across many sites. When loading the available themes via one of the `GetAvailableThemes` methods both the out of the box themes as the custom themes are returned, for custom themes the `IsCustomTheme` property is set to `true`.

```csharp
var themes = await context.Web.GetBrandingManager().GetAvailableThemesAsync();

foreach(var theme in themes)
{
    if (theme.IsCustomTheme)
    {
        // this is a custom theme
    }
}
```

## Set a theme to a site

You can set a theme to a site using the `SetTheme` methods, when you want to apply an out of the box theme you can simply provide the needed theme via the `SharePointTheme` enum, when you want to set a custom theme you'll first need to load the theme using one of the `GetAvailableThemes` methods and then pass the `ITheme` instance for the theme you want to set:

```csharp
// Set an out of the box theme
await context.Web.GetBrandingManager().SetThemeAsync(SharePointTheme.Teal);

// Set a custom theme
var themes = await context.Web.GetBrandingManager().GetAvailableThemesAsync();

var customTheme = themes.FirstOrDefault(p => p.IsCustomTheme);
if (customTheme != null)
{
    await context.Web.GetBrandingManager().SetThemeAsync(customTheme);
}
```

## Get the site's chrome options

Each site has a site chrome consisting out of [site header, navigation and footer branding options](https://support.microsoft.com/en-us/office/change-the-look-of-your-sharepoint-site-06bbadc3-6b04-4a60-9d14-894f6a170818). The latter is only available for sites that have communication site features enabled, the header and navigation options are available for any type of site. To list the current site chrome options you can use one of the `GetChromeOptions` methods.

```csharp
var chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync();

// for sites with communication site features enabled the returned chrome options 
// will contain header, navigation and footer objects. Other sites will only have
// the header and navigation objects loaded.
```

## Set the site's chrome options

To set the chrome options for a site you first need to load the existing chrome options using one of the `GetChromeOptions` methods, update the resulting `IChromeOptions` instance and finally use one of the `SetChromeOptions` to persist the new chrome options.

```csharp
var chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync();

// Update chrome options for a site without communication site features
chrome.Header.Emphasis = VariantThemeType.Strong;
chrome.Header.HideTitle = true;
chrome.Header.Layout = HeaderLayoutType.Extended;
chrome.Header.LogoAlignment = LogoAlignment.Middle;
chrome.Navigation.HorizontalQuickLaunch = true;
chrome.Navigation.MegaMenuEnabled = true;
chrome.Navigation.Visible = true;

await context.Web.GetBrandingManager().SetChromeOptionsAsync(chrome);

// Update chrome options for a site with communication site features
chrome.Header.Emphasis = VariantThemeType.Strong;
chrome.Header.HideTitle = true;
chrome.Header.Layout = HeaderLayoutType.Extended;
chrome.Header.LogoAlignment = LogoAlignment.Middle;
chrome.Navigation.MegaMenuEnabled = true;
chrome.Navigation.Visible = false;
chrome.Footer.Enabled = true;
chrome.Footer.Emphasis = FooterVariantThemeType.None;
chrome.Footer.Layout = FooterLayoutType.Extended;
chrome.Footer.DisplayName == "Contoso";

await context.Web.GetBrandingManager().SetChromeOptionsAsync(chrome);
```

## Set and reset the header images

When configuring a header you can set the site logo and the site logo thumbnail (square image used for example in search). Depending on the header layout setting you can also set the header background image.

### Set and reset the site logo and site logo thumbnail

The site logo images can be set via the respective `SetSiteLogo` and `SetSiteLogoThumbnail` methods and resetting them follows a similar pattern via the `ResetSiteLogo` and `ResetSiteLogoThumbnail` methods as shown in below example:

```csharp
var chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync();

// Set the header logo's
await chrome.Header.SetSiteLogoAsync("parker-ms-300.png", File.OpenRead($".{Path.DirectorySeparatorChar}parker-ms-300.png"), true);
await chrome.Header.SetSiteLogoThumbnailAsync("parker-ms-300.png", File.OpenRead($".{Path.DirectorySeparatorChar}parker-ms-300.png"), true);

// Reset the header logo's
await chrome.Header.ResetSiteLogoAsync();
await chrome.Header.ResetSiteLogoThumbnailAsync();
```

While above methods work for any modern SharePoint site the implementation is different for group connected sites (such as a Team site) as for those sites the logo is maintained via the connected group. For group connected sites the site logo and the site logo thumbnail are the same and as such is does not matter which set or reset method you use, both result in the same code being called. Another difference is on how the provided image is stored: for group connected sites the image is stored with the Microsoft 365 group, whereas for other sites the image is first uploaded to the site assets library. Consequently there's also a difference in reset behavior: for group connected sites a reset will try to load the `__siteIcon__.jpg` file from the site assets library and set that as group image, for other site types there's no dependency on an image in site assets library.

### Set and clear the header background image

Whenever the site header layout is set to `Extended` you can optionally set a header background image via the `SetHeaderBackgroundImage` methods. Clearing the header background can be done using the `ClearHeaderBackgroundImage` methods.

```csharp
var chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync();

// Ensure the extended layout is selected
chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.Extended;
await context.Web.GetBrandingManager().SetChromeOptionsAsync(chrome);

// Set the header background with a custom focal point
await chrome.Header.SetHeaderBackgroundImageAsync("pageheader.jpg", File.OpenRead($".{Path.DirectorySeparatorChar}pageheader.jpg"), 23.35, 34.66, true);

// Clear the background image
await chrome.Header.ClearHeaderBackgroundImageAsync();
```

## Set and clear the footer logo

When a site has a footer you can optionally put a logo in the footer via one of the `SetLogo` methods. Clearing a set footer logo can be done using one of the `ClearLogo` methods.

```csharp
var chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync();

// Set the footer
await chrome.Footer.SetLogoAsync("parker-ms-300.png", File.OpenRead($".{Path.DirectorySeparatorChar}parker-ms-300.png"), true);

// Clear the footer logo
await chrome.Footer.ClearLogoAsync();
```
