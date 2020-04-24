
# SharePoint

- Loading lists of a web is returning a subset of the lists being returned when using REST ==> this makes it hard to use Graph as our users are agnostic of what API is being called, but they expect the same results in all cases
- GroupID property is not returned when loading a SharePoint Site ==> we need GroupId to be able to load the Team linked to this site (if there's one)

# Teams

- We miss a single call that load all Teams with their properties
