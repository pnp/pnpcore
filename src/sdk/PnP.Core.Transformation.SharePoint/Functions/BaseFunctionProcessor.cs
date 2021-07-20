using System;
using System.Collections.Generic;
using System.Reflection;

namespace PnP.Core.Transformation.SharePoint.Functions
{
    /// <summary>
    /// Base class for function processors
    /// </summary>
    public abstract class BaseFunctionProcessor
    {
        /// <summary>
        /// Allowed function parameter types
        /// </summary>
        internal enum FunctionType
        {
            String = 0,
            Integer = 1,
            Bool = 2,
            Guid = 3,
            DateTime = 4
        }

        /// <summary>
        /// Definition of a function parameter
        /// </summary>
        internal class FunctionParameter
        {
            /// <summary>
            /// Name of the parameter
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Type if the parameter
            /// </summary>
            public FunctionType Type { get; set; }
            /// <summary>
            /// Value of the parameter
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Is this a static, hard coded, value. Eg 'my hardcoded string'
            /// </summary>
            public bool IsStatic { get; set; }
        }

        /// <summary>
        /// Definition of a function or selector
        /// </summary>
        internal class FunctionDefinition
        {
            /// <summary>
            /// AddOn hosting the function/selector. Empty value means the function is hosted by the internal builtin functions library
            /// </summary>
            public string AddOn { get; set; }
            /// <summary>
            /// Name of the function/selector
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Parameter specifying the function result
            /// </summary>
            public FunctionParameter Output { get; set; }
            /// <summary>
            /// List of input parameter used to call the function
            /// </summary>
            public List<FunctionParameter> Input { get; set; }
        }

        /// <summary>
        /// Defines a loaded AddOn function/selector class instance
        /// </summary>
        internal class AddOnType
        {
            /// <summary>
            /// Name of the addon. The name is used to link the determine which class instance needs to be used to execute a function
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Instance of the class that holds the functions/selectors
            /// </summary>
            public object Instance { get; set; }
            /// <summary>
            /// Assembly holding the functions/selector class
            /// </summary>
            public Assembly Assembly { get; set; }
            /// <summary>
            /// Type of the functions/selector class
            /// </summary>
            public Type Type { get; set; }
        }

        #region Helper methods
        internal static FunctionType MapType(string inputType)
        {
            inputType = inputType.ToLower().Trim();

            if (inputType == "string")
            {
                return FunctionType.String;
            }
            else if (inputType == "integer")
            {
                return FunctionType.Integer;
            }
            else if (inputType == "bool")
            {
                return FunctionType.Bool;
            }
            else if (inputType == "guid")
            {
                return FunctionType.Guid;
            }
            else if (inputType == "datetime")
            {
                return FunctionType.DateTime;
            }

            return FunctionType.String;
        }

        internal static object ExecuteMethod(object functionClassInstance, FunctionDefinition functionDefinition, MethodInfo methodInfo)
        {
            object result = null;
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length == 0)
            {
                // Call the method without parameters
                result = methodInfo.Invoke(functionClassInstance, null);
            }
            else
            {
                // Method requires input, so fill the parameters
                List<object> paramInput = new List<object>(functionDefinition.Input.Count);
                foreach (var param in functionDefinition.Input)
                {
                    switch (param.Type)
                    {
                        case FunctionType.String:
                            {
                                paramInput.Add(param.Value);
                                break;
                            }
                        case FunctionType.Integer:
                            {
                                if (Int32.TryParse(param.Value, out Int32 i))
                                {
                                    paramInput.Add(i);
                                }
                                else
                                {
                                    paramInput.Add(Int32.MinValue);
                                }
                                break;
                            }
                        case FunctionType.Guid:
                            {
                                if (Guid.TryParse(param.Value, out Guid g))
                                {
                                    paramInput.Add(g);
                                }
                                else
                                {
                                    paramInput.Add(Guid.Empty);
                                }
                                break;
                            }
                        case FunctionType.DateTime:
                            {
                                if (DateTime.TryParse(param.Value, out DateTime d))
                                {
                                    paramInput.Add(d);
                                }
                                else
                                {
                                    paramInput.Add(DateTime.MinValue);
                                }
                                break;
                            }
                        case FunctionType.Bool:
                            {
                                if (bool.TryParse(param.Value, out bool b))
                                {
                                    paramInput.Add(b);
                                }
                                else
                                {
                                    paramInput.Add(false);
                                }
                                break;
                            }
                    }

                }

                // Call the method with parameters
                result = methodInfo.Invoke(functionClassInstance, paramInput.ToArray());
            }

            // Return the method invocation result
            return result;
        }
        #endregion

    }
}
