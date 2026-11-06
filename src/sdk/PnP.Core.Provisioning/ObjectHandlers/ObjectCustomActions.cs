using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CorePermissions = PnP.Core.Model.SharePoint.BasePermissions;
using CustomActionModel = PnP.Core.Provisioning.Model.CustomAction;
using TemplatePermissions = PnP.Core.Provisioning.Model.BasePermissions;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts user custom actions at both site collection and web scope.
    /// </summary>
    internal class ObjectCustomActions : ObjectHandlerBase
    {
        public override string Name => "Custom Actions";

        public override string InternalName => "CustomActions";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.CustomActions != null
                && (template.CustomActions.SiteCustomActions.Any() || template.CustomActions.WebCustomActions.Any());

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (template.CustomActions == null)
                {
                    return parser;
                }

                IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.UserCustomActions).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl, s => s.UserCustomActions).ConfigureAwait(false);

                bool isNoScriptSite = await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);

                if (!IsSubSite(web))
                {
                    await ApplyScopeAsync(context, context.Site.UserCustomActions,
                        template.CustomActions.SiteCustomActions, parser, isNoScriptSite, siteScoped: true).ConfigureAwait(false);
                }

                await ApplyScopeAsync(context, context.Web.UserCustomActions,
                    template.CustomActions.WebCustomActions, parser, isNoScriptSite, siteScoped: false).ConfigureAwait(false);

                return parser;
            }
        }

        private async Task ApplyScopeAsync(PnPContext context, IUserCustomActionCollection collection,
            IEnumerable<CustomActionModel> customActions, TokenParser parser, bool isNoScriptSite, bool siteScoped)
        {
            List<CustomActionModel> wanted = customActions?.ToList() ?? new List<CustomActionModel>();
            if (wanted.Count == 0)
            {
                return;
            }

            string scope = siteScoped ? "site" : "web";

            List<IUserCustomAction> existingActions = collection.AsRequested().ToList();

            foreach (CustomActionModel customAction in wanted)
            {
                IUserCustomAction existing = existingActions
                    .FirstOrDefault(a => string.Equals(a.Name, customAction.Name, StringComparison.Ordinal));

                if (existing != null && customAction.Remove)
                {
                    await existing.DeleteAsync().ConfigureAwait(false);
                    existingActions.Remove(existing);
                    continue;
                }

                if (isNoScriptSite && customAction.ClientSideComponentId == Guid.Empty)
                {
                    string message = $"This is a NoScript site, so the custom action '{customAction.Name}' was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    continue;
                }

                IUserCustomAction target;

                if (existing == null)
                {
                    if (customAction.Remove || !customAction.Enabled)
                    {
                        continue;
                    }

                    context.Logger?.LogDebug("{Source}: adding {Scope} scoped custom action '{Name}'.",
                        Constants.LOGGING_SOURCE, scope, customAction.Name);

                    target = await collection.AddAsync(BuildOptions(customAction, parser)).ConfigureAwait(false);
                    existingActions.Add(target);
                }
                else
                {
                    await UpdateAsync(context, existing, customAction, parser).ConfigureAwait(false);
                    target = existing;
                }

                await LocalizeAsync(context, target, customAction, parser, siteScoped).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the per-language title and description, when the template uses <c>{res:}</c> tokens.
        /// </summary>
        private async Task LocalizeAsync(PnPContext context, IUserCustomAction customActionOnSite,
            CustomActionModel customAction, TokenParser parser, bool siteScoped)
        {
            bool localizesTitle = UserResources.ContainsResourceToken(customAction.Title);
            bool localizesDescription = UserResources.ContainsResourceToken(customAction.Description);

            if (!localizesTitle && !localizesDescription)
            {
                return;
            }

            await customActionOnSite.LoadAsync(a => a.Id).ConfigureAwait(false);
            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            UserResourcePath PathFor(string property) => siteScoped
                ? UserResourcePath.ForSiteUserCustomAction(siteId, webId, customActionOnSite.Id, property)
                : UserResourcePath.ForUserCustomAction(siteId, webId, customActionOnSite.Id, property);

            if (localizesTitle)
            {
                await UserResources.TrySetAsync(context, PathFor(ResourceProperty.Title), customAction.Title, parser,
                    $"the title of custom action '{customAction.Name}'", m => WriteMessage(m, ProvisioningMessageType.Warning))
                    .ConfigureAwait(false);
            }

            if (localizesDescription)
            {
                await UserResources.TrySetAsync(context, PathFor(ResourceProperty.Description), customAction.Description, parser,
                    $"the description of custom action '{customAction.Name}'", m => WriteMessage(m, ProvisioningMessageType.Warning))
                    .ConfigureAwait(false);
            }
        }

        internal static AddUserCustomActionOptions BuildOptions(CustomActionModel customAction, TokenParser parser)
        {
            return new AddUserCustomActionOptions
            {
                ClientSideComponentId = customAction.ClientSideComponentId != Guid.Empty ? customAction.ClientSideComponentId : null,
                ClientSideComponentProperties = parser.ParseString(customAction.ClientSideComponentProperties),
                CommandUIExtension = customAction.CommandUIExtension != null
                    ? parser.ParseString(customAction.CommandUIExtension.ToString())
                    : string.Empty,
                Description = parser.ParseString(customAction.Description),
                Group = customAction.Group,
                ImageUrl = parser.ParseString(customAction.ImageUrl),
                Location = customAction.Location,
                Name = customAction.Name,
                RegistrationId = parser.ParseString(customAction.RegistrationId),
                RegistrationType = customAction.RegistrationType,
                Rights = ToCorePermissions(customAction.Rights),
                ScriptBlock = parser.ParseString(customAction.ScriptBlock),
                ScriptSrc = parser.ParseString(customAction.ScriptSrc),
                Sequence = customAction.Sequence,
                Title = parser.ParseString(customAction.Title),
                Url = parser.ParseString(customAction.Url),
            };
        }

        /// <summary>
        /// Brings an existing custom action in line with the template, updating only when something
        /// actually differs.
        /// </summary>
        internal static async Task UpdateAsync(PnPContext context, IUserCustomAction existing, CustomActionModel customAction, TokenParser parser)
        {
            bool dirty = false;

            string commandUIExtension = customAction.CommandUIExtension != null
                ? parser.ParseString(customAction.CommandUIExtension.ToString())
                : null;

            dirty |= SetIfChanged(existing.CommandUIExtension, commandUIExtension, v => existing.CommandUIExtension = v);

            if (customAction.ClientSideComponentId != Guid.Empty)
            {
                dirty |= SetIfChanged(existing.ClientSideComponentId, customAction.ClientSideComponentId, v => existing.ClientSideComponentId = v);
            }

            if (!string.IsNullOrEmpty(customAction.ClientSideComponentProperties))
            {
                dirty |= SetIfChanged(existing.ClientSideComponentProperties,
                    parser.ParseString(customAction.ClientSideComponentProperties), v => existing.ClientSideComponentProperties = v);
            }

            dirty |= SetIfChanged(existing.Description, customAction.Description, v => existing.Description = v);
            dirty |= SetIfChanged(existing.Group, customAction.Group, v => existing.Group = v);
            dirty |= SetIfChanged(existing.ImageUrl, parser.ParseString(customAction.ImageUrl), v => existing.ImageUrl = v);
            dirty |= SetIfChanged(existing.Location, customAction.Location, v => existing.Location = v);
            dirty |= SetIfChanged(existing.RegistrationId, parser.ParseString(customAction.RegistrationId), v => existing.RegistrationId = v);
            dirty |= SetIfChanged(existing.RegistrationType, customAction.RegistrationType, v => existing.RegistrationType = v);
            dirty |= SetIfChanged(existing.ScriptBlock, parser.ParseString(customAction.ScriptBlock), v => existing.ScriptBlock = v);
            dirty |= SetIfChanged(existing.ScriptSrc, parser.ParseString(customAction.ScriptSrc), v => existing.ScriptSrc = v);
            dirty |= SetIfChanged(existing.Sequence, customAction.Sequence, v => existing.Sequence = v);
            dirty |= SetIfChanged(existing.Title, parser.ParseString(customAction.Title), v => existing.Title = v);
            dirty |= SetIfChanged(existing.Url, parser.ParseString(customAction.Url), v => existing.Url = v);

            if (dirty)
            {
                context.Logger?.LogDebug("{Source}: updating custom action '{Name}'.", Constants.LOGGING_SOURCE, customAction.Name);
                await existing.UpdateAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.UserCustomActions).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl, s => s.UserCustomActions).ConfigureAwait(false);

                bool isSubSite = IsSubSite(web);

                var customActions = new CustomActions();
                ProvisioningTemplateCreationInformation creationInformation = configuration?.ToCreationInformation();

                foreach (IUserCustomAction customAction in web.UserCustomActions.AsRequested())
                {
                    CustomActionModel copied = Copy(customAction);
                    await PersistResourcesAsync(context, customAction, copied, template, creationInformation, siteScoped: false).ConfigureAwait(false);
                    customActions.WebCustomActions.Add(copied);
                }

                if (!isSubSite)
                {
                    foreach (IUserCustomAction customAction in context.Site.UserCustomActions.AsRequested())
                    {
                        CustomActionModel copied = Copy(customAction);
                        await PersistResourcesAsync(context, customAction, copied, template, creationInformation, siteScoped: true).ConfigureAwait(false);
                        customActions.SiteCustomActions.Add(copied);
                    }
                }

                template.CustomActions = customActions;

                ProvisioningTemplate baseTemplate = creationInformation?.BaseTemplate;
                if (baseTemplate != null)
                {
                    RemoveBaseTemplateEntries(context, template, baseTemplate, isSubSite);
                }

                return template;
            }
        }

        /// <summary>
        /// Drops the actions the target site template already provides, so the extracted template
        /// carries only what a person actually added.
        /// </summary>
        private static void RemoveBaseTemplateEntries(PnPContext context, ProvisioningTemplate template, ProvisioningTemplate baseTemplate, bool isSubSite)
        {
            if (!isSubSite)
            {
                RemoveMatching(context, template.CustomActions.SiteCustomActions, baseTemplate.CustomActions.SiteCustomActions, "site");
            }

            RemoveMatching(context, template.CustomActions.WebCustomActions, baseTemplate.CustomActions.WebCustomActions, "web");
        }

        private static void RemoveMatching(PnPContext context, CustomActionCollection target, CustomActionCollection baseline, string scope)
        {
            foreach (CustomActionModel customAction in baseline)
            {
                int index = target.FindIndex(c => c.Name.Equals(customAction.Name, StringComparison.Ordinal));
                if (index < 0)
                {
                    continue;
                }

                context.Logger?.LogDebug("{Source}: dropping {Scope} scoped custom action '{Name}' - it is part of the base template.",
                    Constants.LOGGING_SOURCE, scope, customAction.Name);

                target.RemoveAt(index);
            }
        }

        private static CustomActionModel Copy(IUserCustomAction userCustomAction)
        {
            return new CustomActionModel
            {
                Description = userCustomAction.Description,
                Enabled = true,
                Group = userCustomAction.Group,
                ImageUrl = userCustomAction.ImageUrl,
                Location = userCustomAction.Location,
                Name = userCustomAction.Name,
                Rights = ToTemplatePermissions(userCustomAction.Rights),
                ScriptBlock = userCustomAction.ScriptBlock,
                ScriptSrc = userCustomAction.ScriptSrc,
                Sequence = userCustomAction.Sequence,
                Title = userCustomAction.Title,
                Url = userCustomAction.Url,
                RegistrationId = userCustomAction.RegistrationId,
                RegistrationType = userCustomAction.RegistrationType,
                ClientSideComponentId = userCustomAction.ClientSideComponentId,
                ClientSideComponentProperties = userCustomAction.ClientSideComponentProperties,
                CommandUIExtension = !string.IsNullOrEmpty(userCustomAction.CommandUIExtension)
                    ? XElement.Parse(userCustomAction.CommandUIExtension)
                    : null,
            };

        }

        /// <summary>
        /// Reads a custom action's title and description in every supported language, records them
        /// against a token, and replaces the literal value with that token.
        /// </summary>
        private static async Task PersistResourcesAsync(PnPContext context, IUserCustomAction customAction,
            CustomActionModel copied, ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInformation, bool siteScoped)
        {
            if (creationInformation?.PersistMultiLanguageResources != true || template.SupportedUILanguages.Count == 0)
            {
                return;
            }

            string key = customAction.Name.Replace(" ", "_");
            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            UserResourcePath PathFor(string property) => siteScoped
                ? UserResourcePath.ForSiteUserCustomAction(siteId, webId, customAction.Id, property)
                : UserResourcePath.ForUserCustomAction(siteId, webId, customAction.Id, property);

            string titleToken = $"CustomAction_{key}_Title";
            if (await UserResources.PersistAsync(context, PathFor(ResourceProperty.Title), titleToken, template, creationInformation).ConfigureAwait(false))
            {
                copied.Title = UserResources.TokenFor(titleToken);
            }

            string descriptionToken = $"CustomAction_{key}_Description";
            if (!string.IsNullOrWhiteSpace(customAction.Description)
                && await UserResources.PersistAsync(context, PathFor(ResourceProperty.Description), descriptionToken, template, creationInformation).ConfigureAwait(false))
            {
                copied.Description = UserResources.TokenFor(descriptionToken);
            }
        }

        #endregion

        #region Helpers

        private static bool SetIfChanged<T>(T current, T wanted, Action<T> set)
        {
            if (EqualityComparer<T>.Default.Equals(current, wanted))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        /// <summary>
        /// Converts the template's plain permission mask into the live model PnP Core expects.
        /// </summary>
        private static IBasePermissions ToCorePermissions(TemplatePermissions permissions)
        {
            var rights = new CorePermissions();

            if (permissions != null)
            {
                rights.Low = permissions.Low;
                rights.High = permissions.High;
            }

            return rights;
        }

        private static TemplatePermissions ToTemplatePermissions(IBasePermissions permissions)
        {
            if (permissions == null)
            {
                return new TemplatePermissions();
            }

            return new TemplatePermissions
            {
                Low = (uint)permissions.Low,
                High = (uint)permissions.High,
            };
        }

        #endregion
    }
}
