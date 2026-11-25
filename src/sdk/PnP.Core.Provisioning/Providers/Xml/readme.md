# Refreshing the object model #
After updating the schema file use below command to refresh the object model:

```Cmd
xsd -c ProvisioningSchema-2022-09.xsd /n:PnP.Core.Provisioning.Providers.Xml.V202209
```

## Hand edits to re-apply after regenerating #
xsd.exe generates `[XmlText] public string[] Text` for the mixed-content types `PreferencesParameter`, `TeamTemplate` and `Theme`. Change it to `public string Text` (and the `textField` backing field to `string`) in every schema version.

Starting with .NET 11, `XmlSerializer` reads an `[XmlText] string[]` member as a whitespace-separated list, so a value such as `Team site` comes back as `Team` and `site` and the whitespace is lost. A plain `string` member keeps the text exactly as written on every runtime.
