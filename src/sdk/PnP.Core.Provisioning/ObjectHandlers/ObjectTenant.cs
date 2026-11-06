using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SiteDesignModel = PnP.Core.Provisioning.Model.SiteDesign;
using UserProfile = PnP.Core.Provisioning.Model.SPUPS.UserProfile;
using SiteScriptModel = PnP.Core.Provisioning.Model.SiteScript;
using StorageEntityModel = PnP.Core.Provisioning.Model.StorageEntity;
using ThemeModel = PnP.Core.Provisioning.Model.Theme;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies the <c>&lt;pnp:Tenant&gt;</c> element - settings that belong to the tenant rather than
    /// to the site the template is being applied to.
    /// </summary>
    internal class ObjectTenant : ObjectHandlerBase
    {
        public override string Name => "Tenant Settings";

        public override string InternalName => "TenantSettings";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            ProvisioningTenant tenant = template.Tenant;

            _willProvision ??= tenant != null
                && (tenant.AppCatalog != null
                    || tenant.ContentDeliveryNetwork != null
                    || tenant.SiteDesigns?.Count > 0
                    || tenant.SiteScripts?.Count > 0
                    || tenant.StorageEntities?.Count > 0
                    || tenant.WebApiPermissions?.Count > 0
                    || tenant.Themes?.Count > 0
                    || tenant.SharingSettings != null);

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= false;
            return _willExtract.Value;
        }

        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            return await ApplyTenantAsync(context, template?.Tenant, template?.Connector, parser)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Applies a tenant element, wherever it hangs.
        /// </summary>
        internal async Task<TokenParser> ApplyTenantAsync(PnPContext context, ProvisioningTenant tenant,
            FileConnectorBase connector, TokenParser parser)
        {
            if (tenant == null)
            {
                return parser;
            }

            PnPContext admin;

            try
            {
                admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = "The tenant admin site could not be reached, so the tenant settings " +
                    $"were skipped. This usually means the account is not a SharePoint administrator: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            using (admin)
            {
                parser = await ApplySiteScriptsAsync(admin, connector, tenant, parser).ConfigureAwait(false);
                parser = await ApplySiteDesignsAsync(admin, tenant, parser).ConfigureAwait(false);

                await ApplyThemesAsync(admin, tenant, parser).ConfigureAwait(false);
                await ApplySharingSettingsAsync(admin, tenant).ConfigureAwait(false);
                await ApplyCdnAsync(admin, tenant, parser).ConfigureAwait(false);

                parser = await ApplyStorageEntitiesAsync(context, tenant, parser).ConfigureAwait(false);

                await ApplyUserProfilesAsync(admin, tenant, parser).ConfigureAwait(false);
                await ApplyWebApiPermissionsAsync(context, tenant, parser).ConfigureAwait(false);
                await ApplyGroupSettingsAsync(admin, tenant, parser).ConfigureAwait(false);

                ReportUnsupported(context, tenant);
            }

            return parser;
        }

        /// <summary>
        /// Names the parts of the element this handler does not yet apply.
        /// </summary>
        private void ReportUnsupported(PnPContext context, ProvisioningTenant tenant)
        {
            var pending = new List<string>();

            if (tenant.Office365GroupLifecyclePolicies?.Count > 0)
            {
                pending.Add($"{tenant.Office365GroupLifecyclePolicies.Count} Microsoft 365 group lifecycle policy(s)");
            }

            if (tenant.AppCatalog?.Packages?.Count > 0)
            {
                pending.Add($"{tenant.AppCatalog.Packages.Count} tenant app catalog package(s)");
            }

            if (pending.Count == 0)
            {
                return;
            }

            string warning = "These parts of the tenant element are not applied by this engine yet, " +
                $"so they were skipped: {string.Join(", ", pending)}.";
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
            WriteMessage(warning, ProvisioningMessageType.Warning);
        }

        #endregion

        #region Site scripts

        private async Task<TokenParser> ApplySiteScriptsAsync(PnPContext admin, FileConnectorBase connector,
            ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.SiteScripts?.Count > 0))
            {
                return parser;
            }

            List<SiteScriptUtility.SiteScriptMetadata> existing;

            try
            {
                existing = await SiteScriptUtility.GetSiteScriptsAsync(admin).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(admin, $"The existing site scripts could not be read, so none were processed: {ErrorText.Describe(ex)}");
                return parser;
            }

            foreach (SiteScriptModel script in tenant.SiteScripts)
            {
                string title = parser.ParseString(script.Title);

                try
                {
                    WriteMessage($"Processing site script {title}", ProvisioningMessageType.Progress);

                    string content = ReadScriptContent(connector, parser, script);

                    if (content == null)
                    {
                        Warn(admin, $"The site script file '{script.JsonFilePath}' is not in the template, so '{title}' was skipped.");
                        continue;
                    }

                    string description = parser.ParseString(script.Description);

                    SiteScriptUtility.SiteScriptMetadata match = existing.FirstOrDefault(
                        s => string.Equals(s.Title, title, StringComparison.Ordinal));

                    Guid id;

                    if (match == null)
                    {
                        SiteScriptUtility.SiteScriptMetadata created = await SiteScriptUtility
                            .CreateSiteScriptAsync(admin, title, description, content).ConfigureAwait(false);

                        id = created.Id;
                    }
                    else if (script.Overwrite)
                    {
                        await SiteScriptUtility.UpdateSiteScriptAsync(admin, match.Id, title, description, content)
                            .ConfigureAwait(false);

                        id = match.Id;
                    }
                    else
                    {
                        id = match.Id;
                    }

                    parser = ReplaceToken(parser, new SiteScriptIdToken(admin, title, id));
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The site script '{title}' could not be processed: {ErrorText.Describe(ex)}");
                }
            }

            return parser;
        }

        /// <summary>
        /// Reads a site script's JSON out of the template and resolves any tokens in it.
        /// </summary>
        private static string ReadScriptContent(FileConnectorBase connector, TokenParser parser, SiteScriptModel script)
        {
            byte[] bytes = TemplateFileUtilities.TryGetFileBytes(connector, parser.ParseString(script.JsonFilePath));

            return bytes == null ? null : parser.ParseString(Encoding.UTF8.GetString(bytes));
        }

        #endregion

        #region Site designs

        private async Task<TokenParser> ApplySiteDesignsAsync(PnPContext admin, ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.SiteDesigns?.Count > 0))
            {
                return parser;
            }

            List<SiteScriptUtility.SiteDesignMetadata> existing;

            try
            {
                existing = await SiteScriptUtility.GetSiteDesignsAsync(admin).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(admin, $"The existing site designs could not be read, so none were processed: {ErrorText.Describe(ex)}");
                return parser;
            }

            foreach (SiteDesignModel design in tenant.SiteDesigns)
            {
                string title = parser.ParseString(design.Title);

                try
                {
                    WriteMessage($"Processing site design {title}", ProvisioningMessageType.Progress);

                    var info = new SiteScriptUtility.SiteDesignInfo
                    {
                        Title = title,
                        Description = parser.ParseString(design.Description),
                        PreviewImageUrl = parser.ParseString(design.PreviewImageUrl),
                        PreviewImageAltText = parser.ParseString(design.PreviewImageAltText),
                        IsDefault = design.IsDefault,
                        WebTemplate = WebTemplateIdFor(design.WebTemplate),
                    };

                    foreach (string script in design.SiteScripts)
                    {
                        info.SiteScriptIds.Add(parser.ParseString(script));
                    }

                    SiteScriptUtility.SiteDesignMetadata match = existing.FirstOrDefault(
                        d => string.Equals(d.Title, title, StringComparison.Ordinal));

                    Guid id;

                    if (match == null)
                    {
                        SiteScriptUtility.SiteDesignMetadata created = await SiteScriptUtility
                            .CreateSiteDesignAsync(admin, info).ConfigureAwait(false);

                        id = created.Id;
                    }
                    else
                    {
                        if (design.Overwrite)
                        {
                            await SiteScriptUtility.UpdateSiteDesignAsync(admin, match.Id, info).ConfigureAwait(false);
                        }

                        id = match.Id;
                    }

                    parser = ReplaceToken(parser, new SiteDesignIdToken(admin, title, id));

                    await ApplyGrantsAsync(admin, design, id, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The site design '{title}' could not be processed: {ErrorText.Describe(ex)}");
                }
            }

            return parser;
        }

        private async Task ApplyGrantsAsync(PnPContext admin, SiteDesignModel design, Guid id, TokenParser parser)
        {
            if (!(design.Grants?.Count > 0))
            {
                return;
            }

            foreach (IGrouping<SiteDesignRight, SiteDesignGrant> byRight in design.Grants.GroupBy(g => g.Right))
            {
                List<string> principals = byRight
                    .Select(g => parser.ParseString(g.Principal))
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

                if (principals.Count == 0)
                {
                    continue;
                }

                try
                {
                    await SiteScriptUtility.GrantSiteDesignRightsAsync(
                        admin, id, principals, (int)byRight.Key).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The '{byRight.Key}' rights on site design '{design.Title}' could not be " +
                        $"granted: {ErrorText.Describe(ex)}");
                }
            }
        }

        /// <summary>
        /// The numeric web template a site design applies to.
        /// </summary>
        private static string WebTemplateIdFor(SiteDesignWebTemplate template)
        {
            return ((int)template).ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Themes

        private async Task ApplyThemesAsync(PnPContext admin, ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.Themes?.Count > 0))
            {
                return;
            }

            HashSet<string> existing;

            try
            {
                existing = await TenantThemes.GetNamesAsync(admin).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(admin, $"The existing tenant themes could not be read, so none were processed: {ErrorText.Describe(ex)}");
                return;
            }

            foreach (ThemeModel theme in tenant.Themes)
            {
                string name = parser.ParseString(theme.Name);

                try
                {
                    if (existing.Contains(name))
                    {
                        if (!theme.Overwrite)
                        {
                            WriteMessage($"Skipped processing theme {name} as it already exists and Overwrite is set to false",
                                ProvisioningMessageType.Progress);
                            continue;
                        }

                        WriteMessage($"Overwriting existing theme {name}", ProvisioningMessageType.Progress);

                        await TenantThemes.UpdateAsync(admin, name, parser.ParseString(theme.Palette),
                            theme.IsInverted).ConfigureAwait(false);
                    }
                    else
                    {
                        WriteMessage($"Processing theme {name}", ProvisioningMessageType.Progress);

                        await TenantThemes.AddAsync(admin, name, parser.ParseString(theme.Palette),
                            theme.IsInverted).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The theme '{name}' could not be processed: {ErrorText.Describe(ex)}");
                }
            }
        }

        #endregion

        #region Sharing

        /// <summary>
        /// Writes the tenant's sharing configuration.
        /// </summary>
        private async Task ApplySharingSettingsAsync(PnPContext admin, ProvisioningTenant tenant)
        {
            SharingSettings wanted = tenant.SharingSettings;

            if (wanted == null)
            {
                return;
            }

            try
            {
                WriteMessage("Processing sharing settings", ProvisioningMessageType.Progress);

                ITenantProperties properties = await admin.GetSharePointAdmin()
                    .GetTenantPropertiesAsync().ConfigureAwait(false);

                properties.SharingCapability = Convert<SharingCapabilities>(wanted.SharingCapability);

                if (wanted.SharingCapability != Model.SharingCapability.Disabled)
                {
                    properties.RequireAnonymousLinksExpireInDays = wanted.RequireAnonymousLinksExpireInDays;
                    properties.FileAnonymousLinkType = Convert<PnP.Core.Admin.Model.SharePoint.AnonymousLinkType>(wanted.FileAnonymousLinkType);
                    properties.FolderAnonymousLinkType = Convert<PnP.Core.Admin.Model.SharePoint.AnonymousLinkType>(wanted.FolderAnonymousLinkType);
                    properties.DefaultSharingLinkType = Convert<PnP.Core.Admin.Model.SharePoint.SharingLinkType>(wanted.DefaultSharingLinkType);
                    properties.PreventExternalUsersFromResharing = wanted.PreventExternalUsersFromResharing;
                    properties.RequireAcceptingAccountMatchInvitedAccount = wanted.RequireAcceptingAccountMatchInvitedAccount;
                    properties.SharingDomainRestrictionMode = Convert<SharingDomainRestrictionModes>(wanted.SharingDomainRestrictionMode);

                    if (wanted.SharingDomainRestrictionMode == Model.SharingDomainRestrictionMode.AllowList)
                    {
                        properties.SharingAllowedDomainList = string.Join(" ", wanted.AllowedDomainList);
                    }
                    else if (wanted.SharingDomainRestrictionMode == Model.SharingDomainRestrictionMode.BlockList)
                    {
                        properties.SharingBlockedDomainList = string.Join(" ", wanted.BlockedDomainList);
                    }
                }

                await properties.UpdateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(admin, $"The sharing settings could not be applied: {ErrorText.Describe(ex)}");
            }
        }

        /// <summary>
        /// Converts between the template's enum and PnP Core's by member name.
        /// </summary>
        private static TTarget Convert<TTarget>(Enum source) where TTarget : struct, Enum
        {
            if (Enum.TryParse(source.ToString(), out TTarget target))
            {
                return target;
            }

            throw new NotSupportedException(
                $"'{source}' has no equivalent in {typeof(TTarget).Name}, so it cannot be applied.");
        }

        #endregion

        #region Content delivery network

        /// <summary>
        /// Applies the public and private CDN configuration.
        /// </summary>
        private async Task ApplyCdnAsync(PnPContext admin, ProvisioningTenant tenant, TokenParser parser)
        {
            ContentDeliveryNetwork cdn = tenant.ContentDeliveryNetwork;

            if (cdn == null)
            {
                return;
            }

            await ApplyCdnAsync(admin, cdn.PublicCdn, TenantCdnType.Public, parser).ConfigureAwait(false);
            await ApplyCdnAsync(admin, cdn.PrivateCdn, TenantCdnType.Private, parser).ConfigureAwait(false);
        }

        private async Task ApplyCdnAsync(PnPContext admin, CdnSettings settings, TenantCdnType type, TokenParser parser)
        {
            if (settings == null)
            {
                return;
            }

            try
            {
                WriteMessage($"Processing the {type} CDN", ProvisioningMessageType.Progress);

                CdnEnabledInfo current = await CsomRequestSender.SendAsync(admin,
                    new GetTenantCdnEnabledRequest(type)).ConfigureAwait(false);

                if (current.Enabled != settings.Enabled)
                {
                    await CsomRequestSender.SendAsync(admin,
                        new SetTenantCdnEnabledRequest(type, settings.Enabled)).ConfigureAwait(false);
                }

                if (!settings.Enabled)
                {
                    return;
                }

                if (!settings.NoDefaultOrigins)
                {
                    await CsomRequestSender.SendAsync(admin,
                        new TenantCdnOriginRequest(type, CdnOriginAction.CreateDefaults)).ConfigureAwait(false);
                }

                await ApplyCdnOriginsAsync(admin, settings, type, parser).ConfigureAwait(false);
                await ApplyCdnPoliciesAsync(admin, settings, type, parser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(admin, $"The {type} CDN could not be configured: {ErrorText.Describe(ex)}");
            }
        }

        private async Task ApplyCdnOriginsAsync(PnPContext admin, CdnSettings settings, TenantCdnType type, TokenParser parser)
        {
            if (!(settings.Origins?.Count > 0))
            {
                return;
            }

            List<string> existing = await CsomRequestSender.SendAsync(admin,
                GetTenantCdnStringsRequest.Origins(type)).ConfigureAwait(false);

            foreach (CdnOrigin origin in settings.Origins)
            {
                string url = parser.ParseString(origin.Url);
                bool present = existing.Contains(url, StringComparer.OrdinalIgnoreCase);

                try
                {
                    if (origin.Action == OriginAction.Add && !present)
                    {
                        await CsomRequestSender.SendAsync(admin,
                            new TenantCdnOriginRequest(type, CdnOriginAction.Add, url)).ConfigureAwait(false);
                    }
                    else if (origin.Action == OriginAction.Remove && present)
                    {
                        await CsomRequestSender.SendAsync(admin,
                            new TenantCdnOriginRequest(type, CdnOriginAction.Remove, url)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The {type} CDN origin '{url}' could not be " +
                        $"{origin.Action.ToString().ToLowerInvariant()}ed: {ErrorText.Describe(ex)}");
                }
            }
        }

        private async Task ApplyCdnPoliciesAsync(PnPContext admin, CdnSettings settings, TenantCdnType type, TokenParser parser)
        {
            var wanted = new Dictionary<TenantCdnPolicyType, string>();

            Add(wanted, TenantCdnPolicyType.IncludeFileExtensions, settings.IncludeFileExtensions, parser);
            Add(wanted, TenantCdnPolicyType.ExcludeRestrictedSiteClassifications,
                settings.ExcludeRestrictedSiteClassifications, parser);
            Add(wanted, TenantCdnPolicyType.ExcludeIfNoScriptDisabled, settings.ExcludeIfNoScriptDisabled, parser);

            if (wanted.Count == 0)
            {
                return;
            }

            Dictionary<TenantCdnPolicyType, string> current = TenantCdnPolicies.Parse(
                await CsomRequestSender.SendAsync(admin, GetTenantCdnStringsRequest.Policies(type))
                    .ConfigureAwait(false));

            foreach (KeyValuePair<TenantCdnPolicyType, string> policy in wanted)
            {
                if (current.TryGetValue(policy.Key, out string existing)
                    && string.Equals(existing, policy.Value, StringComparison.Ordinal))
                {
                    continue;
                }

                try
                {
                    await CsomRequestSender.SendAsync(admin,
                        new SetTenantCdnPolicyRequest(type, policy.Key, policy.Value)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(admin, $"The {type} CDN policy '{policy.Key}' could not be set: {ErrorText.Describe(ex)}");
                }
            }
        }

        /// <summary>
        /// Records a policy the template actually specified.
        /// </summary>
        private static void Add(Dictionary<TenantCdnPolicyType, string> wanted, TenantCdnPolicyType type,
            string value, TokenParser parser)
        {
            if (!string.IsNullOrEmpty(value))
            {
                wanted[type] = parser.ParseString(value);
            }
        }

        #endregion

        #region Storage entities

        /// <summary>
        /// Writes the template's storage entities to the tenant app catalog.
        /// </summary>
        private async Task<TokenParser> ApplyStorageEntitiesAsync(PnPContext context, ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.StorageEntities?.Count > 0))
            {
                return parser;
            }

            Uri catalogUri;

            try
            {
                catalogUri = await context.GetTenantAppManager().GetTenantAppCatalogUriAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                catalogUri = null;
                context.Logger?.LogDebug(ex, "{Source}: the tenant app catalog url could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            if (catalogUri == null)
            {
                Warn(context, "The tenant does not have an app catalog, so the storage entities were skipped.");
                return parser;
            }

            using (PnPContext catalog = await context.CloneAsync(catalogUri).ConfigureAwait(false))
            {
                foreach (StorageEntityModel entity in tenant.StorageEntities)
                {
                    string key = parser.ParseString(entity.Key);
                    string value = parser.ParseString(entity.Value);

                    try
                    {
                        WriteMessage($"Processing storage entity {key}", ProvisioningMessageType.Progress);

                        await StorageEntities.SetAsync(catalog, key, value,
                            parser.ParseString(entity.Description),
                            parser.ParseString(entity.Comment)).ConfigureAwait(false);

                        parser = ReplaceToken(parser, new StorageEntityValueToken(context, key, value));
                    }
                    catch (Exception ex)
                    {
                        Warn(context, $"The storage entity '{key}' could not be written: " +
                            $"{ErrorText.Describe(ex)}{await NoScriptHintAsync(catalog, ex).ConfigureAwait(false)}");
                    }
                }
            }

            return parser;
        }

        #endregion

        #region User profiles

        /// <summary>
        /// Writes single-valued user profile properties.
        /// </summary>
        private async Task ApplyUserProfilesAsync(PnPContext admin, ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.SPUsersProfiles?.Count > 0))
            {
                return;
            }

            WriteMessage("Processing user profiles", ProvisioningMessageType.Progress);

            foreach (UserProfile profile in tenant.SPUsersProfiles)
            {
                string target = parser.ParseString(
                    string.IsNullOrEmpty(profile.TargetUser) ? profile.TargetGroup : profile.TargetUser);

                if (string.IsNullOrWhiteSpace(target))
                {
                    Warn(admin, "A user profile entry names neither a user nor a group, so it was skipped.");
                    continue;
                }

                string accountName = $"i:0#.f|membership|{target}";

                foreach (KeyValuePair<string, string> property in profile.Properties)
                {
                    try
                    {
                        string body = JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["accountName"] = accountName,
                            ["propertyName"] = property.Key,
                            ["propertyValue"] = parser.ParseString(property.Value),
                        });

                        await admin.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                            "_api/SP.UserProfiles.PeopleManager/SetSingleValueProfileProperty", body))
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Warn(admin, $"The profile property '{property.Key}' for '{target}' could not be " +
                            $"set: {ErrorText.Describe(ex)}");
                    }
                }
            }
        }

        #endregion

        #region Web API permissions

        /// <summary>
        /// Approves the pending API permission requests the template names.
        /// </summary>
        private async Task ApplyWebApiPermissionsAsync(PnPContext context, ProvisioningTenant tenant, TokenParser parser)
        {
            if (!(tenant.WebApiPermissions?.Count > 0))
            {
                return;
            }

            Uri catalogUri;

            try
            {
                catalogUri = await context.GetTenantAppManager().GetTenantAppCatalogUriAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                catalogUri = null;
                context.Logger?.LogDebug(ex, "{Source}: the tenant app catalog url could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            if (catalogUri == null)
            {
                Warn(context, "The tenant does not have an app catalog, so the web API permissions were skipped.");
                return;
            }

            WriteMessage("Processing web API permissions", ProvisioningMessageType.Progress);

            using (PnPContext catalog = await context.CloneAsync(catalogUri).ConfigureAwait(false))
            {
                List<PermissionEntry> pending;
                List<PermissionEntry> granted;

                try
                {
                    pending = await ReadServicePrincipalAsync(catalog, "permissionrequests").ConfigureAwait(false);
                    granted = await ReadServicePrincipalAsync(catalog, "permissiongrants").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"The service principal's permissions could not be read, so none were " +
                        $"approved: {ErrorText.Describe(ex)}");
                    return;
                }

                foreach (WebApiPermission permission in tenant.WebApiPermissions)
                {
                    string scope = parser.ParseString(permission.Scope);
                    string resource = parser.ParseString(permission.Resource);

                    if (granted.Any(g => Matches(g, resource, scope)))
                    {
                        continue;
                    }

                    List<PermissionEntry> matching = pending.Where(r => Matches(r, resource, scope)).ToList();

                    if (matching.Count == 0)
                    {
                        Warn(context, $"No pending permission request matches '{scope}' on '{resource}', " +
                            "so there was nothing to approve. A request is raised by deploying the package " +
                            "that asks for it.");
                        continue;
                    }

                    foreach (PermissionEntry request in matching)
                    {
                        try
                        {
                            await catalog.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                                $"_api/web/tenantappcatalog/ServicePrincipal/permissionrequests('{request.Id}')/Approve",
                                "{}")).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Warn(context, $"The permission request for '{scope}' on '{resource}' could not " +
                                $"be approved: {ErrorText.Describe(ex)}");
                        }

                        pending.Remove(request);
                    }
                }
            }
        }

        private static bool Matches(PermissionEntry entry, string resource, string scope)
        {
            return string.Equals(entry.Resource, resource, StringComparison.OrdinalIgnoreCase)
                && entry.Scope != null
                && entry.Scope.IndexOf(scope, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static async Task<List<PermissionEntry>> ReadServicePrincipalAsync(PnPContext catalog, string collection)
        {
            ApiRequestResponse response = await catalog.Web.ExecuteRequestAsync(new ApiRequest(
                ApiRequestType.SPORest, $"_api/web/tenantappcatalog/ServicePrincipal/{collection}"))
                .ConfigureAwait(false);

            var entries = new List<PermissionEntry>();

            if (string.IsNullOrEmpty(response.Response))
            {
                return entries;
            }

            using (JsonDocument document = JsonDocument.Parse(response.Response))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    return entries;
                }

                foreach (JsonElement item in value.EnumerateArray())
                {
                    entries.Add(new PermissionEntry
                    {
                        Id = item.TryGetProperty("id", out JsonElement id) ? id.GetString() : null,
                        Resource = item.TryGetProperty("Resource", out JsonElement resource) ? resource.GetString() : null,
                        Scope = item.TryGetProperty("Scope", out JsonElement scope) ? scope.GetString() : null,
                    });
                }
            }

            return entries;
        }

        private sealed class PermissionEntry
        {
            internal string Id { get; set; }

            internal string Resource { get; set; }

            internal string Scope { get; set; }
        }

        #endregion

        #region Microsoft 365 group settings

        /// <summary>The directory setting template every Microsoft 365 group setting belongs to.</summary>
        private const string UnifiedGroupTemplate = "Group.Unified";

        /// <summary>
        /// Applies the tenant's Microsoft 365 group settings, over Graph.
        /// </summary>
        private async Task ApplyGroupSettingsAsync(PnPContext admin, ProvisioningTenant tenant, TokenParser parser)
        {
            Dictionary<string, string> wanted = tenant.Office365GroupsSettings?.Properties;

            if (!(wanted?.Count > 0))
            {
                return;
            }

            try
            {
                WriteMessage("Processing Microsoft 365 group settings", ProvisioningMessageType.Progress);

                var values = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> property in wanted)
                {
                    values[property.Key] = parser.ParseString(property.Value);
                }

                (string settingId, string templateId, Dictionary<string, string> current) =
                    await ReadGroupSettingsAsync(admin).ConfigureAwait(false);

                if (templateId == null)
                {
                    Warn(admin, $"The '{UnifiedGroupTemplate}' directory setting template is not available " +
                        "on this tenant, so the group settings were skipped.");
                    return;
                }

                foreach (KeyValuePair<string, string> value in values)
                {
                    current[value.Key] = value.Value;
                }

                string body = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["templateId"] = templateId,
                    ["values"] = current.Select(v => new Dictionary<string, string>
                    {
                        ["name"] = v.Key,
                        ["value"] = v.Value,
                    }).ToList(),
                });

                if (settingId == null)
                {
                    await admin.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        "groupSettings", body)).ConfigureAwait(false);
                }
                else
                {
                    await admin.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"), ApiRequestType.Graph,
                        $"groupSettings/{settingId}", body)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Warn(admin, $"The Microsoft 365 group settings could not be applied: {ErrorText.Describe(ex)}");
            }
        }

        /// <summary>
        /// Finds the tenant's <c>Group.Unified</c> setting, falling back to its template.
        /// </summary>
        /// <returns>
        /// The existing setting's id or null, the template id, and the values currently in force.
        /// </returns>
        private static async Task<(string SettingId, string TemplateId, Dictionary<string, string> Values)>
            ReadGroupSettingsAsync(PnPContext admin)
        {
            ApiRequestResponse settings = await admin.Web.ExecuteRequestAsync(
                new ApiRequest(ApiRequestType.Graph, "groupSettings")).ConfigureAwait(false);

            foreach (JsonElement setting in ItemsOf(settings.Response))
            {
                if (NameOf(setting) == UnifiedGroupTemplate)
                {
                    return (setting.GetProperty("id").GetString(),
                        setting.GetProperty("templateId").GetString(),
                        ValuesOf(setting));
                }
            }

            ApiRequestResponse templates = await admin.Web.ExecuteRequestAsync(
                new ApiRequest(ApiRequestType.Graph, "groupSettingTemplates")).ConfigureAwait(false);

            foreach (JsonElement template in ItemsOf(templates.Response))
            {
                if (NameOf(template) == UnifiedGroupTemplate)
                {
                    return (null, template.GetProperty("id").GetString(), DefaultsOf(template));
                }
            }

            return (null, null, new Dictionary<string, string>());
        }

        private static IEnumerable<JsonElement> ItemsOf(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                yield break;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    yield break;
                }

                foreach (JsonElement item in value.EnumerateArray())
                {
                    yield return item.Clone();
                }
            }
        }

        private static string NameOf(JsonElement element)
        {
            return element.TryGetProperty("displayName", out JsonElement name) ? name.GetString() : null;
        }

        private static Dictionary<string, string> ValuesOf(JsonElement setting)
        {
            return ReadPairs(setting, "values", "value");
        }

        private static Dictionary<string, string> DefaultsOf(JsonElement template)
        {
            return ReadPairs(template, "values", "defaultValue");
        }

        private static Dictionary<string, string> ReadPairs(JsonElement element, string collection, string valueName)
        {
            var pairs = new Dictionary<string, string>(StringComparer.Ordinal);

            if (!element.TryGetProperty(collection, out JsonElement values) || values.ValueKind != JsonValueKind.Array)
            {
                return pairs;
            }

            foreach (JsonElement item in values.EnumerateArray())
            {
                if (item.TryGetProperty("name", out JsonElement name) && name.ValueKind == JsonValueKind.String)
                {
                    pairs[name.GetString()] = item.TryGetProperty(valueName, out JsonElement value)
                        && value.ValueKind == JsonValueKind.String
                        ? value.GetString()
                        : string.Empty;
                }
            }

            return pairs;
        }

        /// <summary>
        /// Explains an access-denied on the app catalog, when NoScript is what caused it.
        /// </summary>
        private static async Task<string> NoScriptHintAsync(PnPContext catalog, Exception ex)
        {
            if (!(ex is SharePointRestServiceException restException)
                || !(restException.Error is SharePointRestError error)
                || error.HttpResponseCode != 403)
            {
                return string.Empty;
            }

            try
            {
                if (!await catalog.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return Environment.NewLine +
                "The app catalog is a NoScript site, and a storage entity is a property bag entry on " +
                "it - which NoScript blocks. Either allow scripting on the app catalog site, or turn " +
                "on the tenant setting AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled.";
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Publishes a token, replacing any earlier one for the same name.
        /// </summary>
        private static TokenParser ReplaceToken(TokenParser parser, TokenDefinition token)
        {
            List<TokenDefinition> stale = parser.Tokens
                .Where(t => t.GetTokens().Intersect(token.GetTokens(), StringComparer.OrdinalIgnoreCase).Any())
                .ToList();

            foreach (TokenDefinition existing in stale)
            {
                parser.Tokens.Remove(existing);
            }

            parser.AddToken(token);

            return parser;
        }

        private void Warn(PnPContext context, string message)
        {
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        #endregion
    }
}
