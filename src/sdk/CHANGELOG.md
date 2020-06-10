# PnP Core SDK Changelog

*Please do not commit changes to this file, it is maintained by the repo owner.*

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- Issue templates (#34) [ypcode - Yannick Plennevaux] / [jansenbe - Bert Jansen]
- Enable PnPContext creation from an Office 365 Group id (#39) [jansenbe - Bert Jansen]
- Added the Group model, represents an Office 365 group (#48) [jansenbe - Bert Jansen]
- Added custom exception handling for authentication errors (#45) [jansenbe - Bert Jansen]
- Support to use an externally obtained access token (#44) [jansenbe - Bert Jansen]
- Add step-by-step guide on how to extend the model (#46) [ypcode - Yannick Plennevaux] / [jansenbe - Bert Jansen]
- First iteration of content type support (within the limitations of SharePoint REST) (#47) [ypcode - Yannick Plennevaux]
- Update test code so that custom settings file do not have to be marked as "copy always" #53 [jansenbe - Bert Jansen]
- Use RegisterWaitForSingleObject for the token invalidation thread in combination with IDisposable to prevent threads to leak #56 [jansenbe - Bert Jansen]
- Mark read-only properties as Get only in the interfaces to ensure SDK consumers are not trying to update them #59 [jansenbe - Bert Jansen]

### Changed

