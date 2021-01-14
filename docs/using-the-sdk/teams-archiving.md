# Working with Teams: Team Archiving

The Core SDK provides support for archiving and unarchiving Teams.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Archiving a Team

To archive a Team, use the Archive method. The following examples show how a Team can be archived including waiting for an operation to complete:

```csharp
// Get the Team
var team = await context.Team.GetAsync();

// Perform the archiving operation 
var archiveOperation = await team.ArchiveAsync();
 
// lets wait for the operation to complete
await archiveOperation.WaitForCompletionAsync();

// reload from the server
await context.Team.GetAsync();

// Server side should be archived as well now
var archiveStatus = team.IsArchived;

```

## Unarchiving a Team

To perform an operation to unarchive a Team use the following example to show you how to do it:

```csharp
// Get the Team
var team = await context.Team.GetAsync();

// Perform the unarchiving operation
var unarchiveOperation = await team.UnarchiveAsync();

// lets wait for the operation to complete
await unarchiveOperation.WaitForCompletion();

// reload from the server
context.Team.GetAsync();

// Server side should be archived as well now
var archiveStatus = team.IsArchived;
```