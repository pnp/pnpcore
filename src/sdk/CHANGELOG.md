# PnP Core SDK Changelog

*Please do not commit changes to this file, it is maintained by the repo owner.*

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- Issue templates (#34) [ypcode - Yannick Plennevaux] / [jansenbe - Bert Jansen]
- Enable PnPContext creation from a Microsoft 365 Group id (#39) [jansenbe - Bert Jansen]
- Added the Group model, represents a Microsoft 365 group (#48) [jansenbe - Bert Jansen]
- Added custom exception handling for authentication errors (#45) [jansenbe - Bert Jansen]
- Support to use an externally obtained access token (#44) [jansenbe - Bert Jansen]
- Add step-by-step guide on how to extend the model (#46) [ypcode - Yannick Plennevaux] / [jansenbe - Bert Jansen]
- First iteration of content type support (within the limitations of SharePoint REST) (#47) [ypcode - Yannick Plennevaux]
- Update test code so that custom settings file do not have to be marked as "copy always" #53 [jansenbe - Bert Jansen]
- Use RegisterWaitForSingleObject for the token invalidation thread in combination with IDisposable to prevent threads to leak #56 [jansenbe - Bert Jansen]
- Mark read-only properties as Get only in the interfaces to ensure SDK consumers are not trying to update them #59 [jansenbe - Bert Jansen]
- Request retry mechanism, will handle core http and Graph batch requests #21 [jansenbe - Bert Jansen]
- Tweaking and updating of the writing tests documentation #65 [pkbullock - Paul Bullock]
- Paging support for Graph/Rest #3 [jansenbe - Bert Jansen]
- Added Clone method on PnPContext #49 [jansenbe - Bert Jansen]
- Extend the domain model for Web/Site Feature support #62 [pkbullock - Paul Bullock]
- Extend SP Field model (add operation for each field type) for both Web and List fields #71 [ypcode - Yannick Plennevaux]
- Added support for AddFieldAsXml #71 [ypcode - Yannick Plennevaux]
- Add basic support for FieldLinks #75 [ypcode - Yannick Plennevaux]
- Add support for external access token provider (IOAuthAccessTokenProvider) #76 [ypcode - Yannick Plennevaux]
- Added recursive support for .Include() to define which fields of a collection are loaded [jansenbe - Bert Jansen]
- Add support for getting list items via a CAML query #80 [jansenbe - Bert Jansen]
- Support RenderListDataAsStream method on lists - basic implementation #81 [jansenbe - Bert Jansen]
- Add support for calling client.svc (CSOM) endpoint #86 [jansenbe - Bert Jansen]
- Added SystemUpdate() and UpdateOverwriteVersion() methods on the ListItem model (uses CSOM) [jansenbe - Bert Jansen]
- Async LINQ support [PaoloPia - Paolo Pialorsi]
- Added sync equivalents for all async methods [jansenbe - Bert Jansen]
- Support for list and web folders #78 [ypcode - Yannick Plennevaux]

### Changed

- Documentation updates [JarbasHorst - Jarbas Horst]
- EnsurePropertiesAsync takes in account .Include (recursive) usage [jansenbe - Bert Jansen]
- Documentation updates [Ashikpaul - Ashik Paul]
- Moved to complete async internal implementation [jansenbe - Bert Jansen]
- Code cleanup (Teams implementation / ODataQuery) [JarbasHorst - Jarbas Horst]