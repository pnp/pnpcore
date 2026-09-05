using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreList = PnP.Core.Model.SharePoint.IList;
using DataRowModel = PnP.Core.Provisioning.Model.DataRow;
using FieldModel = PnP.Core.Provisioning.Model.Field;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectListInstanceDataRows</c>.
    /// </summary>
    [TestClass]
    public class ObjectListInstanceDataRowsLiveTests : LiveTestBase
    {
        private static string ListTitle => $"{TestPrefix}Rows";

        private const string TextFieldId = "{3c9ff34d-8031-4e40-be3c-6f2041ca3002}";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task DataRows_CreatesItemsWithTypedValues()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.GetProvisioningManager()
                        .ApplyTemplateAsync(TemplateWithRows(
                            Row("Alpha", "First"),
                            Row("Beta", "Second")), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        List<IListItem> items = await LoadItemsAsync(fresh).ConfigureAwait(false);

                        Console.WriteLine($"{items.Count} item(s): {string.Join(", ", items.Select(i => i.Values["Title"]))}");

                        Assert.AreEqual(2, items.Count, "Expected exactly the two rows the template declares.");

                        IListItem alpha = items.FirstOrDefault(i => (string)i.Values["Title"] == "Alpha");
                        Assert.IsNotNull(alpha, "The row keyed 'Alpha' was not created.");
                        Assert.AreEqual("First", alpha.Values[$"{TestPrefix}Note"], "The second column was not written.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task DataRows_AKeyColumnMakesASecondApplyUpdateRatherThanDuplicate()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(TemplateWithRows(Row("Alpha", "First")), Reporting())
                        .ConfigureAwait(false);

                    await manager.ApplyTemplateAsync(TemplateWithRows(Row("Alpha", "Second")), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        List<IListItem> items = await LoadItemsAsync(fresh).ConfigureAwait(false);

                        Console.WriteLine($"{items.Count} item(s) after two applies");

                        Assert.AreEqual(1, items.Count,
                            "The second apply duplicated the row - the key column did not match an existing item.");

                        Assert.AreEqual("Second", items[0].Values[$"{TestPrefix}Note"],
                            "The row was found but not updated.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task DataRows_SkipBehaviourLeavesAnExistingRowAlone()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(TemplateWithRows(Row("Alpha", "First")), Reporting())
                        .ConfigureAwait(false);

                    ProvisioningTemplate second = TemplateWithRows(Row("Alpha", "Second"));
                    second.Lists[0].DataRows.UpdateBehavior = UpdateBehavior.Skip;

                    await manager.ApplyTemplateAsync(second, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        List<IListItem> items = await LoadItemsAsync(fresh).ConfigureAwait(false);

                        Assert.AreEqual(1, items.Count, "The skipped row was added anyway.");
                        Assert.AreEqual("First", items[0].Values[$"{TestPrefix}Note"],
                            "UpdateBehavior.Skip overwrote the existing row.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Extract

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task DataRows_ExtractsTheRowsItJustWrote()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.GetProvisioningManager()
                        .ApplyTemplateAsync(TemplateWithRows(Row("Alpha", "First"), Row("Beta", "Second")), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        var configuration = new ExtractConfiguration();
                        configuration.Handlers.Add(ConfigurationHandler.Lists);
                        configuration.Lists.Lists.Add(new Model.Configuration.Lists.Lists.ExtractListsListsConfiguration
                        {
                            Title = ListTitle,
                            IncludeItems = true,
                            KeyColumn = "Title",
                        });

                        ProvisioningTemplate extracted = await fresh.GetProvisioningManager()
                            .GetTemplateAsync(configuration).ConfigureAwait(false);

                        ListInstance list = extracted.Lists.FirstOrDefault(l => l.Title == ListTitle);
                        Assert.IsNotNull(list, "The list was not extracted, so its rows had nowhere to go.");

                        Console.WriteLine($"Extracted {list.DataRows.Count} row(s), key column '{list.DataRows.KeyColumn}'");

                        Assert.AreEqual(2, list.DataRows.Count, "Expected both rows.");
                        Assert.AreEqual("Title", list.DataRows.KeyColumn,
                            "The key column was not carried into the template, so a re-apply would duplicate.");

                        DataRowModel alpha = list.DataRows.FirstOrDefault(
                            r => r.Values.TryGetValue("Title", out string t) && t == "Alpha");

                        Assert.IsNotNull(alpha, "The 'Alpha' row was not extracted.");
                        Assert.AreEqual("First", alpha.Values[$"{TestPrefix}Note"],
                            "The second column's value was not extracted.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static Dictionary<string, string> Row(string title, string note)
        {
            return new Dictionary<string, string>
            {
                { "Title", title },
                { $"{TestPrefix}Note", note },
            };
        }

        /// <summary>
        /// A template with the list, its extra column, and the rows - keyed on Title.
        /// </summary>
        private static ProvisioningTemplate TemplateWithRows(params Dictionary<string, string>[] rows)
        {
            var list = new ListInstance
            {
                Title = ListTitle,
                Url = $"Lists/{TestPrefix}Rows",
                TemplateType = (int)ListTemplateType.GenericList,
            };

            list.Fields.Add(new FieldModel
            {
                SchemaXml = $"<Field ID=\"{TextFieldId}\" Type=\"Text\" Name=\"{TestPrefix}Note\" " +
                    $"StaticName=\"{TestPrefix}Note\" DisplayName=\"Suite Note\" Group=\"{TestPrefix}Group\" />",
            });

            list.DataRows.KeyColumn = "Title";

            foreach (Dictionary<string, string> row in rows)
            {
                var dataRow = new DataRowModel();
                foreach (KeyValuePair<string, string> value in row)
                {
                    dataRow.Values.Add(value.Key, value.Value);
                }

                list.DataRows.Add(dataRow);
            }

            var template = new ProvisioningTemplate();
            template.Lists.Add(list);
            return template;
        }

        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        private static async Task<List<IListItem>> LoadItemsAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

            CoreList list = context.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == ListTitle);
            Assert.IsNotNull(list, "The list was not created.");

            await list.LoadItemsByCamlQueryAsync("<View><Query><OrderBy><FieldRef Name='ID' /></OrderBy></Query></View>")
                .ConfigureAwait(false);

            return list.Items.AsRequested().ToList();
        }

        private static async Task SweepAsync()
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

                    foreach (CoreList list in context.Web.Lists.AsRequested()
                        .Where(l => l.Title != null && l.Title.StartsWith(TestPrefix, StringComparison.Ordinal)).ToList())
                    {
                        string name = list.Title;
                        try
                        {
                            await list.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted list '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE list '{name}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP: {Describe(ex)}");
            }
        }

        #endregion
    }
}
