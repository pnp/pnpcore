using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace PnP.Core.Provisioning.Test.Offline.Csom
{
    internal static class CsomSnapshot
    {
        internal const string UpdateVariable = "PNPCORE_UPDATE_CSOM_SNAPSHOTS";

        private const string ProjectFileName = "PnP.Core.Provisioning.Test.csproj";

        internal static void Verify(string name, string actual, string extension = ".xml")
        {
            string fileName = name + extension;

            if (string.Equals(Environment.GetEnvironmentVariable(UpdateVariable), "1", StringComparison.Ordinal))
            {
                string target = Path.Combine(FindProjectFolder(), "Offline", "Csom", "Snapshots", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.WriteAllText(target, actual, new UTF8Encoding(false));
                return;
            }

            string path = Path.Combine(AppContext.BaseDirectory, "Offline", "Csom", "Snapshots", fileName);
            Assert.IsTrue(File.Exists(path),
                $"There is no snapshot for '{name}'. Generate it by running the tests once with {UpdateVariable}=1, then rebuild.");

            string expected = Normalize(File.ReadAllText(path));
            string normalizedActual = Normalize(actual);

            if (!string.Equals(expected, normalizedActual, StringComparison.Ordinal))
            {
                int index = FirstDifference(expected, normalizedActual);
                Assert.Fail(
                    $"'{name}' no longer matches its snapshot. First difference at character {index}.{Environment.NewLine}" +
                    $"Expected: ...{Excerpt(expected, index)}...{Environment.NewLine}" +
                    $"Actual:   ...{Excerpt(normalizedActual, index)}...");
            }
        }

        private static string Normalize(string value)
        {
            return value.Replace("\r\n", "\n");
        }

        private static int FirstDifference(string expected, string actual)
        {
            int length = Math.Min(expected.Length, actual.Length);

            for (int i = 0; i < length; i++)
            {
                if (expected[i] != actual[i])
                {
                    return i;
                }
            }

            return length;
        }

        private static string Excerpt(string value, int index)
        {
            int start = Math.Max(0, index - 60);
            int length = Math.Min(value.Length - start, 160);
            return length > 0 ? value.Substring(start, length) : string.Empty;
        }

        private static string FindProjectFolder()
        {
            DirectoryInfo folder = new DirectoryInfo(AppContext.BaseDirectory);

            while (folder != null)
            {
                if (File.Exists(Path.Combine(folder.FullName, ProjectFileName)))
                {
                    return folder.FullName;
                }

                folder = folder.Parent;
            }

            throw new InvalidOperationException($"Could not find {ProjectFileName} above {AppContext.BaseDirectory}.");
        }
    }
}
