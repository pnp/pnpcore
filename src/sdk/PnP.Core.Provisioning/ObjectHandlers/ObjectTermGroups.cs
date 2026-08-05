using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TermGroupModel = PnP.Core.Provisioning.Model.TermGroup;
using TermLabelModel = PnP.Core.Provisioning.Model.TermLabel;
using TermModel = PnP.Core.Provisioning.Model.Term;
using TermSetModel = PnP.Core.Provisioning.Model.TermSet;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts the <c>&lt;pnp:TermGroups&gt;</c> element - managed metadata groups,
    /// their term sets, and the terms inside them.
    /// </summary>
    internal class ObjectTermGroups : ObjectHandlerBase
    {
        public override string Name => "Term groups";

        public override string InternalName => "TermGroups";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.TermGroups.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // Only when asked. The term store is tenant-wide, so sweeping it into every extract
            // would produce a template carrying groups that have nothing to do with the site.
            _willExtract ??= configuration?.Taxonomy != null
                && (configuration.Taxonomy.IncludeSiteCollectionTermGroup
                    || configuration.Taxonomy.IncludeAllTermGroups);

            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!template.TermGroups.Any())
            {
                return parser;
            }

            var provisioner = new TermGroupProvisioner(context, parser, m => WriteMessage(m, ProvisioningMessageType.Warning));

            int index = 0;

            foreach (TermGroupModel modelGroup in template.TermGroups)
            {
                index++;
                WriteSubProgress("Term group", modelGroup.Name, index, template.TermGroups.Count);

                try
                {
                    await provisioner.ProcessGroupAsync(modelGroup).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The term group '{modelGroup.Name}' could not be provisioned: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            // Last, once every group in the template exists: a reused term's source may be declared
            // after the reuse itself.
            await provisioner.ApplyDeferredReusesAsync().ConfigureAwait(false);

            WriteMessage("Done processing term groups", ProvisioningMessageType.Completed);

            return parser;
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            if (!WillExtract(context, template, configuration))
            {
                return template;
            }

            bool allGroups = configuration.Taxonomy.IncludeAllTermGroups;

            ITermGroup siteCollectionGroup = null;
            try
            {
                siteCollectionGroup = await SiteCollectionTermGroupResolver.GetAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: this site has no term group of its own.", Constants.LOGGING_SOURCE);
            }

            if (!allGroups && siteCollectionGroup == null)
            {
                WriteMessage("This site has no term group of its own, so no term groups were extracted.",
                    ProvisioningMessageType.Warning);
                return template;
            }

            try
            {
                await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name, g => g.Scope,
                    g => g.Description)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The term store could not be read: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return template;
            }

            List<ITermGroup> groups = context.TermStore.Groups.AsRequested()
                .Where(g => allGroups || (siteCollectionGroup != null && g.Id == siteCollectionGroup.Id))
                .ToList();

            int index = 0;

            foreach (ITermGroup group in groups)
            {
                index++;
                WriteSubProgress("Term group", group.Name, index, groups.Count);

                // The system groups hold the tenant's keywords and orphaned terms. They exist on
                // every tenant and are not something a template can meaningfully recreate.
                if (group.Scope == TermGroupScope.System)
                {
                    continue;
                }

                template.TermGroups.Add(await ExtractGroupAsync(context, group,
                    isSiteCollectionGroup: siteCollectionGroup != null && group.Id == siteCollectionGroup.Id)
                    .ConfigureAwait(false));
            }

            WriteMessage("Done processing term groups", ProvisioningMessageType.Completed);

            return template;
        }

        private static async Task<TermGroupModel> ExtractGroupAsync(PnPContext context, ITermGroup group, bool isSiteCollectionGroup)
        {
            var modelGroup = new TermGroupModel
            {
                Id = Guid.TryParse(group.Id, out Guid groupId) ? groupId : Guid.Empty,
                Name = group.Name,
                Description = group.Description,
                SiteCollectionTermGroup = isSiteCollectionGroup,
            };

            await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames, s => s.Description,
                s => s.Properties)).ConfigureAwait(false);

            foreach (ITermSet set in group.Sets.AsRequested())
            {
                modelGroup.TermSets.Add(await ExtractTermSetAsync(context, set).ConfigureAwait(false));
            }

            return modelGroup;
        }

        private static async Task<TermSetModel> ExtractTermSetAsync(PnPContext context, ITermSet set)
        {
            var modelSet = new TermSetModel
            {
                Id = Guid.TryParse(set.Id, out Guid setId) ? setId : Guid.Empty,
                Name = set.LocalizedNames?.FirstOrDefault()?.Name,
                Description = set.Description,
            };

            foreach (ITermSetProperty property in set.Properties ?? Enumerable.Empty<ITermSetProperty>())
            {
                modelSet.Properties[property.KeyField] = property.Value;
            }

            await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels, t => t.Descriptions,
                t => t.Properties)).ConfigureAwait(false);

            foreach (ITerm term in set.Terms.AsRequested())
            {
                modelSet.Terms.Add(await ExtractTermAsync(context, term).ConfigureAwait(false));
            }

            return modelSet;
        }

        private static async Task<TermModel> ExtractTermAsync(PnPContext context, ITerm term)
        {
            var modelTerm = new TermModel
            {
                Id = Guid.TryParse(term.Id, out Guid termId) ? termId : Guid.Empty,
                Name = term.Labels?.FirstOrDefault(l => l.IsDefault)?.Name
                    ?? term.Labels?.FirstOrDefault()?.Name,
                Description = term.Descriptions?.FirstOrDefault()?.Description,
            };

            foreach (ITermLocalizedLabel label in term.Labels ?? Enumerable.Empty<ITermLocalizedLabel>())
            {
                modelTerm.Labels.Add(new TermLabelModel
                {
                    Value = label.Name,
                    Language = LcidOf(label.LanguageTag),
                    IsDefaultForLanguage = label.IsDefault,
                });
            }

            // 🔴 Shared properties only. ITerm exposes `Properties` and nothing else - the
            // shared-versus-local distinction that CSOM makes has no representation in the Graph
            // read model at all. Spike S1 recorded that as one of the divergences behind D7; this is
            // the read side of the same gap.
            //
            // The consequence is asymmetric and worth knowing: the apply path *can* write a local
            // property (SetTermCustomPropertyRequest takes the flag), but an extract cannot tell
            // one from the other, so a round trip turns every local property into a shared one.
            foreach (ITermProperty property in term.Properties ?? Enumerable.Empty<ITermProperty>())
            {
                modelTerm.Properties[property.KeyField] = property.Value;
            }

            try
            {
                await term.LoadAsync(t => t.Terms.QueryProperties(child => child.Id, child => child.Labels,
                    child => child.Descriptions, child => child.Properties))
                    .ConfigureAwait(false);

                foreach (ITerm child in term.Terms.AsRequested())
                {
                    modelTerm.Terms.Add(await ExtractTermAsync(context, child).ConfigureAwait(false));
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the child terms of '{Term}' could not be read.",
                    Constants.LOGGING_SOURCE, modelTerm.Name);
            }

            return modelTerm;
        }

        /// <summary>
        /// Converts a Graph language tag to the LCID the schema and the CSOM requests use.
        /// </summary>
        private static int LcidOf(string languageTag)
        {
            if (string.IsNullOrEmpty(languageTag))
            {
                return 0;
            }

            try
            {
                return new CultureInfo(languageTag).LCID;
            }
            catch (CultureNotFoundException)
            {
                return 0;
            }
        }

        #endregion
    }
}
