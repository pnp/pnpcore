namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines the settings for the CodeGenerator service
    /// </summary>
    public class CodeGeneratorOptions
    {
        /// <summary>
        /// Defines the root path of the output files
        /// </summary>
        public string OutputFilesRootPath { get; set; }

        /// <summary>
        /// Defines the base part of the output namespace for generated types
        /// </summary>
        public string BaseNamespace { get; set; }
    }
}
