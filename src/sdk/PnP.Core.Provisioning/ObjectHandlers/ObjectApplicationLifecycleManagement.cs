using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppModel = PnP.Core.Provisioning.Model.App;
using PackageModel = PnP.Core.Provisioning.Model.Package;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:ApplicationLifecycleManagement&gt;</c> element - the
    /// SPFx packages an app catalog holds, and the apps installed in the site.
    /// </summary>
    internal class ObjectApplicationLifecycleManagement : ObjectHandlerBase
    {
        /// <summary>
        /// How long a synchronous app operation is given to reach the state it asked for.
        /// </summary>
        private static readonly TimeSpan SyncTimeout = TimeSpan.FromMinutes(5);

        private static readonly TimeSpan SyncPollInterval = TimeSpan.FromSeconds(5);

        public override string Name => "Application Lifecycle Management";

        public override string InternalName => "ApplicationLifecycleManagement";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.ApplicationLifecycleManagement != null
                && (template.ApplicationLifecycleManagement.Apps.Count > 0
                    || (template.ApplicationLifecycleManagement.AppCatalog?.Packages?.Count ?? 0) > 0);

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            Model.ApplicationLifecycleManagement alm = template.ApplicationLifecycleManagement;

            if (alm == null)
            {
                return parser;
            }

            if (await IsSubSiteAsync(context).ConfigureAwait(false))
            {
                context.Logger?.LogInformation(
                    "{Source}: this is a subsite, so the application lifecycle settings were skipped.",
                    Constants.LOGGING_SOURCE);
                return parser;
            }

            await ApplyPackagesAsync(context, template, alm, parser).ConfigureAwait(false);
            await ApplyAppsAsync(context, alm, parser).ConfigureAwait(false);

            return parser;
        }

        /// <summary>
        /// Puts the template's packages into the site collection's own app catalog.
        /// </summary>
        private async Task ApplyPackagesAsync(PnPContext context, ProvisioningTemplate template,
            Model.ApplicationLifecycleManagement alm, TokenParser parser)
        {
            List<PackageModel> packages = alm.AppCatalog?.Packages?.ToList();

            if (packages == null || packages.Count == 0)
            {
                return;
            }

            ISiteCollectionAppManager manager;

            try
            {
                await context.GetTenantAppManager()
                    .EnsureSiteCollectionAppCatalogAsync(context.Uri).ConfigureAwait(false);

                manager = context.GetSiteCollectionAppManager();
            }
            catch (Exception ex)
            {
                string warning = "The site collection app catalog could not be prepared, so the " +
                    $"packages were skipped: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            foreach (PackageModel package in packages)
            {
                try
                {
                    await ApplyPackageAsync(context, template, manager, package, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The package '{package.PackageId ?? package.Src}' could not be " +
                        $"{package.Action.ToString().ToLowerInvariant()}ed: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        private async Task ApplyPackageAsync(PnPContext context, ProvisioningTemplate template,
            ISiteCollectionAppManager manager, PackageModel package, TokenParser parser)
        {
            if (package.Action == PackageAction.Remove)
            {
                if (!TryParseId(parser.ParseString(package.PackageId), out Guid toRemove))
                {
                    WriteMessage($"A package marked for removal has no usable PackageId, so it was skipped.",
                        ProvisioningMessageType.Warning);
                    return;
                }

                await manager.RemoveAsync(toRemove).ConfigureAwait(false);
                return;
            }

            Guid appId = Guid.Empty;

            if (package.Action == PackageAction.Upload || package.Action == PackageAction.UploadAndPublish)
            {
                string source = parser.ParseString(package.Src);
                byte[] bytes = TemplateFileUtilities.TryGetFileBytes(template, source);

                if (bytes == null)
                {
                    WriteMessage($"The package file '{source}' is not in the template, so it was skipped.",
                        ProvisioningMessageType.Warning);
                    return;
                }

                ISiteCollectionApp uploaded = await manager.AddAsync(
                    bytes, NameOf(source), package.Overwrite).ConfigureAwait(false);

                appId = uploaded?.Id ?? Guid.Empty;
            }

            if (package.Action == PackageAction.Publish || package.Action == PackageAction.UploadAndPublish)
            {
                if (appId == Guid.Empty && !TryParseId(parser.ParseString(package.PackageId), out appId))
                {
                    WriteMessage("A package marked for publishing has no usable PackageId, so it was skipped.",
                        ProvisioningMessageType.Warning);
                    return;
                }

                await manager.DeployAsync(appId, package.SkipFeatureDeployment).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Installs, upgrades or uninstalls the template's apps in this site.
        /// </summary>
        private async Task ApplyAppsAsync(PnPContext context, Model.ApplicationLifecycleManagement alm, TokenParser parser)
        {
            if (alm.Apps == null || alm.Apps.Count == 0)
            {
                return;
            }

            ITenantAppManager manager = context.GetTenantAppManager();

            Uri appCatalog;

            try
            {
                appCatalog = await manager.GetTenantAppCatalogUriAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                appCatalog = null;
                context.Logger?.LogDebug(ex, "{Source}: the tenant app catalog url could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            if (appCatalog == null)
            {
                string warning = "The tenant does not have an app catalog, so the apps were skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            List<ITenantApp> installed = await InstalledAppsAsync(context, manager).ConfigureAwait(false);

            foreach (AppModel app in alm.Apps)
            {
                if (!TryParseId(parser.ParseString(app.AppId), out Guid appId))
                {
                    WriteMessage($"'{app.AppId}' is not a usable app id, so it was skipped.",
                        ProvisioningMessageType.Warning);
                    continue;
                }

                try
                {
                    bool changed = await ApplyAppAsync(context, manager, app, appId,
                        installed.Any(a => a.Id == appId)).ConfigureAwait(false);

                    if (changed && app.SyncMode == SyncMode.Synchronously)
                    {
                        await WaitForAppAsync(context, manager, appId, app.Action).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The app {appId} could not be " +
                        $"{app.Action.ToString().ToLowerInvariant()}ed: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Carries out one app action, reporting the no-ops rather than performing them silently.
        /// </summary>
        /// <returns>Whether anything was actually asked of SharePoint.</returns>
        private async Task<bool> ApplyAppAsync(PnPContext context, ITenantAppManager manager,
            AppModel app, Guid appId, bool alreadyInstalled)
        {
            switch (app.Action)
            {
                case AppAction.Install when !alreadyInstalled:
                    await manager.InstallAsync(appId).ConfigureAwait(false);
                    return true;

                case AppAction.Install:
                    WriteMessage($"App with ID {appId} already exists in the target site and will be skipped",
                        ProvisioningMessageType.Warning);
                    return false;

                case AppAction.Uninstall when alreadyInstalled:
                    await manager.UninstallAsync(appId).ConfigureAwait(false);
                    return true;

                case AppAction.Uninstall:
                    WriteMessage($"App with ID {appId} does not exist in the target site and cannot be uninstalled",
                        ProvisioningMessageType.Warning);
                    return false;

                case AppAction.Update when alreadyInstalled:
                    await manager.UpgradeAsync(appId).ConfigureAwait(false);
                    return true;

                case AppAction.Update:
                    WriteMessage($"App with ID {appId} does not exist in the target site and cannot be updated",
                        ProvisioningMessageType.Warning);
                    return false;

                case AppAction.InstallOrUpdate when alreadyInstalled:
                    await manager.UpgradeAsync(appId).ConfigureAwait(false);
                    return true;

                case AppAction.InstallOrUpdate:
                    await manager.InstallAsync(appId).ConfigureAwait(false);
                    return true;

                default:
                    context.Logger?.LogDebug("{Source}: app action {Action} is not handled.",
                        Constants.LOGGING_SOURCE, app.Action);
                    return false;
            }
        }

        /// <summary>
        /// Waits for an app to reach the state the action asked for.
        /// </summary>
        private static async Task WaitForAppAsync(PnPContext context, ITenantAppManager manager,
            Guid appId, AppAction action)
        {
            DateTime deadline = DateTime.UtcNow.Add(SyncTimeout);

            while (true)
            {
                ITenantApp app = await manager.GetAvailableAsync(appId).ConfigureAwait(false);

                bool settled = action == AppAction.Uninstall
                    ? app?.InstalledVersion == null
                    : app != null && app.InstalledVersion != null
                        && app.InstalledVersion.Equals(app.AppCatalogVersion);

                if (settled)
                {
                    return;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException(
                        $"App {appId} did not reach the state requested by {action} within " +
                        $"{SyncTimeout.TotalMinutes} minutes.");
                }

                await Task.Delay(SyncPollInterval).ConfigureAwait(false);
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            if (await IsSubSiteAsync(context).ConfigureAwait(false))
            {
                return template;
            }

            ITenantAppManager manager = context.GetTenantAppManager();

            foreach (ITenantApp app in await InstalledAppsAsync(context, manager).ConfigureAwait(false))
            {
                template.ApplicationLifecycleManagement.Apps.Add(new AppModel
                {
                    AppId = app.Id.ToString(),
                    Action = AppAction.Install,
                });
            }

            return template;
        }

        /// <summary>
        /// The apps from the tenant catalog that are installed in this site.
        /// </summary>
        private static async Task<List<ITenantApp>> InstalledAppsAsync(PnPContext context, ITenantAppManager manager)
        {
            try
            {
                IList<ITenantApp> available = await manager.GetAvailableAsync().ConfigureAwait(false);

                return available?.Where(a => a.InstalledVersion != null).ToList() ?? new List<ITenantApp>();
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the installed apps could not be read.",
                    Constants.LOGGING_SOURCE);
                return new List<ITenantApp>();
            }
        }

        #endregion

        #region Helpers

        private static bool TryParseId(string value, out Guid id)
        {
            return Guid.TryParse(value, out id) && id != Guid.Empty;
        }

        private static string NameOf(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            int lastSeparator = path.Replace('\\', '/').LastIndexOf('/');
            return lastSeparator < 0 ? path : path.Substring(lastSeparator + 1);
        }

        #endregion
    }
}
