using PnP.M365.DomainModelGenerator.Mappings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator.CodeAnalyzer
{
    internal interface ICodeAnalyzer
    {
        Task<Dictionary<string, AnalyzedModelType>> ProcessNamespace(UnifiedModelLocation location);
    }
}
