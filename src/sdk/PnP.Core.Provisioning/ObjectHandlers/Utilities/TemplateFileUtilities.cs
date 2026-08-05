using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DirectoryModel = PnP.Core.Provisioning.Model.Directory;
using FileModel = PnP.Core.Provisioning.Model.File;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Reads the files a template ships, and expands a <c>&lt;pnp:Directory&gt;</c> into the
    /// individual files it stands for.
    /// </summary>
    internal static class TemplateFileUtilities
    {
        /// <summary>
        /// Opens a file carried by the template's connector.
        /// </summary>
        /// <returns>The file's bytes, or null when the connector has no such file.</returns>
        internal static byte[] TryGetFileBytes(ProvisioningTemplate template, string source)
        {
            return TryGetFileBytes(template?.Connector, source);
        }

        /// <summary>
        /// Opens a file from a connector directly.
        /// </summary>
        internal static byte[] TryGetFileBytes(FileConnectorBase connector, string source)
        {
            if (connector == null || string.IsNullOrEmpty(source))
            {
                return null;
            }

            try
            {
                return ConnectorFileHelper.GetFileBytes(connector, source);
            }
            catch (Exception)
            {
                // ConnectorFileHelper throws when the file is absent; the caller reports that in
                // terms of the template rather than of the connector.
                return null;
            }
        }

        /// <summary>
        /// Expands every directory in the template into the files it contains.
        /// </summary>
        internal static List<FileModel> ExpandDirectories(ProvisioningTemplate template)
        {
            var files = new List<FileModel>();

            foreach (DirectoryModel directory in template.Directories)
            {
                Dictionary<string, Dictionary<string, string>> metadata = ReadMetadataMapping(template, directory);
                Collect(template, directory, directory.Src, directory.Folder, metadata, files);
            }

            return files;
        }

        /// <summary>
        /// Walks one directory, and its children when the element asks for it.
        /// </summary>
        private static void Collect(ProvisioningTemplate template, DirectoryModel directory, string source,
            string targetFolder, Dictionary<string, Dictionary<string, string>> metadata, List<FileModel> collected)
        {
            string container = string.IsNullOrEmpty(template.Connector.GetContainer())
                ? source
                : Path.Combine(template.Connector.GetContainer(), source);

            List<string> names = template.Connector.GetFiles(container)?.ToList() ?? new List<string>();

            names = ApplyExtensionFilters(directory, names);

            foreach (string name in names)
            {
                string path = Path.Combine(source, name);

                metadata.TryGetValue(path, out Dictionary<string, string> properties);

                collected.Add(new FileModel(
                    path,
                    targetFolder,
                    directory.Overwrite,
                    // Web parts cannot be expressed per file when the files come from a directory:
                    // the element names a folder, not the individual pages inside it.
                    null,
                    properties,
                    directory.Security,
                    directory.Level));
            }

            if (!directory.Recursive)
            {
                return;
            }

            foreach (string child in template.Connector.GetFolders(container) ?? Enumerable.Empty<string>())
            {
                Collect(template, directory, Path.Combine(source, child), Path.Combine(targetFolder, child),
                    metadata, collected);
            }
        }

        private static List<string> ApplyExtensionFilters(DirectoryModel directory, List<string> names)
        {
            if (!string.IsNullOrEmpty(directory.IncludedExtensions) && directory.IncludedExtensions != "*.*")
            {
                HashSet<string> included = SplitExtensions(directory.IncludedExtensions);
                names = names.Where(n => included.Contains(ExtensionPatternOf(n))).ToList();
            }

            if (!string.IsNullOrEmpty(directory.ExcludedExtensions))
            {
                HashSet<string> excluded = SplitExtensions(directory.ExcludedExtensions);
                names = names.Where(n => !excluded.Contains(ExtensionPatternOf(n))).ToList();
            }

            return names;
        }

        // The schema writes extensions as glob patterns - "*.aspx,*.js" - so the comparison is
        // against that shape rather than against a bare extension.
        private static HashSet<string> SplitExtensions(string value)
        {
            return new HashSet<string>(
                value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim().ToLowerInvariant()),
                StringComparer.Ordinal);
        }

        private static string ExtensionPatternOf(string fileName)
        {
            return $"*{Path.GetExtension(fileName).ToLowerInvariant()}";
        }

        /// <summary>
        /// Reads the optional JSON file that gives per-file column values.
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> ReadMetadataMapping(
            ProvisioningTemplate template, DirectoryModel directory)
        {
            var empty = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(directory.MetadataMappingFile))
            {
                return empty;
            }

            byte[] bytes = TryGetFileBytes(template, directory.MetadataMappingFile);
            if (bytes == null)
            {
                return empty;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(bytes);

                return parsed == null
                    ? empty
                    : new Dictionary<string, Dictionary<string, string>>(parsed, StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return empty;
            }
        }
    }
}
