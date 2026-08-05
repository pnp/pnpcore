using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UserModel = PnP.Core.Provisioning.Model.AzureActiveDirectory.User;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Creates the Microsoft Entra users a tenant template declares, and assigns their licences.
    /// </summary>
    internal class ObjectAzureActiveDirectory : ObjectHierarchyHandlerBase
    {
        public override string Name => "Azure Active Directory";

        public override bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ApplyConfiguration configuration)
        {
            _willProvision ??= hierarchy?.AzureActiveDirectory?.Users?.Count > 0;
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            // Matching PnP Framework: users are not extracted. A tenant's directory is not a property
            // of the sites being extracted, and a template full of real people is not something to
            // produce by accident.
            _willExtract ??= false;
            return _willExtract.Value;
        }

        public override Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(hierarchy);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            string sequenceId, TokenParser parser, ApplyConfiguration configuration)
        {
            if (!(hierarchy?.AzureActiveDirectory?.Users?.Count > 0))
            {
                return parser;
            }

            int index = 0;

            foreach (UserModel user in hierarchy.AzureActiveDirectory.Users)
            {
                index++;

                string principalName = parser.ParseString(user.UserPrincipalName);
                WriteSubProgress("User", principalName, index, hierarchy.AzureActiveDirectory.Users.Count);

                try
                {
                    string userId = await CreateOrUpdateUserAsync(context, user, principalName, parser)
                        .ConfigureAwait(false);

                    if (string.IsNullOrEmpty(userId))
                    {
                        continue;
                    }

                    if (user.Licenses?.Count > 0)
                    {
                        await AssignLicensesAsync(context, userId, user, parser).ConfigureAwait(false);
                    }

                    await SetPhotoAsync(context, hierarchy.Connector, user, userId, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The user '{principalName}' could not be provisioned: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            WriteMessage("Done processing users", ProvisioningMessageType.Completed);

            return parser;
        }

        #region Users

        /// <summary>
        /// Creates the user, or patches the one that already has that principal name.
        /// </summary>
        /// <returns>The user's object id, or null if it could not be created or found.</returns>
        private async Task<string> CreateOrUpdateUserAsync(PnPContext context, UserModel user, string principalName,
            TokenParser parser)
        {
            Dictionary<string, object> body = BuildUserBody(user, principalName, parser);

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    HttpMethod.Post, ApiRequestType.Graph, "users", JsonSerializer.Serialize(body)))
                    .ConfigureAwait(false);

                return IdOf(response.Response);
            }
            catch (Exception ex) when (IsAlreadyExists(ex))
            {
                string existingId = await FindUserIdAsync(context, principalName).ConfigureAwait(false);

                if (existingId == null)
                {
                    // Graph said the principal name is taken and then could not find it, which is
                    // worth reporting rather than swallowing - it usually means a soft-deleted user.
                    string warning = $"'{principalName}' already exists but could not be read back, so it " +
                        "was skipped. A soft deleted user with the same principal name will do this.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    return null;
                }

                // A password profile cannot be patched onto an existing user the way it is set on a
                // new one, and re-sending it would reset a real person's password.
                body.Remove("passwordProfile");
                body.Remove("userPrincipalName");

                await context.Web.ExecuteRequestAsync(new ApiRequest(
                    new HttpMethod("PATCH"), ApiRequestType.Graph, $"users/{existingId}",
                    JsonSerializer.Serialize(body))).ConfigureAwait(false);

                return existingId;
            }
        }

        private static Dictionary<string, object> BuildUserBody(UserModel user, string principalName, TokenParser parser)
        {
            var body = new Dictionary<string, object>
            {
                ["accountEnabled"] = user.AccountEnabled,
                ["displayName"] = parser.ParseString(user.DisplayName),
                ["mailNickname"] = parser.ParseString(user.MailNickname),
                ["userPrincipalName"] = principalName,
                ["givenName"] = parser.ParseString(user.GivenName),
                ["surname"] = parser.ParseString(user.Surname),
                ["jobTitle"] = parser.ParseString(user.JobTitle),
                ["mobilePhone"] = parser.ParseString(user.MobilePhone),
                ["officeLocation"] = parser.ParseString(user.OfficeLocation),
                ["preferredLanguage"] = parser.ParseString(user.PreferredLanguage),
                ["userType"] = "Member",
                ["usageLocation"] = parser.ParseString(user.UsageLocation),
                ["passwordPolicies"] = parser.ParseString(user.PasswordPolicies),
            };

            if (user.PasswordProfile != null)
            {
                body["passwordProfile"] = new Dictionary<string, object>
                {
                    ["forceChangePasswordNextSignIn"] = user.PasswordProfile.ForceChangePasswordNextSignIn,
                    ["forceChangePasswordNextSignInWithMfa"] = user.PasswordProfile.ForceChangePasswordNextSignInWithMfa,
                    ["password"] = EncryptionUtility.ToInsecureString(user.PasswordProfile.Password),
                };
            }

            // Graph rejects an explicit null for several of these, and a template that omits a field
            // means "leave it alone" rather than "clear it".
            foreach (string empty in body.Where(p => p.Value is string s && string.IsNullOrEmpty(s))
                .Select(p => p.Key).ToList())
            {
                body.Remove(empty);
            }

            return body;
        }

        /// <summary>
        /// Finds a user by principal name.
        /// </summary>
        private static async Task<string> FindUserIdAsync(PnPContext context, string principalName)
        {
            string filter = Uri.EscapeDataString($"userPrincipalName eq '{principalName.Replace("'", "''")}'");

            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                ApiRequestType.Graph, $"users?$filter={filter}&$select=id")).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Response))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(response.Response))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                foreach (JsonElement item in value.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out JsonElement id) && id.ValueKind == JsonValueKind.String)
                    {
                        return id.GetString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Whether the failure is Graph saying the object is already there.
        /// </summary>
        private static bool IsAlreadyExists(Exception ex)
        {
            string text = ex.ToString();

            return text.IndexOf("Request_ResourceExists", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("ObjectConflict", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string IdOf(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                return document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("id", out JsonElement id)
                    && id.ValueKind == JsonValueKind.String
                    ? id.GetString()
                    : null;
            }
        }

        #endregion

        #region Licences

        /// <summary>
        /// Brings the user's assigned licences in line with the template.
        /// </summary>
        private async Task AssignLicensesAsync(PnPContext context, string userId, UserModel user, TokenParser parser)
        {
            var wanted = new List<Dictionary<string, object>>();
            var wantedSkus = new HashSet<Guid>();

            foreach (Model.AzureActiveDirectory.UserLicense license in user.Licenses)
            {
                if (!Guid.TryParse(parser.ParseString(license.SkuId), out Guid skuId))
                {
                    Warn(context, $"'{license.SkuId}' is not a usable licence sku id, so it was skipped.");
                    continue;
                }

                wantedSkus.Add(skuId);

                wanted.Add(new Dictionary<string, object>
                {
                    ["skuId"] = skuId,
                    ["disabledPlans"] = (license.DisabledPlans ?? Array.Empty<string>())
                        .Select(p => parser.ParseString(p))
                        .Where(p => Guid.TryParse(p, out _))
                        .Select(Guid.Parse)
                        .ToList(),
                });
            }

            if (wanted.Count == 0)
            {
                return;
            }

            List<Guid> toRemove = (await AssignedSkusAsync(context, userId).ConfigureAwait(false))
                .Where(sku => !wantedSkus.Contains(sku))
                .ToList();

            var body = new Dictionary<string, object>
            {
                ["addLicenses"] = wanted,
                ["removeLicenses"] = toRemove,
            };

            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                $"users/{userId}/assignLicense", JsonSerializer.Serialize(body))).ConfigureAwait(false);
        }

        private static async Task<List<Guid>> AssignedSkusAsync(PnPContext context, string userId)
        {
            var skus = new List<Guid>();

            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                ApiRequestType.Graph, $"users/{userId}?$select=assignedLicenses")).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Response))
            {
                return skus;
            }

            using (JsonDocument document = JsonDocument.Parse(response.Response))
            {
                if (!document.RootElement.TryGetProperty("assignedLicenses", out JsonElement assigned)
                    || assigned.ValueKind != JsonValueKind.Array)
                {
                    return skus;
                }

                foreach (JsonElement license in assigned.EnumerateArray())
                {
                    if (license.TryGetProperty("skuId", out JsonElement skuId)
                        && skuId.ValueKind == JsonValueKind.String
                        && Guid.TryParse(skuId.GetString(), out Guid parsed))
                    {
                        skus.Add(parsed);
                    }
                }
            }

            return skus;
        }

        #endregion

        #region Photo

        /// <summary>
        /// Reports a declared profile photo, which this engine cannot yet upload.
        /// </summary>
        private Task SetPhotoAsync(PnPContext context, FileConnectorBase connector, UserModel user,
            string userId, TokenParser parser)
        {
            if (string.IsNullOrEmpty(user.ProfilePhoto))
            {
                return Task.CompletedTask;
            }

            Warn(context, $"A profile photo is declared for '{parser.ParseString(user.UserPrincipalName)}' " +
                "but this engine cannot upload one yet: the Graph call needs a binary request body, and " +
                "PnP Core's ApiRequest only carries text. Set it by hand, or through Graph directly.");

            return Task.CompletedTask;
        }

        #endregion

        private void Warn(PnPContext context, string message)
        {
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }
    }
}
