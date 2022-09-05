# Security

The Core SDK Admin library provides Microsoft 365 Admin APIs for working with security like for example understanding if the current access token has certain permissions or uses for example application permissions.

[!INCLUDE [Microsoft 365 Admin setup](fragments/setup-admin-m365.md)]

## Verify if you're running using Application permissions

If you're application supports application and delegated permissions you might want to know whether the current user/app is using application permissions. This can be checked via the `AccessTokenUsesApplicationPermissions` methods.

```csharp
if (await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync())
{
    // We're using application permissions
}
else
{
    // We're using delegated permissions
}
```

## Verify if the current access token has a specific scope or role

When using application permissions and access token contains one or more roles that identify the permissions granted to the app for which the access token was issued. Using the `AccessTokenHasRole` methods you can verify that.

> [!Note]
> This method checks for the actual granted role, so if you check for `Sites.Read.All` and the token contains `Sites.Manage.All` then the check will return false.

Sample code:

```csharp
if (await context.GetMicrosoft365Admin().AccessTokenHasRoleAsync("Sites.Read.All"))
{
    // We have the needed permissions, let's continue
}
else
{
    // Oops...
}
```

When using delegated permissions and access token contains one or more scopes that identify the permissions granted to the app+user for which the access token was issued. Using the `AccessTokenHasScope` methods you can verify that.

> [!Note]
> This method checks for the actual granted role, so if you check for `Sites.Read.All` and the token contains `Sites.Manage.All` then the check will return false.

Sample code:

```csharp
if (await context.GetMicrosoft365Admin().AccessTokenHasScopeAsync("Sites.Read.All"))
{
    // We have the needed permissions, let's continue
}
else
{
    // Oops...
}
```
