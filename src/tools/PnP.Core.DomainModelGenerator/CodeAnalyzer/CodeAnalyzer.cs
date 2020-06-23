using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.M365.DomainModelGenerator.Mappings;
using System;
using System.Threading.Tasks;
using Mono.Cecil;
using System.Linq;
using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.CodeAnalyzer
{
    internal class CodeAnalyzer : ICodeAnalyzer
    {
        private readonly CodeAnalyzerOptions options;
        private readonly ILogger log;
        private AssemblyDefinition coreSDKAssembly;

        public CodeAnalyzer(
            IOptionsMonitor<CodeAnalyzerOptions> options,
            ILogger<CodeAnalyzer> logger,
            IServiceProvider serviceProvider)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options.CurrentValue;
            this.log = logger;
            coreSDKAssembly = AssemblyDefinition.ReadAssembly("PnP.Core.dll");
        }

        public async Task<Dictionary<string, AnalyzedModelType>> ProcessNamespace(UnifiedModelLocation location)
        {
            if (coreSDKAssembly == null)
            {
                throw new Exception("PnP.Core.dll was not loaded for analysis");
            }

            Dictionary<string, AnalyzedModelType> analyzedModelTypes = new Dictionary<string, AnalyzedModelType>();

            var typesToAnalyze = coreSDKAssembly.MainModule.Types.Where(p => p.Namespace == location.Namespace);
            foreach(var type in typesToAnalyze)
            {
                var name = type.Name.ToLower();
                if (type.IsInterface && type.IsPublic)
                {
                    name = name.Substring(1);
                    AddOrUpdate(analyzedModelTypes, true, name, type);
                }
                else
                {
                    AddOrUpdate(analyzedModelTypes, false, name, type);
                }
            }

            // Process collection information to make consumption of this data easier
            foreach(var analyzedModelType in analyzedModelTypes)
            {
                if (analyzedModelType.Value.Implementation != null)
                {
                    var attributes = analyzedModelType.Value.Implementation.CustomAttributes;
                    if (attributes.Any())
                    {
                        var sharePointAttributes = attributes.Where(p => p.AttributeType.Name == "SharePointTypeAttribute");
                        if (sharePointAttributes.Any())
                        {
                            foreach(var sharePointAttribute in sharePointAttributes)
                            {
                                // Type name is a constructor argument
                                var typeNameArgument = sharePointAttribute.ConstructorArguments.First().Value.ToString();
                                if (!analyzedModelType.Value.SPRestTypes.Contains(typeNameArgument))
                                {
                                    analyzedModelType.Value.SPRestTypes.Add(typeNameArgument);
                                }
                            }
                        }
                    }
                }
            }

            return analyzedModelTypes;
        }

        private void AddOrUpdate(Dictionary<string, AnalyzedModelType> analyzedModelTypes, bool isPublic, string name, TypeDefinition typeDefinition)
        {
            if (analyzedModelTypes.ContainsKey(name))
            {
                if (isPublic)
                {
                    analyzedModelTypes[name].Public = typeDefinition;
                }
                else
                {
                    analyzedModelTypes[name].Implementation = typeDefinition;
                }
            }
            else
            {
                AnalyzedModelType analyzedModel = new AnalyzedModelType
                {
                    Name = name,
                    Namespace = typeDefinition.Namespace
                };

                if (isPublic)
                {
                    analyzedModel.Public = typeDefinition;
                }
                else
                {
                    analyzedModel.Implementation = typeDefinition;
                }

                analyzedModelTypes.Add(name, analyzedModel);
            }
        }
    }
}
