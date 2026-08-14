using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.Console.Provisioning
{
    public sealed class CommandLine
    {
        public bool IsApply { get; private set; }

        public bool ShowHelp { get; private set; }

        public bool Verbose { get; private set; }

        public bool IsValid { get; private set; } = true;

        public Uri SiteUrl { get; private set; }

        public string TemplatePath { get; private set; }

        public static CommandLine Parse(string[] args)
        {
            var command = new CommandLine();

            if (args == null || args.Length == 0)
            {
                return command;
            }

            var positional = new List<string>();

            foreach (string arg in args)
            {
                switch (arg.ToLowerInvariant())
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

                    case "apply":
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

            if (positional.Count != 2)
            {
                System.Console.Error.WriteLine(positional.Count < 2
                    ? "A site url and a template path are both needed."
                    : $"Expected a site url and a template path, got {positional.Count} arguments.");

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

            command.IsApply = true;
            command.SiteUrl = siteUrl;
            command.TemplatePath = positional[1];

            return command;
        }

        public static void WriteUsage()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Demo.Console.Provisioning");
            System.Console.WriteLine();
            System.Console.WriteLine("  dotnet run                                     interactive menu");
            System.Console.WriteLine("  dotnet run -- <site-url> <template.xml>         apply a template and exit");
            System.Console.WriteLine("  dotnet run -- apply <site-url> <template.xml>   the same, spelled out");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("  -v, --verbose   log the SDK's own requests as well");
            System.Console.WriteLine("  -h, --help      this text");
            System.Console.WriteLine();
            System.Console.WriteLine("Exit codes:");
            System.Console.WriteLine("  0  applied, nothing reported");
            System.Console.WriteLine("  2  applied, but warnings or errors were reported");
            System.Console.WriteLine("  1  failed");
            System.Console.WriteLine();
            System.Console.WriteLine("Example:");
            System.Console.WriteLine("  dotnet run -- https://contoso.sharepoint.com/sites/target Templates/site.xml");
            System.Console.WriteLine();
        }
    }
}
