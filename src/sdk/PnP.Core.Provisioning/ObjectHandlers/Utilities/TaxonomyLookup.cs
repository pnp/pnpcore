using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Taxonomy lookups the provisioning engine needs but the Graph term store does not offer
    /// directly.
    /// </summary>
    internal static class TaxonomyLookup
    {
        /// <summary>
        /// Finds a term set and its owning group by the term set's id.
        /// </summary>
        /// <returns>The group and set, or <c>(null, null)</c> when no such set exists.</returns>
        internal static async Task<(ITermGroup Group, ITermSet Set)> FindTermSetAsync(PnPContext context, string termSetId)
        {
            if (string.IsNullOrEmpty(termSetId))
            {
                return (null, null);
            }

            await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(
                g => g.Id, g => g.Name, g => g.Scope)).ConfigureAwait(false);

            foreach (ITermGroup group in context.TermStore.Groups.AsRequested())
            {
                await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames)).ConfigureAwait(false);

                ITermSet set = group.Sets.AsRequested()
                    .FirstOrDefault(s => termSetId.Equals(s.Id, StringComparison.OrdinalIgnoreCase));

                if (set != null)
                {
                    return (group, set);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Finds a term written as a <c>Group|Set|Term|Child</c> path.
        /// </summary>
        /// <returns>The term, or null when any segment of the path does not exist.</returns>
        internal static async Task<ITerm> FindTermByPathAsync(PnPContext context, string termPath)
        {
            if (string.IsNullOrEmpty(termPath))
            {
                return null;
            }

            string[] segments = termPath.Split('|');
            if (segments.Length < 3)
            {
                return null;
            }

            await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name))
                .ConfigureAwait(false);

            ITermGroup group = context.TermStore.Groups.AsRequested()
                .FirstOrDefault(g => string.Equals(g.Name, segments[0], StringComparison.OrdinalIgnoreCase));

            if (group == null)
            {
                return null;
            }

            await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames)).ConfigureAwait(false);

            ITermSet set = group.Sets.AsRequested().FirstOrDefault(s =>
                s.LocalizedNames.Any(n => string.Equals(n.Name, segments[1], StringComparison.OrdinalIgnoreCase)));

            if (set == null)
            {
                return null;
            }

            await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels)).ConfigureAwait(false);
            ITerm term = MatchTerm(set.Terms.AsRequested(), segments[2]);

            for (int i = 3; i < segments.Length && term != null; i++)
            {
                await term.LoadAsync(t => t.Terms.QueryProperties(child => child.Id, child => child.Labels)).ConfigureAwait(false);
                term = MatchTerm(term.Terms.AsRequested(), segments[i]);
            }

            return term;
        }

        private static ITerm MatchTerm(IEnumerable<ITerm> terms, string name)
        {
            return terms.FirstOrDefault(t =>
                t.Labels.Any(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
