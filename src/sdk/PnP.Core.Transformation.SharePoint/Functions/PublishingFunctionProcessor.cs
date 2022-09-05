using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint.Extensions;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PnP.Core.Transformation.SharePoint.Functions
{
    /// <summary>
    /// Function processor for publishing page transformation
    /// </summary>
    public class PublishingFunctionProcessor : BaseFunctionProcessor
    {
        /// <summary>
        /// Field types
        /// </summary>
#pragma warning disable CA1720
        public enum FieldType
        {
            /// <summary>
            /// String type
            /// </summary>
            String = 0,

            /// <summary>
            /// Bool type
            /// </summary>
            Bool = 1,

            /// <summary>
            /// Guid type
            /// </summary>
            Guid = 2,

            /// <summary>
            /// Integer type
            /// </summary>
            Integer = 3,

            /// <summary>
            /// Datetime type
            /// </summary>
            DateTime = 4,

            /// <summary>
            /// User type
            /// </summary>
            User = 5,
        }
#pragma warning restore CA1720

        /// <summary>
        /// Name token
        /// </summary>
        public string NameAttributeToken
        {
            get { return "{@Name}"; }
        }

        //private PublishingPageTransformation publishingPageTransformation;
        //private ClientContext sourceClientContext;
        private ListItem page;

        private ILogger<FunctionProcessor> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly SharePointPublishingFunctionsService sharePointPublishingFunctionsService;
        private readonly IOptions<SharePointTransformationOptions> options;

        #region Construction

        /// <summary>
        /// Instantiates the function processor. Also loads the defined add-ons
        /// </summary>
        public PublishingFunctionProcessor(ILogger<FunctionProcessor> logger,
            IOptions<SharePointTransformationOptions> options,
            SharePointPublishingFunctionsService sharePointPublishingFunctionsService,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.sharePointPublishingFunctionsService = sharePointPublishingFunctionsService ?? throw new ArgumentNullException(nameof(sharePointPublishingFunctionsService));
            this.serviceProvider = serviceProvider;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the Function Processor with a custom transformation context and a CSOM source context
        /// </summary>
        /// <param name="pageTransformationContext">The current page transformation context</param>
        /// <param name="sourceContext">The CSOM source context</param>
        /// <param name="pageItem">The source page list item</param>
        public void Init(PageTransformationContext pageTransformationContext, ClientContext sourceContext, ListItem pageItem)
        {
            // Configure the SharePoint Function service instance
            this.sharePointPublishingFunctionsService.PageTransformationContext = pageTransformationContext;
            this.sharePointPublishingFunctionsService.SourceContext = sourceContext;
            this.page = pageItem;

            // NOTE: We removed support for addons
        }

        /// <summary>
        /// Replaces instances of the NameAttributeToken with the provided PropertyName
        /// </summary>
        /// <param name="functions">A string value containing the function definition</param>
        /// <param name="propertyName">The property to replace it with</param>
        /// <returns>The newly formatted function value.</returns>
        public string ResolveFunctionToken(string functions, string propertyName)
        {
            return Regex.Replace(functions, NameAttributeToken, propertyName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Executes a function and returns results
        /// </summary>
        /// <param name="functions">Function to process</param>
        /// <param name="propertyName">Field/property the function runs on</param>
        /// <param name="propertyType">Type of the field/property the function will run on</param>
        /// <returns>Function output</returns>
        public Tuple<string, string> Process(string functions, string propertyName, FieldType propertyType)
        {
            string propertyKey = "";
            string propertyValue = "";

            if (!string.IsNullOrEmpty(functions))
            {
                // Updating parsing logic to allow use of {@Name} token value in the function definition
                functions = ResolveFunctionToken(functions, propertyName);

                var functionDefinition = ParseFunctionDefinition(functions, propertyName, propertyType, this.page);

                // Execute function
                MethodInfo methodInfo = null;
                object functionClassInstance = null;

                // Native builtin function
                methodInfo = this.sharePointPublishingFunctionsService.GetType().GetMethod(functionDefinition.Name);
                functionClassInstance = this.sharePointPublishingFunctionsService;

                if (methodInfo != null)
                {
                    // Execute the function
                    object result = ExecuteMethod(functionClassInstance, functionDefinition, methodInfo);

                    // output types support: string or bool
                    if (result is string || result is bool)
                    {
                        propertyKey = propertyName;
                        propertyValue = result.ToString();
                    }
                }
            }

            return new Tuple<string, string>(propertyKey, propertyValue);
        }
        #endregion

        #region Helper methods
        private static FunctionDefinition ParseFunctionDefinition(string function, string propertyName, FieldType propertyType, ListItem page)
        {
            // Supported function syntax: 
            // - EncodeGuid()
            // - MyLib.EncodeGuid()
            // - EncodeGuid({ListId})
            // - StaticString('a string')
            // - EncodeGuid({ListId}, {Param2})
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
                    staticReplacement++;
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
                    Name = propertyName,
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

                    if (functionParameter.Trim().StartsWith("'StaticParameter"))
                    {
                        if (staticParameters.TryGetValue(functionParameter.Trim(), out string staticParameterValue))
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

                // Populate the function parameter with a value coming from publishing page
                input.Type = MapType(propertyType.ToString());

                if (!input.IsStatic)
                {
                    if (propertyType == FieldType.String)
                    {
                        input.Value = page.GetFieldValueAs<string>(input.Name);
                    }
                    else if (propertyType == FieldType.User)
                    {
                        if (page.FieldExistsAndUsed(input.Name))
                        {
                            input.Value = ((FieldUserValue)page[input.Name]).LookupId.ToString();
                        }
                    }
                }
                def.Input.Add(input);
            }

            return def;
        }

        #endregion
    }
}
