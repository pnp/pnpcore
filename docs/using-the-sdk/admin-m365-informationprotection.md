# Information protection

The Core SDK Admin library provides Microsoft 365 Admin APIs for working with information protection.

[!INCLUDE [Microsoft 365 Admin setup](fragments/setup-admin-m365.md)]

## Enumerate the available sensitivity labels

[Sensitivity labels from the Microsoft Information Protection solution](https://docs.microsoft.com/en-us/microsoft-365/compliance/sensitivity-labels?view=o365-worldwide) let you classify and protect your organization's data, while making sure that user productivity and their ability to collaborate isn't hindered. When you want to set a sensitivity label you first need to enumerate the labels to understand which labels there are via one of the `GetSensitivityLabels` methods:

```csharp
var labels = await context.GetMicrosoft365Admin().GetSensitivityLabelsAsync();
```
