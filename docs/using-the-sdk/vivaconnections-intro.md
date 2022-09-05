# Customizing Microsoft Viva Connections

Microsoft Viva Connections is your gateway to a modern employee experience designed to keep everyone engaged and informed. Viva Connections gives everyone a personalized destination where you'll discover relevant news, conversations, and the tools they need to succeed. Checkout [this article](https://docs.microsoft.com/en-us/viva/connections/viva-connections-overview#:~:text=Viva%20Connections%20is%3A&text=A%20gateway%20to%20employee%20experiences,Teams%2C%20Yammer%2C%20and%20Stream.) to learn more.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for adding, updating and deleting data
}
```

## Discovering the tenant home site

Currently a Microsoft 365 tenant can have only one site marked as "home site". This home site is important for Viva Connections as that Viva Connections dashboard is defined and managed from within that home site. To verify if the site collection you're using is a home site you can use one of the `IsHomeSite` methods on `ISite`:

```csharp
 if (await context.Site.IsHomeSiteAsync())
 {
     // Continue with customizing the Viva Connections dashboard
 }
```

## Customizing the Viva Connections dashboard

The Microsoft Viva Connections dashboard is the key component of the Microsoft Viva setup, the dashboard is comprised out of a set of adaptive card extensions (ACEs) which are added on a page. Similar to SharePoint pages there are a number of out of the box adaptive card extensions (e.g. the Card Designer ACE) and the option to build your own custom adaptive card extensions. If you want to built your own ACEs then check out the [developer documentation](https://docs.microsoft.com/en-us/sharepoint/dev/spfx/viva/overview-viva-connections).

### Reading the dashboard

A Microsoft Viva Connections dashboard is represented via the `IVivaDashboard` interface in PnP Core SDK. After having loaded the dashboard you can add, update and remove ACEs from the dashboard and save the changes. You can also enumerate the currently added ACEs and inspect them. To read the Microsoft Viva Connections dashboard you will need to use one of the `GetVivaDashboard` methods on `IWeb`:

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

foreach(var ACE in dashboard.ACEs)
{
    // loop over the ACEs that were added to the dashboard
}
```

### Adding an ACE to the dashboard: typed approach

To add an ACE to the dashboard you first need to instantiate an `AdaptiveCardExtension` or a class inheriting from that one (e.g. `CardDesignerACE`, `AssignedTasksACE` or `TeamsACE`). When instantiating that class the constructor allows you to specify the ACE card size to use on the dashboard, default will be `CardSize.Medium`, but you can also choose `CardSize.Large`. When using the typed approach, which is available for some of the ACEs, you can build the ACE configuration in a typed manner as shown in the below sample. Once the ACE has been created and configured it can be added to the dashboard via an `AddACE` method and finally the dashboard changes are persisted via one of the `Save` methods.

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

var cardDesignerACE = new CardDesignerACE(CardSize.Large)
{
    Title = "Test Card Designer ACE",
    Description = "Test description",
    Properties = new CardDesignerProps
    {
        DataType = "Static",
        TemplateType = "primaryText",
        CardIconSourceType = 1,
        CardImageSourceType = 1,
        CardSelectionAction = new ExternalLinkAction
        {
            Parameters = new ExternalLinkActionParameter
            {
                Target = "https://bing.com"
            }
        },
        NumberCardButtonActions = 2,
        CardButtonActions = new List<ButtonAction>
        {
            new ButtonAction
            {
                Title = "Test 1",
                Style = "positive",
                Action = new ExternalLinkAction
                {
                    Parameters = new ExternalLinkActionParameter
                    {
                        Target = "https://google.com"
                    }
                }
            },
            new ButtonAction
            {
                Title = "Test 2",
                Style = "default",
                Action = new QuickViewAction
                {
                    Parameters = new QuickViewActionParameter
                    {
                        View = "quickView"
                    }
                }
            }
        },
        QuickViews = new List<QuickView>
        {
            new QuickView
            {
                Data = "{\n  \"Url\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"Text\": \"Hello, World!\"\n}",
                Template = "{\n  \"type\": \"AdaptiveCard\",\n  \"body\": [\n    {\n      \"type\": \"TextBlock\",\n      \"size\": \"Medium\",\n      \"weight\": \"Bolder\",\n      \"text\": \"${Text}\",\n      \"wrap\": true\n    }\n  ],\n  \"actions\": [\n    {\n      \"type\": \"Action.OpenUrl\",\n      \"title\": \"View\",\n      \"url\": \"${Url}\"\n    }\n  ],\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n  \"version\": \"1.2\"\n}",
                Id = "quickView",
                DisplayName = "Test Quick View"
            }
        },
        QuickViewConfigured = false,
        PrimaryText = "Test heading",
        CustomImageSettings = new CustomImageSettings
        {
            Type = 1,
            AltText = "There should be an image here",
            ImageUrl = "https://cdn.hubblecontent.osi.office.net/m365content/publish/06faab94-9fb9-43b2-a274-9f0f51fedc3c/982488044.jpg"
        }
    }
};

// Add the ACE to the dashboard on position 10
dashboard.AddACE(cardDesignerACE, 10);

// Persist the dashboard
await dashboard.SaveAsync();
```

### Adding an ACE to the dashboard: untyped approach

Whereas above approach showed a typed way of configuring the ACE you can also use a generic approach. In the generic way you provide the ACE configuration as a JSON bloc. There are two possible options, both shown in below sample. In option B the ACE default settings are applied, so if you'd not set any specific properties you will end up with the default ACE configuration as like you would get when manually adding the ACE to the dashboard. When using option A you're required to also set the `Properties`.

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

// Instantiate the ACE: Option A
var cardDesignerACE = new AdaptiveCardExtension(CardSize.Large);
cardDesignerACE.Title = "PnP Rocks!";
cardDesignerACE.Description = "Text+image card. Text on second line. We can ";
cardDesignerACE.Id = VivaDashboard.DefaultACEToId(DefaultACE.CardDesigner);

// Instantiate the ACE: Option B
var cardDesignerACE = dashboard.NewACE(DefaultACE.CardDesigner, CardSize.Large);

// Set the ACE properties
cardDesignerACE.Properties = JsonSerializer.Deserialize<JsonElement>("{\"templateType\":\"image\",\"cardIconSourceType\":2,\"cardImageSourceType\":1,\"cardSelectionAction\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}},\"numberCardButtonActions\":1,\"cardButtonActions\":[{\"title\":\"Button\",\"style\":\"positive\",\"action\":{\"type\":\"ExternalLink\",\"parameters\":{\"target\":\"https://www.bing.com/\"}}},{\"title\":\"Button\",\"style\":\"default\",\"action\":{\"type\":\"QuickView\",\"parameters\":{\"view\":\"quickView\"}}}],\"quickViews\":[{\"data\":\"\",\"template\":\"\",\"id\":\"quickView\",\"displayName\":\"Default Quick View\"}],\"isQuickViewConfigured\":true,\"currentQuickViewIndex\":0,\"dataType\":\"Static\",\"spRequestUrl\":\"\",\"requestUrl\":null,\"graphRequestUrl\":\"\",\"primaryText\":\"Text+image card. Text on second line. We can \",\"cardIconCustomImageSettings\":null,\"aceData\":{\"cardSize\":null},\"title\":\"PnP rocks!\",\"description\":\"Description text\",\"iconProperty\":\"https://image.flaticon.com/icons/png/512/747/747055.png\",\"cardImageCustomImageSettings\":{\"type\":1,\"altText\":\"Image thumbnail preview\",\"imageUrl\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"},\"imagePicker\":\"https://bertonline.sharepoint.com/SiteAssets/SitePages/newpage/40298-NewPerspective-newspost.jpg\"}");


// Add the ACE to the dashboard on position 10
dashboard.AddACE(cardDesignerACE, 10);

// Persist the dashboard
await dashboard.SaveAsync();
```

### Adding 3rd party ACEs to the dashboard

The techniques shown to add out of the box ACEs also apply to 3rd party ACEs, although there are minor differences in the initial instantiation.

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

// Instantiate the ACE: Option A
var customACE = new AdaptiveCardExtension();
cardDesignerACE.Title = "Custom Async ACE";
cardDesignerACE.Description = "something";
cardDesignerACE.Id = "9e73ef29-1b62-4084-92b5-207bedea22b8";

// Instantiate the ACE: Option B
var customACE = dashboard.NewACE(new Guid("9e73ef29-1b62-4084-92b5-207bedea22b8"));
customACE.Title = "Custom Async ACE";
customACE.Description = "something";

// Set the ACE properties
customACE.Properties = JsonSerializer.Deserialize<JsonElement>("{...}");

// Add the ACE to the dashboard as first ACE
dashboard.AddACE(customACE);

// Persist the dashboard
await dashboard.SaveAsync();
```

### Removing an ACE from the dashboard

To remove an ACE from a dashboard simply use the `RemoveACE` method providing the ACE instance id as key and then save the dashboard again.

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

// Get the first card designer ACE
var firstCardDesignerACE = dashboard.ACEs.FirstOrDefault(p => p.ACEType == DefaultACE.CardDesigner)

// Remove the ACE from the dashboard
dashboard.RemoveACE(firstCardDesignerACE.InstanceId);

// Persist the dashboard
await dashboard.SaveAsync();
```

### Updating an ACE

You can also update existing ACEs by using the `UpdateACE` method followed by saving the dashboard changes again. When updating an ACE you need to specify the updated `AdaptiveCardExtension` instance:

```csharp
IVivaDashboard dashboard = await context.Web.GetVivaDashboardAsync();

// Get the first card designer ACE
var firstCardDesignerACE = dashboard.ACEs.FirstOrDefault(p => p.ACEType == DefaultACE.CardDesigner)

// Update the ACE
firstCardDesignerACE.Title = "Updated title";
firstCardDesignerACE.CardSize = CardSize.Medium;

// Update the ACE, but also move it to be the last card on the dashboard
dashboard.UpdateACE(firstCardDesignerACE, 500);

// Persist the dashboard
await dashboard.SaveAsync();
```

### Using a typed approach for your own 3rd party ACEs

If you whish, you can extend the typed support like there is for some of the 1st party ACEs to your own 3rd party ACEs by implementing the `AdaptiveCardExtension`, `ACEProperties` and `ACEFactory` classes:

```csharp
internal class CustomAsyncCard : AdaptiveCardExtension<CustomAsyncCardProps>
{
    public CustomAsyncCard()
    {
        Id = "9e73ef29-1b62-4084-92b5-207bedea22b8";
        Title = "Async Card";
    }
}

internal class CustomAsyncCardProps : ACEProperties
{
    [JsonPropertyName("myProp")]
    public string MyACEProp { get; set; } = "default value";
}

internal class CustomAsyncCardFactory : ACEFactory
{
    public override string ACEId => "9e73ef29-1b62-4084-92b5-207bedea22b8";

    public override AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
    {
        return new CustomAsyncCard()
        {
            Id = control.WebPartId,
            Description = control.Description,
            InstanceId = control.InstanceId,
            Title = control.Title,
            Properties = JsonSerializer.Deserialize<CustomAsyncCardProps>(control.PropertiesJson)
        };
    }
}
```

Once you've implementing the typed model for your ACE you need to register your custom ACE factory with the dashboard so when reading a dashboard you ACE properties are loaded into the typed model you've defined:

```csharp
// Load the dashboard again
dashboard = context.Web.GetVivaDashboard();

// Register custom ACE factory
dashboard.RegisterCustomACEFactory(new CustomAsyncCardFactory());
```
