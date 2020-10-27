using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.M365.DomainModelGenerator.CodeAnalyzer;
using System.Threading.Tasks;


namespace PnP.M365.DomainModelGenerator
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
            .UseEnvironment("bertonline")
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

                services.AddPnPCoreAuthentication(
                      options => {
                                // Configure an Authentication Provider relying on the interactive authentication
                                options.Credentials.Configurations.Add("credman",
                                    new PnPCoreAuthenticationCredentialConfigurationOptions
                                    {
                                        CredentialManager = new PnPCoreAuthenticationCredentialManagerOptions()
                                        {
                                            CredentialManagerName = metadataSettings.CredentialManager
                                        }
                                    });

                                // Configure the default authentication provider
                                options.Credentials.DefaultConfiguration = "credman";
                            }
                  )
                .AddEdmxProcessor(options =>
                {
                    options.MappingFilePath = metadataSettings.MappingFilePath;
                    options.SPMappingFilePath = metadataSettings.SPMappingFilePath;
                    options.GraphMappingFilePath = metadataSettings.GraphMappingFilePath;

                    options.EdmxProviders.Add(new EdmxProviderOptions
                    {
                        EdmxProviderName = "SPO",
                        MetadataUri = metadataSettings.SPRestMetadataUri,
                        AuthenticationProviderName = "credman",
                    });
                    options.EdmxProviders.Add(new EdmxProviderOptions
                    {
                        EdmxProviderName = "MSGraph",
                        MetadataUri = metadataSettings.GraphMetadataUri,
                    });
                })
                .AddCodeAnalyzer(options => { })
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
