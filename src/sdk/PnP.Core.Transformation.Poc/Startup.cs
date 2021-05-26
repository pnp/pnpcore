using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using PnP.Core.Transformation.Poc;
using PnP.Core.Transformation.Poc.Implementations;

[assembly: FunctionsStartup(typeof(Startup))]

namespace PnP.Core.Transformation.Poc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(p =>
            {
                var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                return account.CreateCloudTableClient();
            });

            builder.Services.AddPnPCore();
            builder.Services.AddPnPCoreAuthentication();

            builder.Services.AddPnPSharePointTransformation()
                .WithTransformationStateManager<AzureTransformationStateManager>()
                .WithTransformationExecutor<AzureQueueTransformationExecutor>();
        }
    }
}