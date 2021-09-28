using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Transformation.SharePoint.Functions
{
    /// <summary>
    /// Class that executes functions and selectors defined in the mapping 
    /// </summary>
    public class FunctionProcessor : BaseFunctionProcessor
    {
        private WebPartMapping pageTransformation;
        //private List<AddOnType> addOnTypes;

        private ILogger<FunctionProcessor> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly SharePointFunctionsService sharePointFunctionsService;
        private readonly IOptions<SharePointTransformationOptions> options;

        #region Construction

        /// <summary>
        /// Instantiates the function processor. Also loads the defined add-ons
        /// </summary>
        public FunctionProcessor(ILogger<FunctionProcessor> logger,
            IOptions<SharePointTransformationOptions> options,
            SharePointFunctionsService sharePointFunctionsService,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.sharePointFunctionsService = sharePointFunctionsService ?? throw new ArgumentNullException(nameof(sharePointFunctionsService));
            this.serviceProvider = serviceProvider;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the Function Processor with a custom transformation context and a CSOM source context
        /// </summary>
        /// <param name="pageTransformationContext">The current page transformation context</param>
        /// <param name="sourceContext">The CSOM source context</param>
        public void Init(PageTransformationContext pageTransformationContext, ClientContext sourceContext
            //, WebPartMapping pageTransformation
            )
        {
            // Configure the SharePoint Function service instance
            this.sharePointFunctionsService.PageTransformationContext = pageTransformationContext;
            this.sharePointFunctionsService.SourceContext = sourceContext;

            // NOTE: We removed support for addons
        }

        /// <summary>
        /// Executes the defined functions and selectors in the provided web part
        /// </summary>
        /// <param name="webPartData">Web Part mapping data</param>
        /// <param name="webPart">Definition of the web part to be transformed</param>
        /// <returns>The ouput of the mapping selector if there was one executed, null otherwise</returns>
        public string Process(ref WebPart webPartData, WebPartEntity webPart)
        {
            // First process the transform functions
            foreach (var property in webPartData.Properties.ToList())
            {
                // No function defined, so skip
                if (string.IsNullOrEmpty(property.Functions))
                {
                    continue;
                }

                // Multiple functions can be specified using ; as delimiter
                var functionsToProcess = property.Functions.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                // Use reflection to run the functions
                ExecutePropertyFunctions(functionsToProcess, webPartData, webPart, property);
            }

            // Process the selector function
            if (!string.IsNullOrEmpty(webPartData.Mappings.Selector))
            {
                FunctionDefinition functionDefinition = ParseFunctionDefinition(webPartData.Mappings.Selector, null, webPartData, webPart);

                // Execute function
                MethodInfo methodInfo = null;
                object functionClassInstance = null;

                // NOTE: We removed support for addons
                methodInfo = this.sharePointFunctionsService.GetType().GetMethod(functionDefinition.Name);
                    functionClassInstance = this.sharePointFunctionsService;

                if (methodInfo != null)
                {
                    // Execute the selector
                    object result = ExecuteMethod(functionClassInstance, functionDefinition, methodInfo);
                    return result.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Executes the defined functions and selectors in the provided web part
        /// </summary>
        /// <param name="webPartData">Web Part mapping data</param>
        /// <param name="webPart">Definition of the web part to be transformed</param>
        /// <param name="webPartMapping"></param>
        /// <returns>The ouput of the mapping selector if there was one executed, null otherwise</returns>
        public void ProcessMappingFunctions(ref WebPart webPartData, WebPartEntity webPart, Mapping webPartMapping)
        {
            // No function defined, so skip
            if (string.IsNullOrEmpty(webPartMapping.Functions))
            {
                return;
            }

            // Multiple functions can be specified using ; as delimiter
            var functionsToProcess = webPartMapping.Functions.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            // Use reflection to run the functions
            ExecutePropertyFunctions(functionsToProcess, webPartData, webPart, null);
        }
        #endregion

        #region Helper methods
        private void ExecutePropertyFunctions(string[] functionsToProcess, WebPart webPartData, WebPartEntity webPart, Property property)
        {
            // Process each function
            foreach (var function in functionsToProcess)
            {
                // Parse the function
                FunctionDefinition functionDefinition = ParseFunctionDefinition(function, property, webPartData, webPart);

                // Execute function
                MethodInfo methodInfo = null;
                object functionClassInstance = null;

                // NOTE: We removed support for addons
                methodInfo = this.sharePointFunctionsService.GetType().GetMethod(functionDefinition.Name);
                    functionClassInstance = this.sharePointFunctionsService;

                if (methodInfo != null)
                {
                    // Execute the function
                    object result = ExecuteMethod(functionClassInstance, functionDefinition, methodInfo);

                    if (result is string || result is bool)
                    {
                        // Update the existing web part property or add a new one
                        if (webPart.Properties.Keys.Contains<string>(functionDefinition.Output.Name))
                        {
                            webPart.Properties[functionDefinition.Output.Name] = (result is bool) ? result.ToString().ToLower() : result.ToString();
                        }
                        else
                        {
                            webPart.Properties.Add(functionDefinition.Output.Name, (result is bool) ? result.ToString().ToLower() : result.ToString());
                        }

                        // Add results from the function evaluation to the web part properties mapping data so that upcoming functions can use these new properties
                        if (Array.FindIndex<Property>(webPartData.Properties, p => p.Name.Equals(functionDefinition.Output.Name, StringComparison.InvariantCultureIgnoreCase)) < 0)
                        {
                            UpdateWebPartDataProperties(webPartData, functionDefinition.Output.Name);
                        }
                    }
                    else if (result is Dictionary<string, string>)
                    {
                        if (result != null)
                        {
                            var parameters = result as Dictionary<string, string>;
                            foreach (var param in parameters)
                            {
                                // Update the existing web part property or add a new one
                                if (webPart.Properties.Keys.Contains<string>(param.Key))
                                {
                                    webPart.Properties[param.Key] = result.ToString();
                                }
                                else
                                {
                                    webPart.Properties.Add(param.Key, param.Value);
                                }

                                // Add results from the function evaluation to the web part properties mapping data so that upcoming functions can use these new properties
                                if (Array.FindIndex<Property>(webPartData.Properties, p => p.Name.Equals(param.Key, StringComparison.InvariantCultureIgnoreCase)) < 0)
                                {
                                    UpdateWebPartDataProperties(webPartData, param.Key);
                                }
                            }
                        }
                    }
                }

            }
        }

        private static void UpdateWebPartDataProperties(WebPart webPartData, string functionDefinitionName)
        {
            List<Property> tempList = new List<Property>();
            tempList.AddRange(webPartData.Properties);

            tempList.Add(new Property()
            {
                Functions = "",
                Name = functionDefinitionName,
                Type = PropertyType.@string
            });

            webPartData.Properties = tempList.ToArray();
        }

        private static FunctionDefinition ParseFunctionDefinition(string function, Property property, WebPart webPartData, WebPartEntity webPart)
        {
            // Supported function syntax: 
            // - EncodeGuid()
            // - MyLib.EncodeGuid()
            // - EncodeGuid({ListId})
            // - EncodeGuid({ListId}, {Param2})
            // - StaticString('a string')
            // - {ViewId} = EncodeGuid()
            // - {ViewId} = EncodeGuid({ListId})
            // - {ViewId} = MyLib.EncodeGuid({ListId})
            // - {ViewId} = EncodeGuid({ListId}, {Param2})

            FunctionDefinition def = new FunctionDefinition();

            // Let's grab 'static' parameters and replace them with a simple value, otherwise the parsing can go wrong due to special characters inside the static string
            Dictionary<string, string> staticParameters = null;
            if (function.IndexOf("'") > 0)
            {
                staticParameters = new Dictionary<string, string>();

                // Grab '' enclosed strings
                var regex = new Regex(@"('(?:[^'\\]|(?:\\\\)|(?:\\\\)*\\.{1})*')");
                var matches = regex.Matches(function);

                int staticReplacement = 0;
                foreach (var match in matches)
                {
                    staticParameters.Add($"'StaticParameter{staticReplacement}'", match.ToString());
                    function = function.Replace(match.ToString(), $"'StaticParameter{staticReplacement}'");
                }
            }

            // Set the output parameter
            string functionString = null;
            if (function.IndexOf("=") > 0)
            {
                var split = function.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                FunctionParameter output = new FunctionParameter()
                {
                    Name = split[0].Replace("{", "").Replace("}", "").Trim(),
                    Type = FunctionType.String
                };

                def.Output = output;
                functionString = split[1].Trim();
            }
            else
            {
                FunctionParameter output = new FunctionParameter()
                {
                    Name = property != null ? property.Name : "SelectedMapping",
                    Type = FunctionType.String
                };

                def.Output = output;
                functionString = function.Trim();
            }

            // Analyze the fuction
            string functionName = functionString.Substring(0, functionString.IndexOf("("));
            if (functionName.IndexOf(".") > -1)
            {
                // This is a custom function
                def.AddOn = functionName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
                def.Name = functionName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            else
            {
                // this is an BuiltIn function
                def.AddOn = "";
                def.Name = functionString.Substring(0, functionString.IndexOf("("));
            }
            
            def.Input = new List<FunctionParameter>();

            // Analyze the function parameters
            int staticCounter = 0;
            var functionParameters = functionString.Substring(functionString.IndexOf("(") + 1).Replace(")", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var functionParameter in functionParameters)
            {
                FunctionParameter input = new FunctionParameter();
                if (functionParameter.Contains("{") && functionParameter.Contains("}"))
                {
                    input.Name = functionParameter.Replace("{", "").Replace("}", "").Trim();
                }
                else if (functionParameter.Contains("'"))
                {
                    input.IsStatic = true;
                    input.Name = $"Static_{staticCounter}";

                    if (functionParameter.StartsWith("'StaticParameter"))
                    {
                        if (staticParameters.TryGetValue(functionParameter, out string staticParameterValue))
                        {
                            input.Value = staticParameterValue.Replace("'", "");
                        }
                    }
                    else
                    {
                        input.Value = functionParameter.Replace("'", "");
                    }

                    staticCounter++;
                }
                else
                {
                    input.Name = functionParameter.Trim();
                }

                // Populate the function parameter with a value coming from the analyzed web part
                var wpProp = webPartData.Properties?.FirstOrDefault(p => p.Name.Equals(input.Name, StringComparison.CurrentCultureIgnoreCase));
                if (wpProp != null)
                {
                    // Map types used in the model to types used in function processor
                    input.Type = MapType(wpProp.Type.ToString());
                    if (!input.IsStatic)
                    {
                        var wpInstanceProp = webPart.Properties.FirstOrDefault(p => p.Key.Equals(input.Name, StringComparison.CurrentCultureIgnoreCase));
                        input.Value = wpInstanceProp.Value;
                    }
                    def.Input.Add(input);
                }
                else
                {
                    // For add-in parts we've dynamically loaded all web part properties. These properties are typically not defined 
                    // in the mapping as they differ per add-in part and we only have one ClientWebPart in our mapping. Therefore we
                    // perform an additional validation with the loaded web part properties
                    if (webPartData.Type.GetTypeShort() == WebParts.Client.GetTypeShort())
                    {
                        if (webPart.Properties.ContainsKey(input.Name))
                        {
                            // We can't know the type of these dynamic properties, hence they default to string
                            input.Type = MapType("string");
                            input.Value = webPart.Properties[input.Name];
                            def.Input.Add(input);
                        }
                        else
                        {
                            // Add with empty string as value. Since we can only have one ClientWebPart mapping it's valid for a parameter to be not available.
                            input.Type = MapType("string");
                            input.Value = "";
                            def.Input.Add(input);
                        }
                    }
                    else
                    {
                        throw new Exception($"Parameter {input.Name} was used but is not listed as a web part property that can be used.");
                    }
                }
            }

            return def;
        }
        #endregion

    }
}
