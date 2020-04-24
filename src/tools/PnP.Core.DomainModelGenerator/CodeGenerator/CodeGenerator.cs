using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Implements the logic to generate the code
    /// </summary>
    internal class CodeGenerator : ICodeGenerator
    {
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

            foreach (var e in model.Entities)
            {
                GenerateClassFile(e);
                GenerateInterfaceFile(e);
            }

            log.LogInformation("Processed model.");
        }

        private void GenerateClassFile(ModelEntity entity)
        {
            // Prepare the Code Compilation Unit
            var compilationUnit = ConfigureCodeCompilationUnit();

            // Configure the namespace for the output type
            NamespaceDeclarationSyntax schemaNamespace =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{this.options.BaseNamespace}.{entity.Namespace}"));

            ClassDeclarationSyntax typeClass = SyntaxFactory.ClassDeclaration(entity.TypeName)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.InternalKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                )
                //.AddAttributeLists(SyntaxFactory.AttributeList(
                //    SyntaxFactory.SingletonSeparatedList(
                //        SyntaxFactory.Attribute(
                //            SyntaxFactory.ParseName($"{this.options.BaseNamespace}.ClassMapping"),
                //            SyntaxFactory.ParseAttributeArgumentList($"(RestType = \"{entity.Value}.{typeNameAttribute.Value}\", RestGet = \"_api/{typeNameAttribute.Value.ToLower()}\")")
                //        ))))
                .AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"RestBaseDataModel<I{entity.TypeName}>")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{entity.TypeName}"))
                );

            foreach (var property in entity.Properties)
            {
                this.log.LogInformation($"Processing property '{property.Name}' of type '{entity.TypeName}'");

                PropertyDeclarationSyntax classPropertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(property.Type), property.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"GetValue<{property.Type}>();")
                            )).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement("SetValue(value);")
                            )).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                typeClass = typeClass.AddMembers(classPropertyDeclaration);
            }

            schemaNamespace = schemaNamespace.AddMembers(typeClass);
            compilationUnit = compilationUnit.AddMembers(schemaNamespace);

            SaveOutputFile($"{entity.TypeName}.gen.cs", entity.Namespace, compilationUnit);
        }

        private void GenerateInterfaceFile(ModelEntity entity)
        {
            // Prepare the Code Compilation Unit
            var compilationUnit = ConfigureCodeCompilationUnit();

            // Configure the namespace for the output type
            NamespaceDeclarationSyntax schemaNamespace =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{this.options.BaseNamespace}.{entity.Namespace}"));

            InterfaceDeclarationSyntax typeInterface = SyntaxFactory.InterfaceDeclaration($"I{entity.TypeName}")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IDataModel<I{entity.TypeName}>"))
                );

            foreach (var property in entity.Properties)
            {
                this.log.LogInformation($"Processing property '{property.Name}' of interface 'I{entity.TypeName}'");

                PropertyDeclarationSyntax interfacePropertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(property.Type), property.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                typeInterface = typeInterface.AddMembers(interfacePropertyDeclaration);
            }

            schemaNamespace = schemaNamespace.AddMembers(typeInterface);
            compilationUnit = compilationUnit.AddMembers(schemaNamespace);

            SaveOutputFile($"I{entity.TypeName}.gen.cs", entity.Namespace, compilationUnit);
        }

        /// <summary>
        /// Private method to prepare the Code Compilation Unit with usings of pre-configured namespaces
        /// </summary>
        /// <returns>The configured Code Compilation Unit</returns>
        private CompilationUnitSyntax ConfigureCodeCompilationUnit()
        {
            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective
                    (SyntaxFactory.IdentifierName("System")))
                .AddUsings(SyntaxFactory.UsingDirective
                    (SyntaxFactory.IdentifierName("System.Linq.Expressions")))
                .AddUsings(SyntaxFactory.UsingDirective
                    (SyntaxFactory.IdentifierName("System.Threading.Tasks")))
                .AddUsings(SyntaxFactory.UsingDirective
                    (SyntaxFactory.IdentifierName("Microsoft.Extensions.Logging")));

            return compilationUnit;
        }

        private void SaveOutputFile(String outputFileName, String outputFileSubfolder, CompilationUnitSyntax compilationUnit)
        {
            var outputFolderPath =
                !string.IsNullOrEmpty(outputFileSubfolder) ?
                Path.Combine(this.options.OutputFilesRootPath, outputFileSubfolder.Replace('.', '\\')) :
                this.options.OutputFilesRootPath;
            Directory.CreateDirectory(outputFolderPath);
            var outputFilePath = Path.Combine(outputFolderPath, outputFileName);

            using (var writer = new StreamWriter(outputFilePath))
            {
                AdhocWorkspace cw = new AdhocWorkspace();
                OptionSet options = cw.Options;
                cw.Options.WithChangedOption(CSharpFormattingOptions.IndentBraces, true);
                cw.Options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false);
                SyntaxNode formattedNode = Formatter.Format(compilationUnit, cw, options);

                // Normalize and write code as string to the output file
                formattedNode.WriteTo(writer);
            }
        }
    }
}
