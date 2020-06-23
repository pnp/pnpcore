using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Implements the logic to generate the code
    /// </summary>
    internal class CodeGenerator : ICodeGenerator
    {
        static readonly string ModelClassInternal = "Model.Class.Internal.txt";
        static readonly string TypeKey = "%%Type%%";
        static readonly string NamespaceKey = "%%Namespace%%";
        static readonly string PropertiesKey = "%%Properties%%";

        static readonly string ModelPropertyInternal = "Model.Property.Internal.txt";
        static readonly string ModelNavigationCollectionPropertyInternal = "Model.NavigationCollectionProperty.Internal.txt";
        static readonly string ModelNavigationPropertyInternal = "Model.NavigationProperty.Internal.txt";
        static readonly string PropertyAttributesKey = "%%PropertyAttributes%%";
        static readonly string PropertyTypeKey = "%%PropertyType%%";
        static readonly string PropertyNameKey = "%%PropertyName%%";
        static readonly string CollectionNameKey = "%%CollectionName%%";
        static readonly string NavigationTypeKey = "%%NavigationType%%";

        static readonly string ModelPropertyAttribute = "Model.Property.Attribute.txt";
        static readonly string AttributeNameKey = "%%AttributeName%%";
        static readonly string AttributeConstructorArgumentKey = "%%AttributeConstructorArgument%%";
        static readonly string AttributeParametersKey = "%%AttributeParameters%%";

        private readonly CodeGeneratorOptions options;
        private readonly ILogger log;

        public CodeGenerator(
            IOptionsMonitor<CodeGeneratorOptions> options,
            ILogger<CodeGenerator> logger,
            IServiceProvider serviceProvider)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options.CurrentValue;
            this.log = logger;
        }


        public async Task ProcessAsync(Model model)
        {
            log.LogInformation("Processing model ...");

            foreach (var e in model.SharePoint.Entities)
            {
                if (!e.Skip)
                {
                    GenerateClassFile(e);
                    //GenerateInterfaceFile(e);
                }
                else
                {
                    log.LogInformation($"Entity {e.SPRestType} as skipped");
                }
            }

            log.LogInformation("Processed model.");
        }

        private void GenerateClassFile(UnifiedModelEntity entity)
        {
            var impl = entity.AnalyzedModelType.Implementation;
            var classFile = LoadFile(ModelClassInternal);
            Replace(ref classFile, TypeKey, entity.TypeName);
            Replace(ref classFile, NamespaceKey, entity.Namespace);

            var splitExistingProperties = SplitExistingPropertiesBasedUponAttribute(impl);

            Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>> orderedProperties = OrderProperties(entity, splitExistingProperties.Item1, splitExistingProperties.Item2);

            StringBuilder propertiesString = new StringBuilder();

            propertiesString.AppendLine("        #region Existing properties");
            propertiesString.AppendLine();

            foreach (var property in orderedProperties.Item1)
            {
                if (!property.Skip)
                {
                    AddPropertyToClassFile(entity, splitExistingProperties.Item1, splitExistingProperties.Item2, propertiesString, property);

                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }
            propertiesString.AppendLine("        #endregion");
            propertiesString.AppendLine();
            propertiesString.AppendLine("        #region New properties");
            propertiesString.AppendLine();

            foreach (var property in orderedProperties.Item2)
            {
                if (!property.Skip)
                {
                    AddPropertyToClassFile(entity, splitExistingProperties.Item1, splitExistingProperties.Item2, propertiesString, property);

                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }

            propertiesString.AppendLine("        #endregion");

            // Handle the Key property

            Replace(ref classFile, PropertiesKey, propertiesString.ToString());

            SaveFile(classFile, $"{entity.TypeName}.gen.cs", entity.Folder, false);
        }

        private void AddPropertyToClassFile(UnifiedModelEntity entity, List<PropertyDefinition> withAttributes, List<PropertyDefinition> withoutAttributes, StringBuilder propertiesString, UnifiedModelProperty property)
        {
            this.log.LogInformation($"Processing property '{property.Name}' of type '{entity.TypeName}'");

            string baseProperty = "";
            if (!property.NavigationProperty)
            {
                // Regular props
                baseProperty = LoadFile(ModelPropertyInternal);
            }
            else
            {
                // Navigation props
                if (property.NavigationPropertyIsCollection)
                {
                    baseProperty = LoadFile(ModelNavigationCollectionPropertyInternal);
                }
                else
                {
                    baseProperty = LoadFile(ModelNavigationPropertyInternal);
                }
            }

            var propertyToAdd = baseProperty.Replace(PropertyTypeKey, ShortenType(property.Type));

            if (property.NavigationPropertyIsCollection)
            {
                Replace(ref propertyToAdd, CollectionNameKey, RestTypeToCollectionName(property.Type));
            }
            else if (property.NavigationProperty)
            {
                Replace(ref propertyToAdd, NavigationTypeKey, RestTypeToNavigationTypeName(property.Type));
            }

            string propertyAttributesString = "";
            bool navigationPropertyAttributesAdded = false;
            if (withAttributes.Any() || withoutAttributes.Any())
            {
                // Add attributes which where already defined previously
                var matchingProperty = FindMatchingPropertyBasedUponAttributes(withAttributes, "SharePointPropertyAttribute", property);
                if (matchingProperty == null)
                {
                    // Do we have match on the attribute usage properties based upon name (not all properties have the SharePointPropertyAttribute)
                    matchingProperty = withAttributes.FirstOrDefault(p => p.Name == property.Name);
                }    

                if (matchingProperty != null)
                {
                    foreach (var attr in matchingProperty.CustomAttributes)
                    {
                        navigationPropertyAttributesAdded = true;
                        if (!string.IsNullOrEmpty(propertyAttributesString))
                        {
                            propertyAttributesString += Environment.NewLine;
                        }

                        propertyAttributesString += CustomAttributeToCode(attr);
                    }

                    Replace(ref propertyToAdd, PropertyNameKey, matchingProperty.Name);
                }
                else
                {
                    Replace(ref propertyToAdd, PropertyNameKey, property.Name);
                }
            }
            else
            {
                Replace(ref propertyToAdd, PropertyNameKey, property.Name);
            }

            if (!navigationPropertyAttributesAdded)
            {
                // Add properties we know have to be there
                if (property.NavigationPropertyIsCollection)
                {
                    propertyAttributesString = AddAttribute("SharePointProperty", $"\"{property.Name}\"", "Expandable = true");
                }
            }

            if (!string.IsNullOrEmpty(propertyAttributesString))
            {
                Replace(ref propertyToAdd, PropertyAttributesKey, propertyAttributesString);
            }
            else
            {
                Replace(ref propertyToAdd, $"{PropertyAttributesKey}{Environment.NewLine}", propertyAttributesString);
            }

            propertiesString.AppendLine(propertyToAdd);
            propertiesString.AppendLine();
        }

        private Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>> OrderProperties(UnifiedModelEntity entity, List<PropertyDefinition> withAttributes, List<PropertyDefinition> withoutAttributes)
        {
            List<UnifiedModelProperty> existingProperties = new List<UnifiedModelProperty>();
            List<UnifiedModelProperty> newProperties = new List<UnifiedModelProperty>();

            foreach (var property in entity.Properties)
            {
                // Try to match based upon attribute usage
                // e.g. Id edmx you have property Description
                //      In code you have property DisplayName decorated with [SharePointType("Description")] 
                var matchingProperty = FindMatchingPropertyBasedUponAttributes(withAttributes, "SharePointPropertyAttribute", property);
                if (matchingProperty == null)
                {
                    // Do we have match on the attribute usage properties based upon name (not all properties have the SharePointPropertyAttribute)
                    matchingProperty = withAttributes.FirstOrDefault(p => p.Name == property.Name);

                    if (matchingProperty == null)
                    {
                        // Do we have a regular property?
                        matchingProperty = withoutAttributes.FirstOrDefault(p => p.Name == property.Name);
                    }
                }

                if (matchingProperty != null)
                {
                    existingProperties.Add(property);
                }
                else
                {
                    newProperties.Add(property);
                }
            }

            return new Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>>(existingProperties, newProperties);
        }

        private static PropertyDefinition FindMatchingPropertyBasedUponAttributes(List<PropertyDefinition> withAttributes, string attributeName, UnifiedModelProperty property)
        {
            foreach (var existingProperty in withAttributes)
            {
                var attributesToCheck = existingProperty.CustomAttributes.Where(p => p.AttributeType.Name == attributeName);
                foreach (var attributeToCheck in attributesToCheck)
                {
                    if (attributeToCheck.HasConstructorArguments && attributeToCheck.ConstructorArguments[0].Value.ToString().Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return existingProperty;
                    }
                }
            }

            return null;
        }

        private Tuple<List<PropertyDefinition>, List<PropertyDefinition>> SplitExistingPropertiesBasedUponAttribute(TypeDefinition type)
        {
            List<PropertyDefinition> withAttributes = new List<PropertyDefinition>();
            List<PropertyDefinition> withoutAttributes = new List<PropertyDefinition>();

            foreach(var property in type.Properties)
            {
                if (property.HasCustomAttributes)
                {
                    if (property.CustomAttributes.Any(p=>p.AttributeType.Name == "SharePointPropertyAttribute" || p.AttributeType.Name == "GraphPropertyAttribute"))
                    {
                        withAttributes.Add(property);
                    }
                    else
                    {
                        withoutAttributes.Add(property);
                    }
                }
                else
                {
                    withoutAttributes.Add(property);
                }
            }

            return new Tuple<List<PropertyDefinition>, List<PropertyDefinition>>(withAttributes, withoutAttributes);
        }
        
        private string AddAttribute(string attributeName, string constructorArguments, string properties)
        {
            string code = LoadFile(ModelPropertyAttribute);
            Replace(ref code, AttributeNameKey, attributeName);

            if (string.IsNullOrEmpty(constructorArguments))
            {
                constructorArguments = "";
            }

            Replace(ref code, AttributeConstructorArgumentKey, constructorArguments);

            if (string.IsNullOrEmpty(properties))
            {
                properties = "";
            }

            properties = (!string.IsNullOrEmpty(constructorArguments) ? ", " : "") + properties;

            Replace(ref code, AttributeParametersKey, properties);

            return code;
        }


        private string CustomAttributeToCode(CustomAttribute attribute)
        {
            string code = LoadFile(ModelPropertyAttribute);

            Replace(ref code, AttributeNameKey, attribute.AttributeType.Name.Substring(0, attribute.AttributeType.Name.Length - "Attribute".Length));
            
            if (attribute.HasConstructorArguments)
            {
                string constructorArgumentsString = "";
                foreach (var constructorArgument in attribute.ConstructorArguments)
                {
                    if (constructorArgument.Type.FullName == "System.String")
                    {
                        constructorArgumentsString = constructorArgumentsString + "\"" + CleanupValue(constructorArgument.Type.FullName, constructorArgument.Value.ToString()) + "\",";
                    }
                    else
                    {
                        constructorArgumentsString = constructorArgumentsString + CleanupValue(constructorArgument.Type.FullName, constructorArgument.Value.ToString()) + ",";
                    }
                }

                constructorArgumentsString = constructorArgumentsString.TrimEnd(',');
                Replace(ref code, AttributeConstructorArgumentKey, constructorArgumentsString);
            }
            else
            {
                Replace(ref code, AttributeConstructorArgumentKey, "");
            }

            if (attribute.HasProperties)
            {
                string attributeParametersString = "";
                foreach(var attributeParameter in attribute.Properties)
                {
                    if (attributeParameter.Argument.Type.FullName == "System.String")
                    {
                        attributeParametersString = attributeParametersString + attributeParameter.Name +  " = \"" + CleanupValue(attributeParameter.Argument.Type.FullName, attributeParameter.Argument.Value.ToString()) + "\",";
                    }
                    else
                    {
                        attributeParametersString = attributeParametersString + attributeParameter.Name + " = " + CleanupValue(attributeParameter.Argument.Type.FullName, attributeParameter.Argument.Value.ToString()) + ",";
                    }
                }

                attributeParametersString = (attribute.HasConstructorArguments ? ", " : "") + attributeParametersString.TrimEnd(',');

                Replace(ref code, AttributeParametersKey, attributeParametersString);
            }
            else
            {
                Replace(ref code, AttributeParametersKey, "");
            }

            return code;
        }

        private string CleanupValue(string type, string value)
        {
            if (type == "System.Boolean")
            {
                if (value.ToLower() == "true" || value.ToLower() == "false")
                {
                    return value.ToLower();
                }
            }

            return value;
        }

        private string ShortenType(string type)
        {
            if (type.StartsWith("System."))
            {
                return type.Substring(7);
            }

            return type;
        }

        private string RestTypeToNavigationTypeName(string restType)
        {
            if (restType.IndexOf(".") > -1)
            {
                restType = restType.Substring(restType.LastIndexOf(".") + 1);
            }

            if (restType.StartsWith("SP"))
            {
                restType = restType.Substring(2);
            }

            if (restType.EndsWith("Entity"))
            {
                restType.Substring(0, restType.Length - "Entity".Length);
            }

            return $"{restType}";
        }


        private string RestTypeToCollectionName(string restType)
        {
            if (restType.IndexOf(".") > -1)
            {
                restType = restType.Substring(restType.LastIndexOf(".") + 1);
            }

            if (restType.StartsWith("SP"))
            {
                restType = restType.Substring(2);
            }

            if (restType.EndsWith("Entity"))
            {
                restType.Substring(0, restType.Length - "Entity".Length);
            }

            return $"{restType}Collection";
        }

        private string TypeToString(TypeReference type)
        {
            string result = type.Name;
            if (((GenericInstanceType)type).HasGenericArguments)
            {
                string genericParams = "";
                foreach (var param in ((GenericInstanceType)type).GenericArguments)
                {
                    genericParams = genericParams + param.Name + ",";
                }
                genericParams = genericParams.TrimEnd(',');

                result = type.Name.Substring(0, type.Name.IndexOf("`")) + "<" + genericParams + ">";
            }

            return result;
        }

        #region Template handling

        private void Replace(ref string classFile, string from, string to)
        {
            classFile = classFile.Replace(from, to);
        }

        private void SaveFile(string content, string outputFileName, string outputFileSubfolder, bool publicModel)
        {
            var outputFolderPath =
                    !string.IsNullOrEmpty(outputFileSubfolder) ?
                    Path.Combine(this.options.OutputFilesRootPath, outputFileSubfolder.Replace("/", "")) :
                    this.options.OutputFilesRootPath;

            if (publicModel)
            {
                outputFolderPath = Path.Combine(outputFolderPath, "Public");
            }
            else
            {
                outputFolderPath = Path.Combine(outputFolderPath, "Internal");
            }

            Directory.CreateDirectory(outputFolderPath);
            var outputFilePath = Path.Combine(outputFolderPath, outputFileName);
            
            File.WriteAllText(outputFilePath, content);                
        }

        private string LoadFile(string fileName)
        {
            var fileContent = "";
            using (Stream stream = typeof(CodeGenerator).Assembly.GetManifestResourceStream($"PnP.M365.DomainModelGenerator.CodeGenerator.Templates.{fileName}"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }

            return fileContent;
        }
        #endregion

    }
}
