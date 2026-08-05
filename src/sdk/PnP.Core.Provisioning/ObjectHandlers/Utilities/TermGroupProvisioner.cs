using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy;
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

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Writes one <c>&lt;pnp:TermGroup&gt;</c> - its term sets, its terms, their labels, custom
    /// properties and ordering.
    /// </summary>
    internal sealed class TermGroupProvisioner
    {
        private readonly PnPContext context;
        private readonly TokenParser parser;
        private readonly Action<string> reportWarning;

        /// <summary>
        /// Reuses collected during the walk, applied after every group exists.
        /// </summary>
        private readonly List<DeferredReuse> deferredReuses = new List<DeferredReuse>();

        internal TermGroupProvisioner(PnPContext pnpContext, TokenParser tokenParser, Action<string> warn)
        {
            context = pnpContext;
            parser = tokenParser;
            reportWarning = warn;
        }

        /// <summary>
        /// The default language of the term store, used when a term set or term does not name one.
        /// </summary>
        private int defaultLanguage;

        internal async Task ProcessGroupAsync(TermGroupModel modelGroup)
        {
            await EnsureDefaultLanguageAsync().ConfigureAwait(false);

            ITermGroup group = await EnsureGroupAsync(modelGroup).ConfigureAwait(false);

            if (group == null)
            {
                return;
            }

            foreach (TermSetModel modelTermSet in modelGroup.TermSets)
            {
                await ProcessTermSetAsync(modelGroup, group, modelTermSet).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Applies the reuses that were deferred until every group existed.
        /// </summary>
        internal async Task ApplyDeferredReusesAsync()
        {
            foreach (DeferredReuse reuse in deferredReuses)
            {
                try
                {
                    await CsomRequestSender.SendAsync(context, new ReuseTermRequest(
                        reuse.ParentId, reuse.ParentIsTermSet, reuse.ModelTerm.SourceTermId,
                        reuseBranch: reuse.ModelTerm.IsReused && reuse.ModelTerm.Terms.Any(),
                        pin: false)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn($"The term '{reuse.ModelTerm.Name}' could not reuse " +
                        $"{reuse.ModelTerm.SourceTermId}: {ErrorText.Describe(ex)}", ex);
                }
            }

            deferredReuses.Clear();
        }

        #region Group

        private async Task<ITermGroup> EnsureGroupAsync(TermGroupModel modelGroup)
        {
            string name = parser.ParseString(modelGroup.Name);

            // The site collection group is not created by name - it is fetched, and created by the
            // server on demand, because its name is derived from the site and is not the template's
            // to choose.
            if (await IsSiteCollectionGroupAsync(modelGroup, name).ConfigureAwait(false))
            {
                return await EnsureSiteCollectionGroupAsync().ConfigureAwait(false);
            }

            ITermGroup existing = await FindGroupAsync(modelGroup.Id, name).ConfigureAwait(false);
            if (existing != null)
            {
                return existing;
            }

            if (modelGroup.Id == Guid.Empty)
            {
                // The schema allows an id-less group. One is minted here rather than letting the
                // server choose, so the tokens registered below refer to something stable.
                modelGroup.Id = Guid.NewGuid();
            }

            try
            {
                TermGroupInfo created = await CsomRequestSender.SendAsync(context, new CreateTermGroupRequest(
                    name, modelGroup.Id, parser.ParseString(modelGroup.Description))).ConfigureAwait(false);

                if (created == null)
                {
                    Warn($"The term group '{name}' was not created - the server returned nothing.", null);
                    return null;
                }

                await WarnOnUnsupportedGroupMembersAsync(modelGroup, name).ConfigureAwait(false);

                return await FindGroupAsync(created.Id, name).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn($"The term group '{name}' could not be created: {ErrorText.Describe(ex)}", ex);
                return null;
            }
        }

        /// <summary>
        /// Whether the template means the site's own term group rather than a named one.
        /// </summary>
        private async Task<bool> IsSiteCollectionGroupAsync(TermGroupModel modelGroup, string parsedName)
        {
            if (modelGroup.SiteCollectionTermGroup
                || string.Equals(modelGroup.Name, "Site Collection", StringComparison.Ordinal))
            {
                return true;
            }

            string siteCollectionGroupName = await new SiteCollectionTermGroupNameToken(context)
                .GetReplaceValueAsync().ConfigureAwait(false);

            return !string.IsNullOrEmpty(siteCollectionGroupName)
                && string.Equals(parsedName, siteCollectionGroupName, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<ITermGroup> EnsureSiteCollectionGroupAsync()
        {
            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                TermGroupInfo info = await CsomRequestSender.SendAsync(context,
                    new GetSiteCollectionTermGroupRequest(siteId, webId, createIfMissing: true)).ConfigureAwait(false);

                if (info == null || info.Id == Guid.Empty)
                {
                    Warn("The site collection term group could not be created.", null);
                    return null;
                }

                return await FindGroupAsync(info.Id, info.Name).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn($"The site collection term group could not be resolved: {ErrorText.Describe(ex)}", ex);
                return null;
            }
        }

        /// <summary>
        /// Reports the group members a template can declare and this port does not write.
        /// </summary>
        private Task WarnOnUnsupportedGroupMembersAsync(TermGroupModel modelGroup, string name)
        {
            int count = (modelGroup.Contributors?.Count ?? 0) + (modelGroup.Managers?.Count ?? 0);

            if (count > 0)
            {
                Warn($"The term group '{name}' declares {count} contributor(s) or manager(s). " +
                    "Assigning term store roles is not supported by this engine yet, so they were skipped.", null);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Term sets

        private async Task ProcessTermSetAsync(TermGroupModel modelGroup, ITermGroup group, TermSetModel modelTermSet)
        {
            string name = parser.ParseString(modelTermSet.Name);
            int language = modelTermSet.Language ?? defaultLanguage;

            ITermSet set = await FindTermSetAsync(group, modelTermSet.Id, name).ConfigureAwait(false);

            if (set == null)
            {
                if (modelTermSet.Id == Guid.Empty)
                {
                    modelTermSet.Id = Guid.NewGuid();
                }

                try
                {
                    await CsomRequestSender.SendAsync(context, new CreateTermSetRequest(
                        group.Id != null ? Guid.Parse(group.Id) : Guid.Empty,
                        name, modelTermSet.Id, language,
                        parser.ParseString(modelTermSet.Description),
                        modelTermSet.IsOpenForTermCreation)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn($"The term set '{name}' could not be created: {ErrorText.Describe(ex)}", ex);
                    return;
                }

                set = await FindTermSetAsync(group, modelTermSet.Id, name).ConfigureAwait(false);

                if (set == null)
                {
                    Warn($"The term set '{name}' was created but could not be read back.", null);
                    return;
                }
            }

            RegisterTermSetTokens(group, name, modelTermSet.Id);

            await WarnOnUnsupportedTermSetPropertiesAsync(modelTermSet, name).ConfigureAwait(false);

            await ProcessTermsAsync(modelTermSet.Terms, modelTermSet.Id, parentIsTermSet: true, language,
                await ReadExistingTermsAsync(set).ConfigureAwait(false)).ConfigureAwait(false);

            await ApplyCustomSortOrderAsync(modelTermSet.Terms, modelTermSet.Id, isTermSet: true, name).ConfigureAwait(false);
        }

        private void RegisterTermSetTokens(ITermGroup group, string termSetName, Guid termSetId)
        {
            parser.AddToken(new TermSetIdToken(context, group.Name, termSetName, termSetId));

            if (group.Scope == TermGroupScope.SiteCollection)
            {
                parser.AddToken(new SiteCollectionTermSetIdToken(context, termSetName, termSetId));
            }
        }

        /// <summary>
        /// Reports the term set settings this port does not write.
        /// </summary>
        private Task WarnOnUnsupportedTermSetPropertiesAsync(TermSetModel modelTermSet, string name)
        {
            var unsupported = new List<string>();

            if (!string.IsNullOrEmpty(modelTermSet.Owner))
            {
                unsupported.Add("Owner");
            }

            if (modelTermSet.Properties != null && modelTermSet.Properties.Count > 0)
            {
                unsupported.Add($"{modelTermSet.Properties.Count} custom propert(ies)");
            }

            if (unsupported.Count > 0)
            {
                Warn($"The term set '{name}' declares {string.Join(" and ", unsupported)}, " +
                    "which this engine does not write yet.", null);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Terms

        /// <summary>
        /// Creates or matches each term, then recurses into its children.
        /// </summary>
        private async Task ProcessTermsAsync(IEnumerable<TermModel> modelTerms, Guid parentId, bool parentIsTermSet,
            int language, Dictionary<Guid, ITerm> existingById)
        {
            foreach (TermModel modelTerm in modelTerms)
            {
                if (modelTerm.IsReused && modelTerm.SourceTermId != Guid.Empty)
                {
                    // Deferred: the source may be declared later in the same template.
                    deferredReuses.Add(new DeferredReuse
                    {
                        ModelTerm = modelTerm,
                        ParentId = parentId,
                        ParentIsTermSet = parentIsTermSet,
                    });

                    continue;
                }

                (bool ok, ITerm existingTerm) = await EnsureTermAsync(modelTerm, parentId, parentIsTermSet,
                    language, existingById).ConfigureAwait(false);

                if (!ok)
                {
                    continue;
                }

                await ApplyTermExtrasAsync(modelTerm).ConfigureAwait(false);

                if (!modelTerm.Terms.Any())
                {
                    continue;
                }

                // A term that was just created has no children to reconcile against, and Graph has
                // no way to fetch a term by id alone - so not reading them back is both correct and
                // the difference between one round trip and a walk of the whole store.
                Dictionary<Guid, ITerm> children = existingTerm == null
                    ? new Dictionary<Guid, ITerm>()
                    : await ReadChildTermsAsync(existingTerm).ConfigureAwait(false);

                await ProcessTermsAsync(modelTerm.Terms, modelTerm.Id, parentIsTermSet: false, language, children)
                    .ConfigureAwait(false);

                await ApplyCustomSortOrderAsync(modelTerm.Terms, modelTerm.Id, isTermSet: false, modelTerm.Name)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Matches the term against what is already there, or creates it.
        /// </summary>
        /// <returns>
        /// Whether the term now exists, and the existing term when it was matched rather than
        /// created - the caller needs that only to read its children.
        /// </returns>
        private async Task<(bool Ok, ITerm Existing)> EnsureTermAsync(TermModel modelTerm, Guid parentId,
            bool parentIsTermSet, int language, Dictionary<Guid, ITerm> existingById)
        {
            string name = parser.ParseString(modelTerm.Name);

            if (modelTerm.Id != Guid.Empty && existingById.TryGetValue(modelTerm.Id, out ITerm byId))
            {
                return (true, byId);
            }

            ITerm byName = existingById.Values.FirstOrDefault(t =>
                t.Labels != null && t.Labels.Any(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase)));

            if (byName != null)
            {
                // Adopt the server's id: everything downstream - child terms, sort order, the
                // template's own references - has to address the term that exists.
                modelTerm.Id = Guid.Parse(byName.Id);
                return (true, byName);
            }

            if (modelTerm.Id == Guid.Empty)
            {
                modelTerm.Id = Guid.NewGuid();
            }

            try
            {
                await CsomRequestSender.SendAsync(context, new CreateTermRequest(
                    parentId, parentIsTermSet, name, modelTerm.Id,
                    modelTerm.Language ?? language,
                    parser.ParseString(modelTerm.Description),
                    modelTerm.IsAvailableForTagging)).ConfigureAwait(false);

                return (true, null);
            }
            catch (Exception ex)
            {
                Warn($"The term '{name}' could not be created: {ErrorText.Describe(ex)}", ex);
                return (false, null);
            }
        }

        /// <summary>
        /// Labels, custom properties and deprecation.
        /// </summary>
        private async Task ApplyTermExtrasAsync(TermModel modelTerm)
        {
            foreach (TermLabelModel label in modelTerm.Labels)
            {
                try
                {
                    await CsomRequestSender.SendAsync(context, new AddTermLabelRequest(
                        modelTerm.Id, parser.ParseString(label.Value), label.Language, label.IsDefaultForLanguage))
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // A label that already exists is the common case on a re-apply.
                    context.Logger?.LogDebug(ex, "{Source}: the label '{Label}' was not added to term '{Term}'.",
                        Constants.LOGGING_SOURCE, label.Value, modelTerm.Name);
                }
            }

            await ApplyCustomPropertiesAsync(modelTerm.Id, modelTerm.Properties, isLocal: false, modelTerm.Name)
                .ConfigureAwait(false);
            await ApplyCustomPropertiesAsync(modelTerm.Id, modelTerm.LocalProperties, isLocal: true, modelTerm.Name)
                .ConfigureAwait(false);

            if (modelTerm.IsDeprecated)
            {
                try
                {
                    await CsomRequestSender.SendAsync(context, new DeprecateTermRequest(modelTerm.Id, true))
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn($"The term '{modelTerm.Name}' could not be deprecated: {ErrorText.Describe(ex)}", ex);
                }
            }
        }

        private async Task ApplyCustomPropertiesAsync(Guid termId, Dictionary<string, string> properties,
            bool isLocal, string termName)
        {
            if (properties == null || properties.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> property in properties)
            {
                try
                {
                    await CsomRequestSender.SendAsync(context, new SetTermCustomPropertyRequest(
                        termId, property.Key, parser.ParseString(property.Value), isLocal)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string kind = isLocal ? "local" : "shared";
                    Warn($"The {kind} property '{property.Key}' could not be set on term '{termName}': {ErrorText.Describe(ex)}", ex);
                }
            }
        }

        /// <summary>
        /// Writes the custom sort order a template declares for a set of siblings.
        /// </summary>
        private async Task ApplyCustomSortOrderAsync(IEnumerable<TermModel> modelTerms, Guid parentId,
            bool isTermSet, string parentName)
        {
            List<TermModel> ordered = modelTerms
                .Where(t => t.CustomSortOrder > 0 && t.Id != Guid.Empty)
                .OrderBy(t => t.CustomSortOrder)
                .ToList();

            if (ordered.Count == 0)
            {
                return;
            }

            string sortOrder = string.Join(":", ordered.Select(t => t.Id.ToString()));

            try
            {
                await CsomRequestSender.SendAsync(context,
                    new SetCustomSortOrderRequest(parentId, isTermSet, sortOrder)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn($"The custom sort order of '{parentName}' could not be set: {ErrorText.Describe(ex)}", ex);
            }
        }

        #endregion

        #region Reads

        private async Task EnsureDefaultLanguageAsync()
        {
            if (defaultLanguage != 0)
            {
                return;
            }

            await context.TermStore.LoadAsync(t => t.DefaultLanguage).ConfigureAwait(false);

            // The store's default language is a culture name in Graph and an LCID in CSOM, and the
            // requests take an LCID.
            defaultLanguage = new CultureInfo(context.TermStore.DefaultLanguage).LCID;
        }

        /// <summary>
        /// Finds a term group by id, falling back to its name.
        /// </summary>
        private async Task<ITermGroup> FindGroupAsync(Guid id, string name)
        {
            await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name, g => g.Scope))
                .ConfigureAwait(false);

            List<ITermGroup> groups = context.TermStore.Groups.AsRequested().ToList();

            return groups.FirstOrDefault(g => id != Guid.Empty && string.Equals(g.Id, id.ToString(), StringComparison.OrdinalIgnoreCase))
                ?? groups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static async Task<ITermSet> FindTermSetAsync(ITermGroup group, Guid id, string name)
        {
            await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames)).ConfigureAwait(false);

            List<ITermSet> sets = group.Sets.AsRequested().ToList();

            return sets.FirstOrDefault(s => id != Guid.Empty && string.Equals(s.Id, id.ToString(), StringComparison.OrdinalIgnoreCase))
                ?? sets.FirstOrDefault(s => s.LocalizedNames.Any(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase)));
        }

        private static async Task<Dictionary<Guid, ITerm>> ReadExistingTermsAsync(ITermSet set)
        {
            await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels)).ConfigureAwait(false);

            return set.Terms.AsRequested()
                .Where(t => Guid.TryParse(t.Id, out _))
                .GroupBy(t => Guid.Parse(t.Id))
                .ToDictionary(g => g.Key, g => g.First());
        }

        private static async Task<Dictionary<Guid, ITerm>> ReadChildTermsAsync(ITerm term)
        {
            await term.LoadAsync(t => t.Terms.QueryProperties(child => child.Id, child => child.Labels))
                .ConfigureAwait(false);

            return term.Terms.AsRequested()
                .Where(t => Guid.TryParse(t.Id, out _))
                .GroupBy(t => Guid.Parse(t.Id))
                .ToDictionary(g => g.Key, g => g.First());
        }

        #endregion

        private void Warn(string message, Exception ex)
        {
            if (ex != null)
            {
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            }
            else
            {
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            }

            reportWarning?.Invoke(message);
        }

        /// <summary>
        /// A reuse held back until every group in the template exists.
        /// </summary>
        private sealed class DeferredReuse
        {
            internal TermModel ModelTerm { get; set; }

            internal Guid ParentId { get; set; }

            internal bool ParentIsTermSet { get; set; }
        }
    }
}
