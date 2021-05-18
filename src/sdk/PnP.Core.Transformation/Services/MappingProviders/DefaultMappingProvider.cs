using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    public class DefaultMappingProvider : IMappingProvider
    {
        public Task<MappingProviderOutput> MapAsync(MappingProviderInput input)
        {
            throw new NotImplementedException();
        }
    }
}
