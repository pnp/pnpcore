using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Threading.Tasks;


namespace PnP.M365.DomainModelGenerator
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddEventSourceLogger();
                logging.AddConsole();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                var metadataSettings = new MetadataSettings();
                hostingContext.Configuration.Bind("MetadataSettings", metadataSettings);

                var generatorSettings = new GeneratorSettings();
                hostingContext.Configuration.Bind("GeneratorSettings", generatorSettings);

                services
                .AddAuthenticationProviderFactory(options =>
                {
                    options.Configurations.Add(new OAuthCredentialManagerConfiguration
                    {
                        Name = "CredentialManagerAuthentication",
                        CredentialManagerName = metadataSettings.CredentialManager,
                    });

                    options.DefaultConfiguration = "CredentialManagerAuthentication";
                })
                .AddEdmxProcessor(options =>
                {
                    options.MappingFilePath = metadataSettings.MappingFilePath;

                    options.EdmxProviders.Add(new EdmxProviderOptions
                    {
                        EdmxProviderName = "SPO",
                        MetadataUri = metadataSettings.SPRestMetadataUri,
                        AuthenticationProviderName = "CredentialManagerAuthentication",
                    });
                    options.EdmxProviders.Add(new EdmxProviderOptions
                    {
                        EdmxProviderName = "MSGraph",
                        MetadataUri = metadataSettings.GraphMetadataUri,
                    });
                })
                .AddCodeGenerator(options =>
                {
                    options.OutputFilesRootPath = generatorSettings.OutputFilesRootPath;
                    options.BaseNamespace = generatorSettings.BaseNamespace;
                });
            })
            // Let the builder know we're running in a console
            .UseConsoleLifetime()
            // Add services to the container
            .Build();

            await host.StartAsync();

            using (var scope = host.Services.CreateScope())
            {
                var edmxProcessor = scope.ServiceProvider.GetRequiredService<IEdmxProcessor>();
                var model = await edmxProcessor.ProcessAsync();

                var codeGenerator = scope.ServiceProvider.GetRequiredService<ICodeGenerator>();
                await codeGenerator.ProcessAsync(model);
            }

            host.Dispose();
        }
    }
}
