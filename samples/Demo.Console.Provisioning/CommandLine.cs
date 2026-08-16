using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.Console.Provisioning
{
    public sealed class CommandLine
    {
        public bool IsApply { get; private set; }

        public bool IsExtract { get; private set; }

        public bool ShowHelp { get; private set; }

        public bool Verbose { get; private set; }

        public bool IsValid { get; private set; } = true;

        public Uri SiteUrl { get; private set; }

        public string TemplatePath { get; private set; }

        public bool IncludeItems { get; private set; }

        public List<string> ItemLists { get; } = new List<string>();

        public bool IncludePages { get; private set; }

        public bool IncludeHiddenLists { get; private set; }

        public static CommandLine Parse(string[] args)
        {
            var command = new CommandLine();

            if (args == null || args.Length == 0)
            {
                return command;
            }

            var positional = new List<string>();
            bool extractVerb = false;

            foreach (string arg in args)
            {
                string lower = arg.ToLowerInvariant();

                if (lower.StartsWith("--items=", StringComparison.Ordinal))
                {
                    command.IncludeItems = true;
                    command.ItemLists.AddRange(arg.Substring("--items=".Length)
                        .Split(',')
                        .Select(t => t.Trim())
                        .Where(t => t.Length > 0));
                    continue;
                }

                switch (lower)
                {
                    case "-h":
                    case "--help":
                    case "-?":
                    case "/?":
                        command.ShowHelp = true;
                        break;

                    case "-v":
                    case "--verbose":
                        command.Verbose = true;
                        break;

                    case "--items":
                        command.IncludeItems = true;
                        break;

                    case "--pages":
                        command.IncludePages = true;
                        break;

                    case "--hidden-lists":
                        command.IncludeHiddenLists = true;
                        break;

                    case "apply":
                        break;

                    case "extract":
                        extractVerb = true;
                        break;

                    default:
                        if (arg.StartsWith("-", StringComparison.Ordinal))
                        {
                            System.Console.Error.WriteLine($"Unknown option '{arg}'.");
                            command.ShowHelp = true;
                            command.IsValid = false;
                        }
                        else
                        {
                            positional.Add(arg);
                        }

                        break;
                }
            }

            if (command.ShowHelp)
            {
                return command;
            }

            string what = extractVerb ? "an output file path" : "a template path";

            if (positional.Count != 2)
            {
                System.Console.Error.WriteLine(positional.Count < 2
                    ? $"A site url and {what} are both needed."
                    : $"Expected a site url and {what}, got {positional.Count} arguments.");

                command.ShowHelp = true;
                command.IsValid = false;
                return command;
            }

            if (!Uri.TryCreate(positional[0], UriKind.Absolute, out Uri siteUrl)
                || (siteUrl.Scheme != Uri.UriSchemeHttps && siteUrl.Scheme != Uri.UriSchemeHttp))
            {
                System.Console.Error.WriteLine($"'{positional[0]}' is not a site url.");

                command.ShowHelp = true;
                command.IsValid = false;
                return command;
            }

            // The content options only mean something on an extract. Silently ignoring them on an
            // apply would let a mistyped command look like it did what was asked.
            if (!extractVerb && (command.IncludeItems || command.IncludePages || command.IncludeHiddenLists))
            {
                System.Console.Error.WriteLine("--items, --pages and --hidden-lists apply to 'extract', not 'apply'.");

                command.ShowHelp = true;
                command.IsValid = false;
                return command;
            }

            command.IsExtract = extractVerb;
            command.IsApply = !extractVerb;
            command.SiteUrl = siteUrl;
            command.TemplatePath = positional[1];

            return command;
        }

        public static void WriteUsage()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Demo.Console.Provisioning");
            System.Console.WriteLine();
            System.Console.WriteLine("  dotnet run                                        interactive menu");
            System.Console.WriteLine("  dotnet run -- <site-url> <template.xml>            apply a template and exit");
            System.Console.WriteLine("  dotnet run -- apply <site-url> <template.xml>      the same, spelled out");
            System.Console.WriteLine("  dotnet run -- extract <site-url> <output.xml>      extract a template and exit");
            System.Console.WriteLine();
            System.Console.WriteLine("Extract options (structure only, unless you ask for more):");
            System.Console.WriteLine("  --items              include the items of every list on the site");
            System.Console.WriteLine("  --items=A,B          include the items of these lists only");
            System.Console.WriteLine("  --pages              include the site's client side pages and their contents");
            System.Console.WriteLine("  --hidden-lists       include hidden lists in the structure");
            System.Console.WriteLine();
            System.Console.WriteLine("  Document libraries are skipped by --items and their files are not");
            System.Console.WriteLine("  exported: the engine has no file extraction yet.");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("  -v, --verbose   log the SDK's own requests as well");
            System.Console.WriteLine("  -h, --help      this text");
            System.Console.WriteLine();
            System.Console.WriteLine("Exit codes:");
            System.Console.WriteLine("  0  done, nothing reported");
            System.Console.WriteLine("  2  done, but warnings or errors were reported");
            System.Console.WriteLine("  1  failed");
            System.Console.WriteLine();
            System.Console.WriteLine("Examples:");
            System.Console.WriteLine("  dotnet run -- https://contoso.sharepoint.com/sites/target Templates/site.xml");
            System.Console.WriteLine("  dotnet run -- extract https://contoso.sharepoint.com/sites/src out.xml --items --pages");
            System.Console.WriteLine("  dotnet run -- extract https://contoso.sharepoint.com/sites/src out.xml --items=\"Tasks,Announcements\"");
            System.Console.WriteLine();
        }
    }
}
