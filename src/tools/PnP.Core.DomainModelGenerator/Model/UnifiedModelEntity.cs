using PnP.M365.DomainModelGenerator.CodeAnalyzer;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace PnP.M365.DomainModelGenerator
{
    internal class UnifiedModelEntity
    {
        public string TypeName { get; set; }

        /// <summary>
        /// The name of the type that will be generated
        /// </summary>
        public string SPRestType { get; set; }

        /// <summary>
        /// Base type of this type 
        /// </summary>
        public string BaseType { get; set; }


        public string SchemaNamespace { get; set; }

        /// <summary>
        /// The namespace of the type that will be generated
        /// </summary>
        public string Namespace { get; set; }

        public string Folder { get; set; }

        /// <summary>
        /// Are we skipping this entity during code generation time?
        /// </summary>
        public bool Skip { get; set; }

        /// <summary>
        /// Equivalent model coming from the code base analysis
        /// </summary>
        public AnalyzedModelType AnalyzedModelType { get; set; }

        /// <summary>
        /// Propertoes for this entity
        /// </summary>
        public List<UnifiedModelProperty> Properties { get; set; } = new List<UnifiedModelProperty>();
    }
}
