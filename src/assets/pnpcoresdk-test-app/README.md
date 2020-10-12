## pnpcoresdk-test-app

This SPFx solution contain a sample of client side artifacts

> [!Important]
>This has to be outside of any C# solution folder to prevent issues with MS Build

Type                    |       Id                              | Alias
------------------------|---------------------------------------|-----------------------------
WebPart                 | 9a57f808-ca0e-408e-b28c-319a9c8204ed  | PnPCoreSdkBannerWebPart
ApplicationCustomizer   | a54612b1-e5cb-4a43-80ae-3b5fb6ce1e35  | PnPCoreSdkHeaderApplicationCustomizer
FieldCustomizer         | 5d917ef1-ab2a-4f31-a727-d2da3374b9fa  | PnPCoreSdkFieldCustomizerFieldCustomizer
ListViewCommandSet      | d2480b66-32cb-4e94-87eb-75895fd3dcc6  | PnPCoreSdkTestCommandCommandSet

### Building the code

```bash
git clone the repo
npm i
npm i -g gulp
gulp
```

## Testing the app locally

Update the tokens **[yourtenant]** in *config/serve.json* and run `gulp serve --config=<the appropriate config>`

This package produces the following:

* lib/* - intermediate-stage commonjs build artifacts
* dist/* - the bundled script, along with other resources
* deploy/* - all resources which should be uploaded to a CDN.

### Build options

gulp clean - Cleaned the compiled resources
gulp serve - Serve the locally developped bundle from memory
gulp bundle - Bundle the JS app
gulp package-solution - Package the solution (.sppkg file)
