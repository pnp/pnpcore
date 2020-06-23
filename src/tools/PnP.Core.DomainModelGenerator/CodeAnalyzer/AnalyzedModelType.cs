using Mono.Cecil;
using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.CodeAnalyzer
{
    internal class AnalyzedModelType
    {
        public string Namespace { get; set; }

        public string Name { get; set; }

        public List<string> SPRestTypes { get; set; } = new List<string>();

        public TypeDefinition Public { get; set; }

        public TypeDefinition Implementation { get; set; }
    }
}
