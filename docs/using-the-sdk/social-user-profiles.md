# User profiles APIs

User profiles APIs allow you to read and write user profile properties, change profile picture url, manage OneDrive quotas and other profile-related operations.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and shown below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
}
```

## Getting user's properties

To get the current user's properties use below code:

```csharp
IPersonProperties properties = await context.Social.UserProfile.GetMyPropertiesAsync();
```

The return value contains user properties - such as `AccountName`, `Email`, `PersonalSiteHostUrl`, `Title`, `DirectReports`, etc. and the profile properties - the set of properties coming and configuring under the user profile service. This set of properties is available under `UserProfileProperties` property.

The amount of return data is quite big, since it includes `UserProfileProperties` by default. If you need only a few properties, you can apply a select query:

```csharp
IPersonProperties properties = await context.Social.UserProfile.GetMyPropertiesAsync(p => p.DisplayName, p => p.AccountName);
```

In the case above only `DisplayName` and `AccountName` will be initialized.

You can also query user properties for the specific person by account's name using `GetPropertiesFor`:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
IPersonProperties properties = await context.Social.UserProfile.GetPropertiesForAsync(accountName, p => p.DisplayName, p => p.AccountName);
```

Finally, if you have a need to get only one specific user profile property, you can use the method `GetPropertyFor`:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
string propertyValue = await context.Social.UserProfile.GetPropertyForAsync(accountName, "FirstName");
```

Some properties contain multiple values, in this case they will be separated with "`|`" character.

## Setting user profile properties

PnP Core SDK allows you to set user profile properties. To do so, you should call corresponding method:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
await context.Social.UserProfile.SetSingleValueProfilePropertyAsync(accountName, "FirstName", "John");
```

The code above changes the first name for the account.

If you have a need to change multi-value property, use `SetMultiValuedProfileProperty`:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
var skills = new List<string>() { "csharp", "typescript" };
await context.Social.UserProfile.SetMultiValuedProfilePropertyAsync(accountName, "SPS-Skills", skills);
```

Please note, that the code above replaces a property with a new value. To add a new value to the list, you should first retrieve the property value with `GetPropertyFor` and then update it.

## Setting profile picture

To set a user's profile picture:

```csharp
var fileBytes = System.IO.File.ReadAllBytes("path/to the picture.jpg");
await context.Social.UserProfile.SetMyProfilePictureAsync(fileBytes);
```

Due to some caching mechanism, the image might not be available immediately on the UI.

## Working with OneDrive quotas

You can also manipulate OneDrive quotas for a user.

To get user's OneDrive max quota:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
var result = await context.Social.UserProfile.GetUserOneDriveQuotaMaxAsync(accountName);
```

To reset quota to its defaults:

```csharp
var outcome = await context.Social.UserProfile.ResetUserOneDriveQuotaToDefaultAsync(accountName);
```

To set quota:

```csharp
// set quota to approx. 5TB and warning to 4TB
var outcome = await context.Social.UserProfile.SetUserOneDriveQuotaAsync(accountName, 5497558138880, 4497558138880);
```
