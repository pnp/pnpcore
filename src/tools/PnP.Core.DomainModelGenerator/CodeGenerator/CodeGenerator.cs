using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Implements the logic to generate the code
    /// </summary>
    internal class CodeGenerator : ICodeGenerator
    {
        static readonly string ModelClassGenInternal = "Model.Class.Gen.Internal.txt";
        static readonly string ModelClassInternal = "Model.Class.Internal.txt";
        static readonly string ModelClassPublic = "Model.Class.Public.txt";
        static readonly string ModelCollectionGenInternal = "Model.Collection.Gen.Internal.txt";
        static readonly string ModelCollectionInternal = "Model.Collection.Internal.txt";
        static readonly string ModelCollectionPublic = "Model.Collection.Public.txt";
        static readonly string TypeKey = "%%Type%%";
        static readonly string RestTypeKey = "%%RestType%%";
        static readonly string NamespaceKey = "%%Namespace%%";
        static readonly string PropertiesKey = "%%Properties%%";

        static readonly string ModelPropertyInternal = "Model.Property.Internal.txt";
        static readonly string ModelKeyPropertyInternal = "Model.KeyProperty.Internal.txt";
        static readonly string ModelPropertyPublic = "Model.Property.Public.txt";
        static readonly string ModelNavigationCollectionPropertyInternal = "Model.NavigationCollectionProperty.Internal.txt";
        static readonly string ModelNavigationPropertyInternal = "Model.NavigationProperty.Internal.txt";
        static readonly string PropertyAttributesKey = "%%PropertyAttributes%%";
        static readonly string PropertyTypeKey = "%%PropertyType%%";
        static readonly string PropertyNameKey = "%%PropertyName%%";
        static readonly string CollectionNameKey = "%%CollectionName%%";
        static readonly string CollectionTypeNameKey = "%%CollectionTypeName%%";
        static readonly string NavigationTypeKey = "%%NavigationType%%";
        static readonly string PropertyGetSetKey = "%%PropertyGetSet%%";
        static readonly string KeyPropertyNameKey = "%%KeyPropertyName%%";
        static readonly string KeyPropertyValueKey = "%%KeyPropertyValue%%";

        static readonly string ModelPropertyAttribute = "Model.Property.Attribute.txt";
        static readonly string AttributeNameKey = "%%AttributeName%%";
        static readonly string AttributeConstructorArgumentKey = "%%AttributeConstructorArgument%%";
        static readonly string AttributeParametersKey = "%%AttributeParameters%%";

        private readonly CodeGeneratorOptions options;
        private readonly ILogger log;
        private Dictionary<string, CollectionInformation> neededCollections = new Dictionary<string, CollectionInformation>();

        internal class CollectionInformation
        {
            public string Name { get; set; }
            public string Namespace { get; set; }
            public string Folder { get; set; }
            public string ModelType { get; set; }
        }


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
                    GenerateModelFiles(e);
                }
                else
                {
                    log.LogInformation($"Entity {e.SPRestType} as skipped");
                }
            }

            // Creating collection files
            if (neededCollections.Any())
            {
                foreach(var collection in neededCollections)
                {
                    GenerateModelCollectionFiles(collection.Value);
                }
            }

            log.LogInformation("Processed model.");
        }

        private void GenerateModelCollectionFiles(CollectionInformation collectionInformation)
        {
            var classFile = LoadFile(ModelCollectionInternal);
            var interfaceFile = LoadFile(ModelCollectionPublic);

            Replace(ref classFile, NamespaceKey, collectionInformation.Namespace);
            Replace(ref interfaceFile, NamespaceKey, collectionInformation.Namespace);

            Replace(ref classFile, CollectionNameKey, collectionInformation.Name);
            Replace(ref interfaceFile, CollectionNameKey, collectionInformation.Name);

            Replace(ref classFile, CollectionTypeNameKey, collectionInformation.ModelType);
            Replace(ref interfaceFile, CollectionTypeNameKey, collectionInformation.ModelType);

            SaveFile(classFile, $"{collectionInformation.Name}.cs", collectionInformation.Folder, false);
            SaveFile(interfaceFile, $"I{collectionInformation.Name}.cs", collectionInformation.Folder, true);
        }


        private void GenerateModelFiles(UnifiedModelEntity entity)
        {
            //var classFileGen = LoadFile(ModelClassGenInternal);
            var classFile = LoadFile(ModelClassInternal);
            var interfaceFile = LoadFile(ModelClassPublic);

            Replace(ref classFile, TypeKey, entity.TypeName);
            Replace(ref classFile, NamespaceKey, entity.Namespace);
            Replace(ref classFile, RestTypeKey, entity.SPRestType);
            Replace(ref interfaceFile, TypeKey, entity.TypeName);
            Replace(ref interfaceFile, NamespaceKey, entity.Namespace);

            Tuple<List<PropertyDefinition>, List<PropertyDefinition>> splitExistingPropertiesImplementation = null;
            if (entity.AnalyzedModelType != null && entity.AnalyzedModelType.Implementation != null)
            {
                splitExistingPropertiesImplementation = SplitExistingPropertiesBasedUponAttribute(entity.AnalyzedModelType.Implementation);
            }

            Tuple<List<PropertyDefinition>, List<PropertyDefinition>> splitExistingPropertiesInterface = null;
            if (entity.AnalyzedModelType != null && entity.AnalyzedModelType.Public != null)
            {
                splitExistingPropertiesInterface = SplitExistingPropertiesBasedUponAttribute(entity.AnalyzedModelType.Public);
            }

            Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>> orderedPropertiesImplementation = OrderProperties(entity, splitExistingPropertiesImplementation?.Item1, splitExistingPropertiesImplementation?.Item2);
            Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>> orderedPropertiesInterface = OrderProperties(entity, splitExistingPropertiesInterface?.Item1, splitExistingPropertiesInterface?.Item2);

            StringBuilder classPropertiesString = new StringBuilder();
            StringBuilder interfacePropertiesString = new StringBuilder();

            if (orderedPropertiesImplementation.Item1.Any())
            {
                classPropertiesString.AppendLine("        #region Existing properties");
                classPropertiesString.AppendLine();
            }

            if (orderedPropertiesInterface.Item1.Any())
            {
                interfacePropertiesString.AppendLine("        #region Existing properties");
                interfacePropertiesString.AppendLine();
            }

            foreach (var property in orderedPropertiesImplementation.Item1)
            {
                if (!property.Skip)
                {
                    AddPropertyToClassFile(entity, splitExistingPropertiesImplementation?.Item1, splitExistingPropertiesImplementation?.Item2, classPropertiesString, property);
                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }

            foreach (var property in orderedPropertiesInterface.Item1)
            {
                if (!property.Skip)
                {
                    AddPropertyToInterfaceFile(entity, splitExistingPropertiesInterface?.Item1, splitExistingPropertiesInterface?.Item2, interfacePropertiesString, property);
                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }

            if (orderedPropertiesImplementation.Item1.Any())
            {
                classPropertiesString.AppendLine("        #endregion");
                classPropertiesString.AppendLine();
            }

            classPropertiesString.AppendLine("        #region New properties");
            classPropertiesString.AppendLine();

            if (orderedPropertiesInterface.Item1.Any())
            {
                interfacePropertiesString.AppendLine("        #endregion");
                interfacePropertiesString.AppendLine();
            }

            interfacePropertiesString.AppendLine("        #region New properties");
            interfacePropertiesString.AppendLine();

            foreach (var property in orderedPropertiesImplementation.Item2)
            {
                if (!property.Skip)
                {
                    AddPropertyToClassFile(entity, splitExistingPropertiesImplementation?.Item1, splitExistingPropertiesImplementation?.Item2, classPropertiesString, property);
                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }

            foreach (var property in orderedPropertiesInterface.Item2)
            {
                if (!property.Skip)
                {
                    AddPropertyToInterfaceFile(entity, splitExistingPropertiesInterface?.Item1, splitExistingPropertiesInterface?.Item2, interfacePropertiesString, property);
                }
                else
                {
                    log.LogInformation($"Property {property.Name} was skipped");
                }
            }

            classPropertiesString.AppendLine("        #endregion");
            interfacePropertiesString.AppendLine("        #endregion");

            AddKeyPropertyToClassFile(entity, splitExistingPropertiesImplementation?.Item1, classPropertiesString);

            Replace(ref classFile, PropertiesKey, classPropertiesString.ToString());
            Replace(ref interfaceFile, PropertiesKey, interfacePropertiesString.ToString());

            SaveFile(classFile, $"{entity.TypeName}.cs", entity.Folder, false);
            SaveFile(interfaceFile, $"I{entity.TypeName}.cs", entity.Folder, true);
        }

        private void AddKeyPropertyToClassFile(UnifiedModelEntity entity, List<PropertyDefinition> withAttributes, StringBuilder propertiesString)
        {
            string keyPropertyField = "";
            string keyPropertyType = "";
            //if (withAttributes != null)
            //{
            //    bool found = false;
            //    foreach (var existingProperty in withAttributes)
            //    {
            //        if (found)
            //        {
            //            break;
            //        }

            //        var attributesToCheck = existingProperty.CustomAttributes.Where(p => p.AttributeType.Name == "KeyPropertyAttribute");
            //        foreach (var attributeToCheck in attributesToCheck)
            //        {
            //            if (attributeToCheck.HasConstructorArguments)
            //            {
            //                keyPropertyField = attributeToCheck.ConstructorArguments[0].Value.ToString();
            //                keyPropertyType = ShortenType(attributeToCheck.ConstructorArguments[0].Type.Name);
            //                found = true;
            //            }
            //        }
            //    }
            //}

            if (string.IsNullOrEmpty(keyPropertyField))
            {
                var property = entity.Properties.FirstOrDefault(p => p.Name == "Id");
                if (property == null)
                {
                    property = entity.Properties.FirstOrDefault(p => p.Name == "StringId");
                }

                if (property != null)
                {
                    keyPropertyField = property.Name;
                    keyPropertyType = ShortenType(property.Type);
                }
            }

            if (!string.IsNullOrEmpty(keyPropertyField))
            {
                var keyProperty = LoadFile(ModelKeyPropertyInternal);
                Replace(ref keyProperty, KeyPropertyNameKey, keyPropertyField);

                if (keyPropertyType == "Guid")
                {
                    Replace(ref keyProperty, KeyPropertyValueKey, "Guid.Parse(value.ToString())");
                }
                else if (keyPropertyType == "string")
                {
                    Replace(ref keyProperty, KeyPropertyValueKey, "value.ToString()");
                }
                else if (keyPropertyType == "int" || keyPropertyType == "Int32" || keyPropertyType == "Int64")
                {
                    Replace(ref keyProperty, KeyPropertyValueKey, "(int)value");
                }

                propertiesString.AppendLine(keyProperty);
            }
        }

        private void AddPropertyToInterfaceFile(UnifiedModelEntity entity, List<PropertyDefinition> withAttributes, List<PropertyDefinition> withoutAttributes, StringBuilder propertiesString, UnifiedModelProperty property)
        {
            this.log.LogInformation($"Processing property '{property.Name}' of type '{entity.TypeName}'");

            string propertyToAdd = LoadFile(ModelPropertyPublic);

            if (property.NavigationPropertyIsCollection)
            {
                Replace(ref propertyToAdd, PropertyTypeKey, $"I{RestTypeToCollectionName(property.Type)}");
            }
            else if (property.NavigationProperty)
            {
                Replace(ref propertyToAdd, PropertyTypeKey, $"I{RestTypeToNavigationTypeName(property.Type)}");
            }
            else
            {
                Replace(ref propertyToAdd, PropertyTypeKey, ShortenType(property.Type));
            }

            if (withAttributes != null || withoutAttributes != null)
            {
                // Add attributes which where already defined previously
                var matchingProperty = withAttributes != null ? FindMatchingPropertyBasedUponAttributes(withAttributes, "SharePointPropertyAttribute", property) : null;
                if (matchingProperty == null)
                {
                    // Do we have match on the attribute usage properties based upon name (not all properties have the SharePointPropertyAttribute)
                    matchingProperty = withAttributes != null ? withAttributes.FirstOrDefault(p => p.Name == property.Name) : null;
                }

                if (matchingProperty == null)
                {
                    matchingProperty = withoutAttributes != null ? withoutAttributes.FirstOrDefault(p => p.Name == property.Name) : null;
                }

                if (matchingProperty != null)
                {
                    Replace(ref propertyToAdd, PropertyNameKey, matchingProperty.Name);
                    Replace(ref propertyToAdd, PropertyGetSetKey, property.NavigationProperty ? "get;" : PropertyDefinitionToGetSet(matchingProperty));
                }
                else
                {
                    Replace(ref propertyToAdd, PropertyNameKey, property.Name);
                    Replace(ref propertyToAdd, PropertyGetSetKey, property.NavigationProperty ? "get;" : "get; set;");
                }
            }
            else
            {
                Replace(ref propertyToAdd, PropertyNameKey, property.Name);
                Replace(ref propertyToAdd, PropertyGetSetKey, property.NavigationProperty ? "get;" : "get; set;");
            }

            propertiesString.AppendLine(propertyToAdd);
            propertiesString.AppendLine();
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
                var collectionName = RestTypeToCollectionName(property.Type);
                Replace(ref propertyToAdd, CollectionNameKey, collectionName);
            }
            else if (property.NavigationProperty)
            {
                Replace(ref propertyToAdd, NavigationTypeKey, RestTypeToNavigationTypeName(property.Type));
            }

            string propertyAttributesString = "";
            bool navigationPropertyAttributesAdded = false;
            if (withAttributes != null || withoutAttributes != null)
            {
                // Add attributes which where already defined previously
                var matchingProperty = withAttributes != null ? FindMatchingPropertyBasedUponAttributes(withAttributes, "SharePointPropertyAttribute", property) : null;
                if (matchingProperty == null)
                {
                    // Do we have match on the attribute usage properties based upon name (not all properties have the SharePointPropertyAttribute)
                    matchingProperty = withAttributes != null ? withAttributes.FirstOrDefault(p => p.Name == property.Name) : null;
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
                    AddCollectionToGenerate(entity, property);
                }
                else
                {
                    Replace(ref propertyToAdd, PropertyNameKey, property.Name);
                    AddCollectionToGenerate(entity, property);
                }
            }
            else
            {
                Replace(ref propertyToAdd, PropertyNameKey, property.Name);
                AddCollectionToGenerate(entity, property);
            }

            if (!navigationPropertyAttributesAdded)
            {
                // Add properties we know have to be there
                //if (property.NavigationPropertyIsCollection)
                //{
                //    propertyAttributesString = AddAttribute("SharePointProperty", $"\"{property.Name}\"", "Expandable = true");
                //}
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

        private void AddCollectionToGenerate(UnifiedModelEntity entity, UnifiedModelProperty property)
        {
            if (property.NavigationPropertyIsCollection)
            {
                var collectionName = RestTypeToCollectionName(property.Type);
                var collectionKey = $"{entity.Namespace}.{collectionName}";
                if (!neededCollections.ContainsKey(collectionKey))
                {
                    neededCollections.Add(collectionKey, new CollectionInformation() { Name = collectionName, ModelType = RestTypeToNavigationTypeName(property.Type), Namespace = entity.Namespace, Folder = entity.Folder });
                }
            }
        }

        private string PropertyDefinitionToGetSet(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.SetMethod != null)
            {
                return "get; set;";
            }
            else
            {
                return "get;";
            }
        }

        private Tuple<List<UnifiedModelProperty>, List<UnifiedModelProperty>> OrderProperties(UnifiedModelEntity entity, List<PropertyDefinition> withAttributes, List<PropertyDefinition> withoutAttributes)
        {
            List<UnifiedModelProperty> existingProperties = new List<UnifiedModelProperty>();
            List<UnifiedModelProperty> newProperties = new List<UnifiedModelProperty>();

            foreach (var property in entity.Properties)
            {
                if (withAttributes != null || withoutAttributes != null)
                {
                    // Try to match based upon attribute usage
                    // e.g. Id edmx you have property Description
                    //      In code you have property DisplayName decorated with [SharePointType("Description")]                     
                    var matchingProperty = withAttributes != null ? FindMatchingPropertyBasedUponAttributes(withAttributes, "SharePointPropertyAttribute", property) : null;
                    if (matchingProperty == null)
                    {
                        // Do we have match on the attribute usage properties based upon name (not all properties have the SharePointPropertyAttribute)
                        matchingProperty = withAttributes != null ? withAttributes.FirstOrDefault(p => p.Name == property.Name) : null;

                        if (matchingProperty == null)
                        {
                            // Do we have a regular property?
                            matchingProperty = withoutAttributes != null ? withoutAttributes.FirstOrDefault(p => p.Name == property.Name) : null;
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
