# Common Platform Tests

These set of unit tests are designed to run on any source version of SharePoint.
This requires an element of consistance with the source content with the following rules:

* Page File Names MUST be the same in the source system but can be different in target system
* A method is provided that will rename the target to enable easy cross examination e.g. WelcomePage-1.aspx in the source is suffixed with version to become WelcomePage-1-SP2013.aspx
* Content on each source platform must be identically configured see provision folder for assets and content