using System;

namespace PnP.Core.Transformation.SharePoint.Functions
{
    /// <summary>
    /// Base attribute to document a function or selector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class BaseFunctionDocumentationAttribute : Attribute
    {
        /// <summary>
        /// Description of the function
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Function example
        /// </summary>
        public string Example { get; set; }
    }

    /// <summary>
    /// Function documentation attribute class
    /// </summary>
    public sealed class FunctionDocumentationAttribute: BaseFunctionDocumentationAttribute
    {
    }

    /// <summary>
    /// Selector documentation attribute class
    /// </summary>
    public sealed class SelectorDocumentationAttribute : BaseFunctionDocumentationAttribute
    {
    }

    /// <summary>
    /// Base parameter to document a function/selector parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ParameterDocumentationAttribute: Attribute
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parameter description
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Input parameter documentation attribute
    /// </summary>
    public sealed class InputDocumentationAttribute: ParameterDocumentationAttribute
    {

    }

    /// <summary>
    /// Output parameter documentation attribute
    /// </summary>
    public sealed class OutputDocumentationAttribute : ParameterDocumentationAttribute
    {

    }
}
