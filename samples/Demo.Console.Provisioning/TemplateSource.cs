using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Connectors.OpenXML;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Demo.Console.Provisioning
{
    public sealed class TemplateSource
    {
        public const string SettingsFileName = "settings.json";

        private const string PackageTemplateName = "template.xml";

        public string Path { get; private set; }

        public string DisplayName { get; private set; }

        public ProvisioningTemplate Template { get; private set; }

        public ProvisioningHierarchy Hierarchy { get; private set; }

        public LookBookSettings Settings { get; private set; }

        public bool CreatesItsOwnSite =>
            Hierarchy != null && Hierarchy.Sequences.Any(s => s.SiteCollections.Any());

        public static bool IsPackage(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && path.EndsWith(".pnp", StringComparison.OrdinalIgnoreCase);
        }

        public static string Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string full = System.IO.Path.GetFullPath(path);

            if (System.IO.File.Exists(full))
            {
                return full;
            }

            if (!System.IO.Directory.Exists(full))
            {
                return null;
            }

            LookBookSettings settings = ReadSettings(full);

            if (!string.IsNullOrWhiteSpace(settings?.PackageFile))
            {
                string named = System.IO.Path.Combine(full, settings.PackageFile);

                if (System.IO.File.Exists(named))
                {
                    return named;
                }
            }

            string package = System.IO.Directory.GetFiles(full, "*.pnp").FirstOrDefault();

            if (package != null)
            {
                return package;
            }

            string sourceTemplate = System.IO.Path.Combine(full, "source", PackageTemplateName);

            if (System.IO.File.Exists(sourceTemplate))
            {
                return sourceTemplate;
            }

            string rootTemplate = System.IO.Path.Combine(full, PackageTemplateName);

            if (System.IO.File.Exists(rootTemplate))
            {
                return rootTemplate;
            }

            return System.IO.Directory.GetFiles(full, "*.xml").FirstOrDefault();
        }

        public static LookBookSettings ReadSettings(string folder)
        {
            try
            {
                string path = System.IO.Path.Combine(folder, SettingsFileName);

                if (!System.IO.File.Exists(path))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<LookBookSettings>(
                    System.IO.File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static TemplateSource Load(string pathOrFolder)
        {
            string resolved = Resolve(pathOrFolder);

            if (resolved == null)
            {
                throw new FileNotFoundException(
                    $"Nothing to apply was found at {pathOrFolder}. A folder needs a .pnp package, " +
                    $"a source/template.xml, or an .xml template in it.");
            }

            string folder = System.IO.Path.GetDirectoryName(resolved);

            var source = new TemplateSource
            {
                Path = resolved,
                DisplayName = System.IO.Directory.Exists(System.IO.Path.GetFullPath(pathOrFolder))
                    ? $"{new DirectoryInfo(System.IO.Path.GetFullPath(pathOrFolder)).Name} ({System.IO.Path.GetFileName(resolved)})"
                    : System.IO.Path.GetFileName(resolved),
                Settings = ReadSettings(folder) ?? ReadSettings(System.IO.Directory.GetParent(folder)?.FullName ?? folder),
            };

            if (IsPackage(resolved))
            {
                source.LoadFromPackage(resolved);
            }
            else
            {
                source.LoadFromXml(resolved, folder);
            }

            return source;
        }

        private void LoadFromPackage(string path)
        {
            var package = new OpenXMLConnector(
                System.IO.Path.GetFileName(path),
                new FileSystemConnector(System.IO.Path.GetDirectoryName(path), string.Empty),
                Environment.UserName,
                null,
                PackageTemplateName);

            string inner = string.IsNullOrEmpty(package.Info?.Properties?.TemplateFileName)
                ? PackageTemplateName
                : package.Info.Properties.TemplateFileName;

            var provider = new XMLOpenXMLTemplateProvider(package);

            Hierarchy = TryGetHierarchy(() => provider.GetHierarchy());
            Template = Hierarchy != null
                ? FirstTemplate(Hierarchy)
                : provider.GetTemplate(inner);

            if (Template != null)
            {
                Template.Connector = package;
            }

            if (Hierarchy != null)
            {
                Hierarchy.Connector = package;
            }
        }

        private void LoadFromXml(string path, string folder)
        {
            var connector = new FileSystemConnector(folder, string.Empty);
            var provider = new XMLFileSystemTemplateProvider(folder, string.Empty);

            string fileName = System.IO.Path.GetFileName(path);

            Hierarchy = TryGetHierarchy(() => provider.GetHierarchy(fileName));
            Template = Hierarchy != null
                ? FirstTemplate(Hierarchy)
                : provider.GetTemplate(fileName);

            if (Template != null)
            {
                Template.Connector = connector;
            }

            if (Hierarchy != null)
            {
                Hierarchy.Connector = connector;
            }
        }

        private static ProvisioningHierarchy TryGetHierarchy(Func<ProvisioningHierarchy> read)
        {
            try
            {
                ProvisioningHierarchy hierarchy = read();

                return hierarchy != null && hierarchy.Templates.Any() ? hierarchy : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static ProvisioningTemplate FirstTemplate(ProvisioningHierarchy hierarchy)
        {
            return hierarchy.Templates.FirstOrDefault();
        }

        public string SequenceId()
        {
            return Hierarchy?.Sequences.FirstOrDefault()?.ID;
        }

        public IEnumerable<string> ParameterNames()
        {
            return Template?.Parameters?.Keys ?? Enumerable.Empty<string>();
        }
    }

    public sealed class LookBookSettings
    {
        public string PackageFile { get; set; }

        public string Abstract { get; set; }

        public string MatchingSiteBaseTemplateId { get; set; }

        public LookBookMetadata Metadata { get; set; }
    }

    public sealed class LookBookMetadata
    {
        public List<LookBookProperty> Properties { get; set; } = new List<LookBookProperty>();
    }

    public sealed class LookBookProperty
    {
        public string Name { get; set; }

        public string Caption { get; set; }

        public string Description { get; set; }
    }
}
