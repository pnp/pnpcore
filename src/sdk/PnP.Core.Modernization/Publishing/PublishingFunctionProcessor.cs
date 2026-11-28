
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PnP.Core.Modernization.Publishing
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
            String = 0,
            Bool = 1,
            Guid = 2,
            Integer = 3,
            DateTime = 4,
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

        private PublishingPageTransformation publishingPageTransformation;
        private List<AddOnType> addOnTypes;
        private object builtInFunctions;
        private PnPContext sourceClientContext;
        private PnPContext targetClientContext;
        private IListItem page;
        private BaseTransformationInformation baseTransformationInformation;

        #region Construction
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="page">Page to operate on</param>
        /// <param name="sourceClientContext">Clientcontext of the source site</param>
        /// <param name="targetClientContext">Clientcontext of the target site</param>
        /// <param name="publishingPageTransformation">Publishing page layout mapping</param>
        /// <param name="baseTransformationInformation">Page transformation information</param>
        /// <param name="logObservers">Connected loggers</param>
        public PublishingFunctionProcessor(IListItem page, PnPContext sourceClientContext, PnPContext targetClientContext, PublishingPageTransformation publishingPageTransformation, BaseTransformationInformation baseTransformationInformation, IList<ILogObserver> logObservers = null)
        {
            //Register any existing observers
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            this.page = page;
            this.publishingPageTransformation = publishingPageTransformation;
            this.sourceClientContext = sourceClientContext;
            this.targetClientContext = targetClientContext;
            this.baseTransformationInformation = baseTransformationInformation;

            // we may have null values being passed over from unit tests
            if (this.sourceClientContext != null && this.targetClientContext != null && this.baseTransformationInformation != null)
            {
                RegisterAddons();
            }
        }
        #endregion

        #region Public methods

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

                if (string.IsNullOrEmpty(functionDefinition.AddOn))
                {
                    // Native builtin function
                    methodInfo = typeof(PublishingBuiltIn).GetMethod(functionDefinition.Name);
                    functionClassInstance = this.builtInFunctions;
                }
                else
                {
                    // Function specified via addon
                    var addOn = this.addOnTypes.Where(p => p.Name.Equals(functionDefinition.AddOn, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (addOn != null)
                    {
                        methodInfo = addOn.Type.GetMethod(functionDefinition.Name);
                        functionClassInstance = addOn.Instance;
                    }
                }

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
        private static FunctionDefinition ParseFunctionDefinition(string function, string propertyName, FieldType propertyType, IListItem page)
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
                            input.Value = ((IFieldUserValue)page[input.Name]).LookupId.ToString();
                        }
                    }
                }
                def.Input.Add(input);
            }

            return def;
        }

        private void RegisterAddons()
        {
            // instantiate default built in functions class
            this.addOnTypes = new List<AddOnType>();
            this.builtInFunctions = Activator.CreateInstance(typeof(PublishingBuiltIn), this.baseTransformationInformation, sourceClientContext, targetClientContext, base.RegisteredLogObservers);

            // instantiate the custom function classes (if there are)
            if (this.publishingPageTransformation.AddOns != null)
            {
                foreach (var addOn in this.publishingPageTransformation.AddOns)
                {
                    try
                    {
                        string path = "";
                        if (addOn.Assembly.Contains("\\") && File.Exists(addOn.Assembly))
                        {
                            path = addOn.Assembly;
                        }
                        else
                        {
                            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, addOn.Assembly);
                        }

                        var assembly = Assembly.LoadFile(path);
                        var customType = assembly.GetType(addOn.Type);
                        var instance = Activator.CreateInstance(customType, sourceClientContext);

                        this.addOnTypes.Add(new AddOnType()
                        {
                            Name = addOn.Name,
                            Assembly = assembly,
                            Instance = instance,
                            Type = customType,
                        });
                    }
                    catch (Exception ex)
                    {
                        LogError(LogStrings.Error_FailedToInitiateCustomFunctionClasses, LogStrings.Heading_FunctionProcessor, ex);
                        throw;
                    }
                }
            }
        }
        #endregion

    }
}
