using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Mock.SharePoint
{
    public class MockPnPContext : PnPContext
    {
        internal MockPnPContext(MockHttpClient client) : base(NullLogger.Instance,
                new MockAuthProvider(), 
                new SharePointRestClient(client, NullLogger.Instance as ILogger<SharePointRestClient>, Options.Create(new PnPGlobalSettingsOptions())), 
                new MicrosoftGraphClient(client, NullLogger.Instance as ILogger<MicrosoftGraphClient>, Options.Create(new PnPGlobalSettingsOptions())), 
                new PnPContextFactoryOptions(), 
                new PnPGlobalSettingsOptions(),
                new TelemetryManager(new PnPGlobalSettingsOptions()))
        {
        }
    }
}
