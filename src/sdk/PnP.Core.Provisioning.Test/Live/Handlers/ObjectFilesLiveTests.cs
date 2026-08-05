using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreList = PnP.Core.Model.SharePoint.IList;
using DirectoryModel = PnP.Core.Provisioning.Model.Directory;
using FileLevelModel = PnP.Core.Provisioning.Model.FileLevel;
using FileModel = PnP.Core.Provisioning.Model.File;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectFiles</c>.
    /// </summary>
    [TestClass]
    public class ObjectFilesLiveTests : LiveTestBase
    {
        private static string LibraryTitle => $"{TestPrefix}Docs";

        private static string LibraryUrl => $"{TestPrefix}Docs";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Files_UploadsAFileIntoALibraryTheSameTemplateCreates()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string sourceFolder = CreateSourceFiles(("hello.txt", "hello from the template"));

                try
                {
                    ProvisioningTemplate template = TemplateWithLibrary(sourceFolder);

                    template.Lists[0].EnableVersioning = true;
                    template.Lists[0].EnableMinorVersions = true;

                    template.Files.Add(new FileModel
                    {
                        Src = "hello.txt",
                        Folder = LibraryUrl,
                        Overwrite = true,
                        Level = FileLevelModel.Published,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        string url = await LibraryUrlAsync(fresh).ConfigureAwait(false);
                        IFile uploaded = await FindFileAsync(fresh, $"{url}/hello.txt").ConfigureAwait(false);

                        Assert.IsNotNull(uploaded, "The file was not uploaded.");
                        Console.WriteLine($"Uploaded {uploaded.ServerRelativeUrl}");

                        string content = await ReadTextAsync(uploaded).ConfigureAwait(false);
                        Assert.AreEqual("hello from the template", content, "The file's contents are not the template's.");

                        await uploaded.LoadAsync(f => f.Level).ConfigureAwait(false);
                        Console.WriteLine($"Level: {uploaded.Level}");

                        Assert.AreEqual(PublishedStatus.Published, uploaded.Level,
                            "The template asked for Published and the file is still a draft.");
                    }
                }
                finally
                {
                    Cleanup(sourceFolder);
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Files_OverwriteReplacesTheContentAndDoesNotDuplicate()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string sourceFolder = CreateSourceFiles(("hello.txt", "first"));

                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    ProvisioningTemplate first = TemplateWithLibrary(sourceFolder);
                    first.Files.Add(new FileModel { Src = "hello.txt", Folder = LibraryUrl, Overwrite = true });
                    await manager.ApplyTemplateAsync(first, Reporting()).ConfigureAwait(false);

                    System.IO.File.WriteAllText(Path.Combine(sourceFolder, "hello.txt"), "second");

                    ProvisioningTemplate second = TemplateWithLibrary(sourceFolder);
                    second.Files.Add(new FileModel { Src = "hello.txt", Folder = LibraryUrl, Overwrite = true });
                    await manager.ApplyTemplateAsync(second, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        string url = await LibraryUrlAsync(fresh).ConfigureAwait(false);

                        IFolder folder = await fresh.Web.GetFolderByServerRelativeUrlAsync(url,
                            f => f.Files.QueryProperties(file => file.Name)).ConfigureAwait(false);

                        List<string> names = folder.Files.AsRequested().Select(f => f.Name).ToList();
                        Console.WriteLine($"Library holds: {string.Join(", ", names)}");

                        Assert.AreEqual(1, names.Count, "The second apply added a second file instead of replacing.");

                        IFile uploaded = await FindFileAsync(fresh, $"{url}/hello.txt").ConfigureAwait(false);
                        Assert.AreEqual("second", await ReadTextAsync(uploaded).ConfigureAwait(false),
                            "The file was not overwritten.");
                    }
                }
                finally
                {
                    Cleanup(sourceFolder);
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Files_ADirectoryElementUploadsEveryFileItMatches()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string sourceFolder = CreateSourceFiles(
                    ("one.txt", "one"),
                    ("two.txt", "two"),
                    ("skip.bin", "not a text file"));

                try
                {
                    ProvisioningTemplate template = TemplateWithLibrary(sourceFolder);
                    template.Directories.Add(new DirectoryModel
                    {
                        Src = string.Empty,
                        Folder = LibraryUrl,
                        Overwrite = true,
                        IncludedExtensions = "*.txt",
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        string url = await LibraryUrlAsync(fresh).ConfigureAwait(false);

                        IFolder folder = await fresh.Web.GetFolderByServerRelativeUrlAsync(url,
                            f => f.Files.QueryProperties(file => file.Name)).ConfigureAwait(false);

                        List<string> names = folder.Files.AsRequested().Select(f => f.Name).OrderBy(n => n).ToList();
                        Console.WriteLine($"Library holds: {string.Join(", ", names)}");

                        CollectionAssert.AreEqual(new[] { "one.txt", "two.txt" }, names,
                            "The directory element did not upload exactly the files its extension filter matches.");
                    }
                }
                finally
                {
                    Cleanup(sourceFolder);
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Files_AMissingSourceFileFailsLoudly()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string sourceFolder = CreateSourceFiles(("present.txt", "here"));

                try
                {
                    ProvisioningTemplate template = TemplateWithLibrary(sourceFolder);
                    template.Files.Add(new FileModel { Src = "absent.txt", Folder = LibraryUrl, Overwrite = true });

                    await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                        () => context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()))
                        .ConfigureAwait(false);
                }
                finally
                {
                    Cleanup(sourceFolder);
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static ProvisioningTemplate TemplateWithLibrary(string sourceFolder)
        {
            var template = new ProvisioningTemplate
            {
                Connector = new FileSystemConnector(sourceFolder, string.Empty),
            };

            template.Lists.Add(new ListInstance
            {
                Title = LibraryTitle,
                Url = LibraryUrl,
                TemplateType = (int)ListTemplateType.DocumentLibrary,
            });

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

        private static string CreateSourceFiles(params (string Name, string Content)[] files)
        {
            string folder = Path.Combine(Path.GetTempPath(), $"{TestPrefix}{Guid.NewGuid():N}");
            System.IO.Directory.CreateDirectory(folder);

            foreach ((string name, string content) in files)
            {
                System.IO.File.WriteAllText(Path.Combine(folder, name), content);
            }

            return folder;
        }

        private static void Cleanup(string folder)
        {
            try
            {
                if (System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE '{folder}': {ex.Message}");
            }
        }

        private static async Task<string> LibraryUrlAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            CoreList library = context.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == LibraryTitle);
            Assert.IsNotNull(library, "The library was not created.");

            Console.WriteLine($"Library root folder: {library.RootFolder.ServerRelativeUrl}");

            return library.RootFolder.ServerRelativeUrl;
        }

        private static async Task<IFile> FindFileAsync(PnPContext context, string serverRelativeUrl)
        {
            try
            {
                return await context.Web.GetFileByServerRelativeUrlAsync(serverRelativeUrl,
                    f => f.Name, f => f.ServerRelativeUrl, f => f.UniqueId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<string> ReadTextAsync(IFile file)
        {
            using (Stream content = await file.GetContentAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(content))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
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
