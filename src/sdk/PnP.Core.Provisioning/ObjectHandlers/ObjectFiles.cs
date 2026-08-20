using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreFile = PnP.Core.Model.SharePoint.IFile;
using CoreList = PnP.Core.Model.SharePoint.IList;
using FileLevelModel = PnP.Core.Provisioning.Model.FileLevel;
using FileModel = PnP.Core.Provisioning.Model.File;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Uploads the <c>&lt;pnp:Files&gt;</c> and <c>&lt;pnp:Directories&gt;</c> a template ships.
    /// </summary>
    internal class ObjectFiles : ObjectHandlerBase
    {
        /// <summary>
        /// Extensions a NoScript site will not accept, whatever the library.
        /// </summary>
        private static readonly string[] BlockedExtensionsInNoScript =
        {
            ".asmx", ".ascx", ".aspx", ".htc", ".jar", ".master", ".swf", ".xap", ".xsf",
        };

        /// <summary>
        /// Libraries a NoScript site will not accept any upload into.
        /// </summary>
        private static readonly string[] BlockedLibrariesInNoScript =
        {
            "_catalogs/theme", "style library", "_catalogs/lt", "_catalogs/wp",
        };

        public override string Name => "Files";

        public override string InternalName => "Files";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Files.Any() || template.Directories.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= configuration?.Lists?.Lists?.Any(l => l.IncludeFiles) == true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!WillProvision(context, template, configuration))
            {
                return parser;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl, w => w.Url).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            bool isNoScriptSite = await web.IsNoScriptSiteAsync().ConfigureAwait(false);
            await LoadMinorVersionSupportAsync(context).ConfigureAwait(false);


            List<FileModel> files = template.Files
                .Concat(TemplateFileUtilities.ExpandDirectories(template))
                .ToList();

            if (files.Any(f => f.Folder != null && f.Folder.IndexOf("siteassets", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                await context.Web.Lists.EnsureSiteAssetsLibraryAsync(l => l.Id).ConfigureAwait(false);
            }

            int index = 0;

            foreach (FileModel file in files)
            {
                index++;

                string source = parser.ParseString(file.Src);
                string targetFileName = parser.ParseString(!string.IsNullOrEmpty(file.TargetFileName)
                    ? file.TargetFileName
                    : Path.GetFileName(source.Replace('\\', '/')));

                WriteSubProgress("File", targetFileName, index, files.Count);

                string folderName = NormaliseFolder(parser.ParseString(file.Folder), webUrl);

                if (ShouldSkip(isNoScriptSite, targetFileName, folderName))
                {
                    string warning = $"This is a NoScript site, so '{targetFileName}' was not uploaded to '{folderName}'.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                await UploadAsync(context, template, file, source, targetFileName, folderName, webUrl, parser)
                    .ConfigureAwait(false);
            }

            WriteMessage("Done processing files", ProvisioningMessageType.Completed);

            return parser;
        }

        private async Task UploadAsync(PnPContext context, ProvisioningTemplate template, FileModel file,
            string source, string targetFileName, string folderName, string webUrl, TokenParser parser)
        {
            IFolder folder;
            try
            {
                folder = await EnsureFolderAsync(context, folderName, webUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The folder '{webUrl}/{folderName}' could not be created: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            RegisterFolderTokens(context, parser, folder, webUrl);

            CoreFile existing = await FindFileAsync(context, $"{folder.ServerRelativeUrl}/{targetFileName}").ConfigureAwait(false);

            if (existing != null && !file.Overwrite)
            {
                context.Logger?.LogDebug("{Source}: '{File}' already exists and the template does not overwrite it.",
                    Constants.LOGGING_SOURCE, targetFileName);

                await FinaliseAsync(context, template, existing, file, folder, webUrl, parser, checkedOut: false)
                    .ConfigureAwait(false);
                return;
            }

            byte[] bytes = TemplateFileUtilities.TryGetFileBytes(template, source);
            if (bytes == null)
            {
                string message = $"The file '{source}' is not present in the template's files.";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                throw new FileNotFoundException(message, source);
            }

            bool checkedOut = existing != null && await CheckOutIfNeededAsync(context, existing).ConfigureAwait(false);

            CoreFile uploaded;
            try
            {
                using (var content = new MemoryStream(bytes))
                {
                    uploaded = await folder.Files.AddAsync(targetFileName, content, true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                string warning = $"'{targetFileName}' could not be uploaded to '{folderName}': {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            if (existing == null)
            {
                checkedOut = await CheckOutIfNeededAsync(context, uploaded).ConfigureAwait(false);
            }

            await FinaliseAsync(context, template, uploaded, file, folder, webUrl, parser, checkedOut).ConfigureAwait(false);
        }

        /// <summary>
        /// Everything that happens to a file once its bytes are in place.
        /// </summary>
        private async Task FinaliseAsync(PnPContext context, ProvisioningTemplate template, CoreFile targetFile,
            FileModel file, IFolder folder, string webUrl, TokenParser parser, bool checkedOut)
        {
            await targetFile.LoadAsync(f => f.UniqueId, f => f.ServerRelativeUrl).ConfigureAwait(false);

            RegisterFileTokens(context, parser, targetFile, webUrl);

            await AddWebPartsAsync(context, targetFile, file, parser).ConfigureAwait(false);

            await ApplyPropertiesAsync(context, targetFile, file, folder, parser).ConfigureAwait(false);

            await ApplyLevelAsync(context, targetFile, file, checkedOut).ConfigureAwait(false);

            if (HasSecurity(file))
            {
                try
                {
                    await targetFile.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);

                    await SecurityUtilities.ApplyAsync(context, targetFile.ListItemAllFields, file.Security, parser,
                        $"file '{targetFile.ServerRelativeUrl}'", m => WriteMessage(m, ProvisioningMessageType.Warning))
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The permissions of '{targetFile.ServerRelativeUrl}' could not be set: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        private async Task ApplyPropertiesAsync(PnPContext context, CoreFile targetFile, FileModel file,
            IFolder folder, TokenParser parser)
        {
            if (file.Properties == null || file.Properties.Count == 0)
            {
                return;
            }

            try
            {
                await targetFile.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);

                IListItem item = targetFile.ListItemAllFields;
                if (item == null)
                {
                    return;
                }

                IList parentList = await FindParentListAsync(context, folder).ConfigureAwait(false);
                if (parentList == null)
                {
                    string warning = $"'{targetFile.ServerRelativeUrl}' is not in a library, so its properties were not set.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    return;
                }

                Dictionary<string, object> values = await ListItemUtilities.BuildValuesAsync(
                    context, parentList, file.Properties, parser,
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);

                if (values.Count == 0)
                {
                    return;
                }

                foreach (KeyValuePair<string, object> value in values)
                {
                    item.Values[value.Key] = value.Value;
                }

                await item.UpdateOverwriteVersionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The properties of '{targetFile.ServerRelativeUrl}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }


        private static FileLevelModel LevelOf(CoreFile file)
        {
            try
            {
                switch (file.Level)
                {
                    case PublishedStatus.Draft:
                        return FileLevelModel.Draft;

                    case PublishedStatus.Checkout:
                        return FileLevelModel.Checkout;

                    default:
                        return FileLevelModel.Published;
                }
            }
            catch (Exception)
            {
                return FileLevelModel.Published;
            }
        }
        private Dictionary<string, bool> _minorVersionsByLibrary;

        private async Task LoadMinorVersionSupportAsync(PnPContext context)
        {
            _minorVersionsByLibrary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await context.Web.LoadAsync(w => w.Lists.QueryProperties(
                    l => l.BaseType, l => l.EnableMinorVersions,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

                foreach (CoreList list in context.Web.Lists.AsRequested())
                {
                    string root = list.RootFolder?.ServerRelativeUrl;

                    if (!string.IsNullOrEmpty(root))
                    {
                        _minorVersionsByLibrary[root.TrimEnd('/')] = list.EnableMinorVersions;
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the libraries' versioning settings could not be read.",
                    Constants.LOGGING_SOURCE);
            }
        }

        private bool SupportsPublishing(string fileServerRelativeUrl)
        {
            if (_minorVersionsByLibrary == null || string.IsNullOrEmpty(fileServerRelativeUrl))
            {
                return false;
            }

            string match = _minorVersionsByLibrary.Keys
                .Where(root => fileServerRelativeUrl.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(root => root.Length)
                .FirstOrDefault();

            return match != null && _minorVersionsByLibrary[match];
        }

        private async Task ApplyLevelAsync(PnPContext context, CoreFile targetFile, FileModel file, bool checkedOut)
        {
            try
            {
                switch (file.Level)
                {
                    case FileLevelModel.Published:
                        if (checkedOut)
                        {
                            await targetFile.CheckinAsync(string.Empty, CheckinType.MajorCheckIn).ConfigureAwait(false);
                        }

                        if (SupportsPublishing(targetFile.ServerRelativeUrl))
                        {
                            await targetFile.PublishAsync().ConfigureAwait(false);
                        }

                        break;

                    case FileLevelModel.Draft:
                        if (checkedOut)
                        {
                            await targetFile.CheckinAsync(string.Empty, CheckinType.MinorCheckIn).ConfigureAwait(false);
                        }

                        break;

                    default:
                        if (checkedOut)
                        {
                            await targetFile.CheckinAsync(string.Empty, CheckinType.MajorCheckIn).ConfigureAwait(false);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                string warning = $"'{targetFile.ServerRelativeUrl}' could not be set to level {file.Level}: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Adds the classic web parts a template places on an uploaded <c>.aspx</c> page.
        /// </summary>
        private async Task AddWebPartsAsync(PnPContext context, CoreFile targetFile, FileModel file, TokenParser parser)
        {
            if (file.WebParts == null || !file.WebParts.Any())
            {
                return;
            }

            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                List<WebPartDefinitionInfo> existing = await CsomRequestSender.SendAsync(context,
                    new GetWebPartDefinitionsRequest(siteId, webId, targetFile.ServerRelativeUrl)).ConfigureAwait(false);

                var titles = new HashSet<string>(
                    (existing ?? new List<WebPartDefinitionInfo>()).Select(w => w.Title).Where(t => t != null),
                    StringComparer.Ordinal);

                foreach (WebPart webPart in file.WebParts)
                {
                    string title = parser.ParseString(webPart.Title);

                    if (titles.Contains(title))
                    {
                        continue;
                    }

                    await CsomRequestSender.SendAsync(context, new AddWebPartRequest(
                        siteId, webId, targetFile.ServerRelativeUrl,
                        parser.ParseXmlString(webPart.Contents).Trim('\n', ' '),
                        webPart.Zone, (int)webPart.Order)).ConfigureAwait(false);

                    titles.Add(title);
                }
            }
            catch (Exception ex)
            {
                string warning = $"The web parts on '{targetFile.ServerRelativeUrl}' could not be added: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        #endregion

        #region Extract
        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            List<string> wanted = configuration?.Lists?.Lists?
                .Where(l => l.IncludeFiles && !string.IsNullOrWhiteSpace(l.Title))
                .Select(l => l.Title)
                .ToList() ?? new List<string>();

            if (wanted.Count == 0)
            {
                return template;
            }

            if (configuration.FileConnector == null)
            {
                const string message = "Files were requested but no FileConnector was configured, so there is "
                    + "nowhere to write them. Set ExtractConfiguration.FileConnector.";

                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);

                return template;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl,
                w => w.Lists.QueryProperties(l => l.Title, l => l.BaseType,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl, f => f.Name))).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            List<CoreList> lists = web.Lists.AsRequested().ToList();

            int index = 0;

            foreach (string title in wanted)
            {
                index++;

                CoreList library = lists.FirstOrDefault(l =>
                    string.Equals(l.Title, title, StringComparison.OrdinalIgnoreCase));

                if (library == null)
                {
                    string warning = $"This site has no list called '{title}', so no files were exported from it.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                if (library.BaseType != ListBaseType.DocumentLibrary)
                {
                    string warning = $"The list '{title}' is not a document library, so it holds no files to export. "
                        + "Use IncludeItems for its items instead.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                WriteSubProgress("Library", title, index, wanted.Count);

                await ExtractLibraryFilesAsync(context, template, configuration, library, webUrl).ConfigureAwait(false);
            }

            WriteMessage("Done processing files", ProvisioningMessageType.Completed);

            return template;
        }

        private async Task ExtractLibraryFilesAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, CoreList library, string webUrl)
        {
            IFolder root = await library.RootFolder.GetAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);

            await ExtractFolderAsync(context, template, configuration, root, webUrl, library.Title).ConfigureAwait(false);
        }

        private async Task ExtractFolderAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, IFolder folder, string webUrl, string libraryTitle)
        {
            await folder.LoadAsync(
                f => f.Files.QueryProperties(file => file.Name, file => file.ServerRelativeUrl, file => file.Level),
                f => f.Folders.QueryProperties(sub => sub.Name, sub => sub.ServerRelativeUrl)).ConfigureAwait(false);

            string folderPath = folder.ServerRelativeUrl.StartsWith(webUrl, StringComparison.OrdinalIgnoreCase)
                ? folder.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/')
                : folder.ServerRelativeUrl.TrimStart('/');

            foreach (CoreFile file in folder.Files.AsRequested())
            {
                try
                {
                    byte[] bytes = await file.GetContentBytesAsync().ConfigureAwait(false);

                    string container = folderPath.Replace(Convert.ToChar(47), Convert.ToChar(92));

                    using (var stream = new MemoryStream(bytes))
                    {
                        configuration.FileConnector.SaveFileStream(file.Name, container, stream);
                    }

                    template.Files.Add(new FileModel
                    {
                        Src = string.IsNullOrEmpty(folderPath) ? file.Name : $"{folderPath}/{file.Name}",
                        Folder = $"{{site}}/{folderPath}",
                        Overwrite = true,
                        Level = LevelOf(file),
                    });
                }
                catch (Exception ex)
                {
                    string warning = $"The file '{file.ServerRelativeUrl}' could not be exported: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            foreach (IFolder child in folder.Folders.AsRequested())
            {
                if (string.Equals(child.Name, "Forms", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await ExtractFolderAsync(context, template, configuration, child, webUrl, libraryTitle).ConfigureAwait(false);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Whether a NoScript site will refuse this upload.
        /// </summary>
        internal static bool ShouldSkip(bool isNoScriptSite, string fileName, string folderName)
        {
            if (!isNoScriptSite)
            {
                return false;
            }

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            if (!string.IsNullOrEmpty(extension) && BlockedExtensionsInNoScript.Contains(extension))
            {
                return true;
            }

            if (string.IsNullOrEmpty(folderName))
            {
                return false;
            }

            string folder = folderName.TrimStart('/').ToLowerInvariant();

            return BlockedLibrariesInNoScript.Any(b => folder.StartsWith(b, StringComparison.Ordinal));
        }

        /// <summary>
        /// Reduces a template's folder value to a path relative to the web.
        /// </summary>
        private static string NormaliseFolder(string folderName, string webUrl)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return string.Empty;
            }

            folderName = folderName.Replace('\\', '/');

            if (folderName.StartsWith(webUrl, StringComparison.OrdinalIgnoreCase))
            {
                folderName = folderName.Substring(webUrl.Length);
            }

            return folderName.Trim('/');
        }

        /// <summary>
        /// Creates the folder path if it does not exist, and returns it.
        /// </summary>
        /// <summary>
        /// Returns the target folder, creating any part of the path that does not exist yet.
        /// </summary>
        private static async Task<IFolder> EnsureFolderAsync(PnPContext context, string folderName, string webUrl)
        {
            string target = string.IsNullOrEmpty(folderName) ? webUrl : $"{webUrl}/{folderName}";

            IFolder existing = await TryGetFolderAsync(context, target).ConfigureAwait(false);
            if (existing != null)
            {
                return existing;
            }

            IFolder current = null;
            string currentUrl = webUrl;

            foreach (string segment in folderName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                currentUrl = $"{currentUrl}/{segment}";

                IFolder next = await TryGetFolderAsync(context, currentUrl).ConfigureAwait(false);

                if (next == null)
                {
                    if (current == null)
                    {
                        throw new InvalidOperationException(
                            $"'{currentUrl}' does not exist. A file's target folder must be inside a list or library " +
                            "that the template creates or the site already has.");
                    }

                    next = await current.Folders.AddAsync(segment).ConfigureAwait(false);
                    await next.LoadAsync(f => f.UniqueId, f => f.ServerRelativeUrl).ConfigureAwait(false);
                }

                current = next;
            }

            return current;
        }

        private static async Task<IFolder> TryGetFolderAsync(PnPContext context, string serverRelativeUrl)
        {
            try
            {
                return await context.Web.GetFolderByServerRelativeUrlAsync(serverRelativeUrl,
                    f => f.UniqueId, f => f.ServerRelativeUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<CoreFile> FindFileAsync(PnPContext context, string serverRelativeUrl)
        {
            try
            {
                return await context.Web.GetFileByServerRelativeUrlAsync(serverRelativeUrl,
                    f => f.UniqueId, f => f.ServerRelativeUrl, f => f.CheckOutType, f => f.Level).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks the file out when its library forces check-out, so it can be written to.
        /// </summary>
        /// <returns>Whether the caller is now responsible for checking it back in.</returns>
        private static async Task<bool> CheckOutIfNeededAsync(PnPContext context, CoreFile targetFile)
        {
            try
            {
                await targetFile.LoadAsync(f => f.CheckOutType,
                    f => f.ListItemAllFields.QueryProperties(i => i.Id)).ConfigureAwait(false);

                IList parentList = await FindParentListOfFileAsync(context, targetFile).ConfigureAwait(false);

                if (parentList == null || !parentList.ForceCheckout)
                {
                    return false;
                }

                if (targetFile.CheckOutType == CheckOutType.None)
                {
                    await targetFile.CheckoutAsync().ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task<IList> FindParentListOfFileAsync(PnPContext context, CoreFile targetFile)
        {
            string url = targetFile.ServerRelativeUrl;
            int lastSlash = url.LastIndexOf('/');

            if (lastSlash <= 0)
            {
                return null;
            }

            IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(url.Substring(0, lastSlash),
                f => f.ServerRelativeUrl).ConfigureAwait(false);

            return await FindParentListAsync(context, folder).ConfigureAwait(false);
        }

        /// <summary>
        /// The list a folder belongs to, or null when the folder is not inside one.
        /// </summary>
        private static async Task<IList> FindParentListAsync(PnPContext context, IFolder folder)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.ForceCheckout,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            return context.Web.Lists.AsRequested()
                .Where(l => folder.ServerRelativeUrl.StartsWith(l.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.RootFolder.ServerRelativeUrl.Length)
                .FirstOrDefault();
        }

        private static void RegisterFolderTokens(PnPContext context, TokenParser parser, IFolder folder, string webUrl)
        {
            string relative = folder.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/');

            parser.AddToken(new FileUniqueIdToken(context, relative, folder.UniqueId));
            parser.AddToken(new FileUniqueIdEncodedToken(context, relative, folder.UniqueId));
        }

        private static void RegisterFileTokens(PnPContext context, TokenParser parser, CoreFile targetFile, string webUrl)
        {
            string relative = targetFile.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/');

            parser.AddToken(new FileUniqueIdToken(context, relative, targetFile.UniqueId));
            parser.AddToken(new FileUniqueIdEncodedToken(context, relative, targetFile.UniqueId));

            try
            {
                IListItem item = targetFile.ListItemAllFields;
                if (item != null && item.Id > 0)
                {
                    parser.AddToken(new FileListItemIdToken(context, relative, item.Id));
                }
            }
            catch (ClientException)
            {
            }
        }

        private static bool HasSecurity(FileModel file)
        {
            return file.Security != null
                && (file.Security.ClearSubscopes
                    || file.Security.CopyRoleAssignments
                    || file.Security.RoleAssignments.Count > 0);
        }

        #endregion
    }
}
