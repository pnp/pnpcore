
# SharePoint

- ~~Loading lists of a web is returning a subset of the lists being returned when using REST:~~
	~~- You cannot retrieve the "system" libraries (like "Site Pages", "Form Templates", "Site Assets", and "Style Library") via /lists GET query~~
	~~- This makes it hard to use Graph as our users are agnostic of what API is being called, but they expect the same results in all cases~~
- GroupID property is not returned when loading a SharePoint Site ==> we need GroupId to be able to load the Team linked to this site (if there's one)
- It looks like it is not possible to filter items in lists and libraries by ContentType
- It is not possible to load subwebs of a site via Graph
- The OData querystring parameters (accordingly to https://docs.microsoft.com/en-us/graph/query-parameters) should be with $ on v1. However, if we apply $select, $filter, $expand to SPO resources, they work if and only if we remove $
- When you query for list items with filters against non-indexed fields you need to provide a custom HTTP header (Prefer: HonorNonIndexedQueriesWarningMayFailRandomly)
- The fields/* construct is a bit weird from a developer point of view

## SharePoint Graph details

### Number of lists being returned

Graph call: https://graph.microsoft.com/v1.0/sites/{webid}/lists returns 3 lists:
- Documents (template documentLibrary)
- Content type publishing error log (template genericList, hidden)
- 19:9f4c089b2a87472ebd357031a7c11be9@thread.tacv2_wiki (template genericList, hidden) ==> due to Teams backend

REST call: _api/web/Lists returns 19 lists:
- Documents (Shared%20Documents)
- Site Assets (SiteAssets)
- Site Pages (SitePages)
- Style Library (Style%20Library)
- Form Templates (FormServerTemplates)
- TaxonomyHiddenList (Lists/TaxonomyHiddenList, hidden)
- Converted Forms (IWConvertedForms, hidden)
- Content type publishing error log (Lists/ContentTypeSyncLog, hidden)
- 19:9f4c089b2a87472ebd357031a7c11be9@thread.tacv2_wiki (Lists/199f4c089b2a87472ebd357031a7c11be9threadtacv2_wiki, hidden) ==> due to Teams backend
- Master Page Gallery (_catalogs/masterpage, hidden)
- Maintenance Log Library (_catalogs/MaintenanceLogs, hidden)
- List Template Gallery (_catalogs/lt, hidden)
- Solutions (_catalogs/solutions, hidden)
- Theme Gallery (_catalogs/theme, hidden)
- Composed Looks (_catalogs/design, hidden)
- Web Part Gallery (_catalogs/wp, hidden)
- AppFiles (_catalogs/appfiles, hidden)
- AppData (_catalogs/appdata, hidden)
- User Information List (_catalogs/users, hidden)


FIX: request the system facet to retrieve all lists: https://graph.microsoft.com/v1.0/sites/{webid}/lists?$select=system,field1,field2,...


# Teams

- We miss a single call that load all Teams with their properties
- Doing batching of tab creation works inconsistently: getting "BadGateway", "Failed to execute backend request." errors. 
	- Lowering the batch size to less than the max 20 seems to make it more reliable but failing most of the time
	- Trying the same with adding messages to a channel gives HTTP 412	Precondition Failed (https://docs.microsoft.com/en-us/graph/errors), even with lowered batch count ==> it seems to work with 5 messages in a batch
- When getting a Teams channel message there's no way to know if the message has replies besides querying each message independently for replies
- ~~One can update MemberSettings.AllowCreatePrivateChannels using v1.0 endpoint, but not read it~~
- If you try to add a tab with a SharePoint Online document library (com.microsoft.teamspace.tab.files.sharepoint) and that library has a "non standard" URL (like /lists/MyLibrary instead of /MyLibrary) the Graph request fails with a BadGateway exception, which is misleading
- It is not possible to query Channels with $orderby, $top, $skip
- It is not possible to query Messages in Channels with $select, $filter, $orderby, $top, $skip
- One can archive a team in a batch, but unarchive in a batch does not work unless we send along a fake body

# Taxonomy

- ~~Deleting a termset using DELETE ~termstore/sets/id does not work (known issue, will be fixed)~~
- ~~Using the Terms part of the API (e.g. GET ~ termstore/sets/id/terms) throws an error : "Access Denied: End point cannot be called by the current user" ==> seems to be documentation issue as using terms in only usable to retrieve a single term. Will be fixed.~~
- Deleting a term group (DELETE ~termstore/groups/id) immediately after deleting the termsets inside throws an error "cannot delete non empty term groups". Issue is listed with the graph team and being investigated
