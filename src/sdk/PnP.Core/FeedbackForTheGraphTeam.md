
# SharePoint

- Loading lists of a web is returning a subset of the lists being returned when using REST ==> this makes it hard to use Graph as our users are agnostic of what API is being called, but they expect the same results in all cases
- GroupID property is not returned when loading a SharePoint Site ==> we need GroupId to be able to load the Team linked to this site (if there's one)

# Teams

- We miss a single call that load all Teams with their properties
- Doing batching of tab creation works inconsistently: getting "BadGateway", "Failed to execute backend request." errors. Lowering the batch size to less than the max 20 seems to make it more reliable
- When getting a Teams channel message there's no way to know if the message has replies besides querying each message independently for replies
- One can update MemberSettings.AllowCreatePrivateChannels using v1.0 endpoint, but not read it
