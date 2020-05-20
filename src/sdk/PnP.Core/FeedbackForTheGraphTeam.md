
# SharePoint

- Loading lists of a web is returning a subset of the lists being returned when using REST ==> this makes it hard to use Graph as our users are agnostic of what API is being called, but they expect the same results in all cases
- GroupID property is not returned when loading a SharePoint Site ==> we need GroupId to be able to load the Team linked to this site (if there's one)
- It looks like it is not possible to filter items in lists and libraries by ContentType
- It is not possible to load subwebs of a site via Graph
- You cannot retrieve the "system" libraries (like "Site Pages", "Form Templates", "Site Assets", and "Style Library") via /lists GET query
- The OData querystring parameters (accordingly to https://docs.microsoft.com/en-us/graph/query-parameters) should be with $ on v1. However, if we apply $select, $filter, $expand to SPO resources, they work if and only if we remove $
- When you query for list items with filters against non-indexed fields you need to provide a custom HTTP header (Prefer: HonorNonIndexedQueriesWarningMayFailRandomly)
- The fields/* construct is a bit weird from a developer point of view

# Teams

- We miss a single call that load all Teams with their properties
- Doing batching of tab creation works inconsistently: getting "BadGateway", "Failed to execute backend request." errors. 
	- Lowering the batch size to less than the max 20 seems to make it more reliable but failing most of the time
	- Trying the same with adding messages to a channel gives HTTP 412	Precondition Failed (https://docs.microsoft.com/en-us/graph/errors), even with lowered batch count ==> it seems to work with 5 messages in a batch
- When getting a Teams channel message there's no way to know if the message has replies besides querying each message independently for replies
- One can update MemberSettings.AllowCreatePrivateChannels using v1.0 endpoint, but not read it
- If you try to add a tab with a SharePoint Online document library (com.microsoft.teamspace.tab.files.sharepoint) and that library has a "non standard" URL (like /lists/MyLibrary instead of /MyLibrary) the Graph request fails with a BadGateway exception, which is misleading
