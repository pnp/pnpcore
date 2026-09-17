using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FieldModel = PnP.Core.Provisioning.Model.Field;
using TermGroupModel = PnP.Core.Provisioning.Model.TermGroup;
using TermModel = PnP.Core.Provisioning.Model.Term;
using TermSetModel = PnP.Core.Provisioning.Model.TermSet;

namespace PnP.Core.Provisioning.Test.Scenarios
{
    /// <summary>
    /// Scenario 5 - term groups, a taxonomy site column bound to the term set, and a list that uses
    /// it.
    /// </summary>
    [TestClass]
    public class Scenario05TaxonomyTests : ScenarioTestBase
    {
        private const string Prefix = "PnPCoreScenario5_";

        private static readonly Guid GroupId = Guid.NewGuid();
        private static readonly Guid TermSetId = Guid.NewGuid();
        private static readonly Guid FirstTermId = Guid.NewGuid();
        private static readonly Guid SecondTermId = Guid.NewGuid();

        private static readonly string GroupName = $"{Prefix}Group_{GroupId:N}".Substring(0, 40);
        private static readonly string TermSetName = $"{Prefix}Set";
        private static readonly string TaxonomyFieldId = "{6b1f2a3c-9d4e-4a5b-8c7d-2e1f0a9b8c05}";
        private static readonly string ListUrl = $"Lists/{Prefix}Tagged";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Scenario")]
        [TestCategory("Taxonomy")]
        [Timeout(45 * 60 * 1000)]
        public async Task Scenario5_TermGroupsAndATaxonomyColumnBoundToThem()
        {
            try
            {
                await RunScenarioAsync("s5", BuildTemplate(), new[]
                {
                    ConfigurationHandler.Taxonomy,
                    ConfigurationHandler.Fields,
                    ConfigurationHandler.Lists,
                },
                AssertAsync,

                configuration => configuration.Taxonomy.IncludeAllTermGroups = true)
                .ConfigureAwait(false);
            }
            finally
            {
                await CleanUpTermGroupAsync().ConfigureAwait(false);
            }
        }

        private static ProvisioningTemplate BuildTemplate()
        {
            var template = new ProvisioningTemplate { Id = "SCENARIO-5" };

            var first = new TermModel
            {
                Id = FirstTermId,
                Name = $"First_{Prefix}",
                Description = "Scenario 5 first term",
                CustomSortOrder = 1,
            };

            first.Labels.Add(new TermLabel
            {
                Value = $"Alias_{Prefix}",
                Language = CultureInfo.GetCultureInfo("en-US").LCID,
                IsDefaultForLanguage = false,
            });

            var second = new TermModel
            {
                Id = SecondTermId,
                Name = $"Second_{Prefix}",
                CustomSortOrder = 2,
            };

            var set = new TermSetModel
            {
                Id = TermSetId,
                Name = TermSetName,
                Description = "Scenario 5 term set",
                IsOpenForTermCreation = false,
            };

            set.Terms.Add(first);
            set.Terms.Add(second);

            var group = new TermGroupModel
            {
                Id = GroupId,
                Name = GroupName,
                Description = "Created by the PnP Core provisioning scenario tests",
            };

            group.TermSets.Add(set);
            template.TermGroups.Add(group);

            template.SiteFields.Add(new FieldModel
            {
                SchemaXml =
                    $"<Field ID=\"{TaxonomyFieldId}\" Type=\"TaxonomyFieldType\" Name=\"{Prefix}Category\" " +
                    $"StaticName=\"{Prefix}Category\" DisplayName=\"Category\" Group=\"{Prefix}Group\" " +
                    "ShowField=\"Term1033\">" +
                    "<Customization><ArrayOfProperty>" +
                    "<Property><Name>SspId</Name><Value xmlns:q1=\"http://www.w3.org/2001/XMLSchema\" " +
                    "p4:type=\"q1:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                    "{keywordstermstoreid}</Value></Property>" +
                    "<Property><Name>TermSetId</Name><Value xmlns:q2=\"http://www.w3.org/2001/XMLSchema\" " +
                    "p4:type=\"q2:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                    $"{TermSetId}</Value></Property>" +
                    "</ArrayOfProperty></Customization>" +
                    "</Field>",
            });

            var list = new ListInstance
            {
                Title = $"{Prefix}Tagged",
                Url = ListUrl,
                TemplateType = (int)ListTemplateType.GenericList,
            };

            list.FieldRefs.Add(new FieldRef($"{Prefix}Category")
            {
                Id = Guid.Parse(TaxonomyFieldId),
            });

            template.Lists.Add(list);

            return template;
        }

        private static async Task AssertAsync(ProvisioningTemplate extracted, PnPContext site)
        {
            var problems = new List<string>();

            await site.GetProvisioningManager().ApplyTemplateAsync(BuildTemplate(), new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    Console.WriteLine($"[{type}] {message}");

                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        problems.Add(message);
                    }
                },
            }).ConfigureAwait(false);

            Assert.AreEqual(0, problems.Count,
                $"Re-applying the template reported problems:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems));

            Console.WriteLine("Re-applied cleanly - the taxonomy field update path works.");

            ITermGroup group = await site.TermStore.Groups.GetByIdAsync(GroupId.ToString(),
                g => g.Id, g => g.Name).ConfigureAwait(false);

            Assert.IsNotNull(group,
                $"The term group was not created with the id the template gave it ({GroupId}). " +
                "This is decision D7 failing: a Graph-created group would have a server-assigned id.");

            Assert.AreEqual(GroupName, group.Name, "The term group has the wrong name.");

            await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id)).ConfigureAwait(false);

            ITermSet set = group.Sets.AsRequested().FirstOrDefault(s => s.Id == TermSetId.ToString());

            Assert.IsNotNull(set,
                $"The term set was not created with the id the template gave it ({TermSetId}).");

            await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id)).ConfigureAwait(false);

            List<string> termIds = set.Terms.AsRequested().Select(t => t.Id).ToList();

            Console.WriteLine($"Term ids: {string.Join(", ", termIds)}");

            CollectionAssert.Contains(termIds, FirstTermId.ToString(),
                $"The first term did not keep the id the template gave it ({FirstTermId}).");
            CollectionAssert.Contains(termIds, SecondTermId.ToString(),
                $"The second term did not keep the id the template gave it ({SecondTermId}).");

            await site.Web.LoadAsync(w => w.Fields.QueryProperties(f => f.InternalName, f => f.SchemaXml))
                .ConfigureAwait(false);

            IField column = site.Web.Fields.AsRequested()
                .FirstOrDefault(f => f.InternalName == $"{Prefix}Category");

            Assert.IsNotNull(column, "The taxonomy site column was not created.");

            string boundSetId = TermSetIdOf(column.SchemaXml);

            Console.WriteLine($"Column bound to term set: {boundSetId}");

            Assert.AreEqual(TermSetId.ToString(), boundSetId?.ToLowerInvariant(),
                "The taxonomy column is not bound to the term set the template created. A column " +
                "bound to a set that does not exist looks identical until someone tries to tag with it.");

            await site.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Title,
                l => l.Fields.QueryProperties(f => f.InternalName))).ConfigureAwait(false);

            IList list = site.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == $"{Prefix}Tagged");

            Assert.IsNotNull(list, "The list was not created.");
            Assert.IsTrue(list.Fields.AsRequested().Any(f => f.InternalName == $"{Prefix}Category"),
                "The taxonomy column did not reach the list that references it.");

            Assert.IsTrue(extracted.TermGroups.Any(g => g.Id == GroupId),
                $"The extract did not report the term group by its id. " +
                $"Found: {string.Join(", ", extracted.TermGroups.Select(g => g.Id))}");
        }

        /// <summary>
        /// Reads the <c>TermSetId</c> a taxonomy column is bound to out of its schema.
        /// </summary>
        private static string TermSetIdOf(string schemaXml)
        {
            if (string.IsNullOrEmpty(schemaXml))
            {
                return null;
            }

            foreach (XElement property in XElement.Parse(schemaXml).Descendants()
                .Where(e => e.Name.LocalName == "Property"))
            {
                XElement name = property.Elements().FirstOrDefault(e => e.Name.LocalName == "Name");

                if (name?.Value == "TermSetId")
                {
                    return property.Elements().FirstOrDefault(e => e.Name.LocalName == "Value")?.Value;
                }
            }

            return null;
        }

        private static async Task CleanUpTermGroupAsync()
        {
            try
            {
                using (PnPContext context = await GetContextAsync(4).ConfigureAwait(false))
                {
                    await DeleteTermGroupDeepAsync(context, GroupId.ToString()).ConfigureAwait(false);
                    Console.WriteLine($"Deleted term group {GroupId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE TERM GROUP {GroupId}: {Describe(ex)}");
            }
        }
    }
}
