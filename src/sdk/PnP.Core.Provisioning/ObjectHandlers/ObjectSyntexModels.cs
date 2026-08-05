using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.Model.Configuration.SyntexModels.Models;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileLevelModel = PnP.Core.Provisioning.Model.FileLevel;
using TemplateFile = PnP.Core.Provisioning.Model.File;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Extracts SharePoint Syntex document understanding models, and the training files behind
    /// them, from a content center site.
    /// </summary>
    internal class ObjectSyntexModels : ObjectHandlerBase
    {
        /// <summary>The site template of a Syntex content center.</summary>
        private const string ContentCenterTemplate = "CONTENTCTR";

        /// <summary>List template id of the models library.</summary>
        private const int ModelLibraryTemplate = 1328;

        /// <summary>List template id of the training files library.</summary>
        private const int TrainingLibraryTemplate = 1330;

        private const string FieldFileLeafRef = "FileLeafRef";

        /// <summary>
        /// The model metadata carried into the template. All are copied verbatim as file properties.
        /// </summary>
        private static readonly string[] ModelFields =
        {
            "ModelExplanations", "ModelSchemas", "ModelDescription", "ModelMappedClassifierName",
            "ModelLastTrained", "ModelSettings", "ModelConfidenceScore", "ModelAccuracy",
            "ModelClassifiedItemCount", "ModelMismatchedItemCount",
        };

        /// <summary>
        /// Training file metadata copied verbatim. <c>SampleModelId</c> and <c>SampleMarkups</c> are
        /// handled separately because both need the model id replaced with a token.
        /// </summary>
        private static readonly string[] SampleFields =
        {
            "SampleDescription", "SampleExtractedText", "SampleFileType",
            "SampleLabelUptime", "SampleTokenEndPosition", "SampleTokenStartPosition",
        };

        private const string FieldSampleModelId = "SampleModelId";
        private const string FieldSampleMarkups = "SampleMarkups";

        public override string Name => "SyntexModels";

        public override string InternalName => "SyntexModels";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return false;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // The site template check needs a round trip, so it happens in ExtractObjectsAsync.
            return true;
        }

        public override Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            // Models are applied by ObjectFiles - see the remarks on the class.
            return Task.FromResult(parser);
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.Url, w => w.ServerRelativeUrl, w => w.WebTemplate).ConfigureAwait(false);

                if (!ContentCenterTemplate.Equals(web.WebTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    return template;
                }

                await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.TemplateType)).ConfigureAwait(false);

                IList modelLibrary = FindLibrary(web, ModelLibraryTemplate);
                if (modelLibrary == null)
                {
                    context.Logger?.LogInformation("{Source}: this content center has no models library - nothing to extract.",
                        Constants.LOGGING_SOURCE);
                    return template;
                }

                IList trainingLibrary = FindLibrary(web, TrainingLibraryTemplate);

                // PnP Core has no CamlQuery.CreateAllItemsQuery() equivalent; the empty query is
                // what that helper produced.
                await modelLibrary.LoadItemsByCamlQueryAsync("<View><Query></Query></View>").ConfigureAwait(false);

                List<ExtractSyntexModelsModelsConfiguration> wanted = configuration?.SyntexModels?.Models;

                foreach (IListItem model in modelLibrary.Items.AsRequested())
                {
                    ExtractSyntexModelsModelsConfiguration modelConfiguration = MatchConfiguration(model, wanted);

                    // An explicit list of models in the configuration means "only these"
                    if (wanted != null && wanted.Count > 0 && modelConfiguration == null)
                    {
                        continue;
                    }

                    await ExtractModelAsync(context, web, template, configuration, model, trainingLibrary, modelConfiguration).ConfigureAwait(false);
                }

                return template;
            }
        }

        private static IList FindLibrary(IWeb web, int templateType)
        {
            return web.Lists.AsRequested().SingleOrDefault(l => (int)l.TemplateType == templateType);
        }

        private static ExtractSyntexModelsModelsConfiguration MatchConfiguration(IListItem model, List<ExtractSyntexModelsModelsConfiguration> wanted)
        {
            if (wanted == null)
            {
                return null;
            }

            string fileName = GetStringValue(model, FieldFileLeafRef);

            return wanted.FirstOrDefault(w =>
                (!string.IsNullOrEmpty(w.Name) && w.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                || (w.Id > 0 && w.Id == model.Id));
        }

        private async Task ExtractModelAsync(PnPContext context, IWeb web, ProvisioningTemplate template, ExtractConfiguration configuration,
            IListItem model, IList trainingLibrary, ExtractSyntexModelsModelsConfiguration modelConfiguration)
        {
            TemplateFile modelFile = await AddSyntexFileAsync(context, web, template, configuration, model).ConfigureAwait(false);
            if (modelFile == null)
            {
                return;
            }

            CopyProperties(model, modelFile, ModelFields);

            if (trainingLibrary == null || modelConfiguration?.ExcludeTrainingData == true)
            {
                return;
            }

            await ExtractTrainingFilesAsync(context, web, template, configuration, model, trainingLibrary, modelFile).ConfigureAwait(false);
        }

        private async Task ExtractTrainingFilesAsync(PnPContext context, IWeb web, ProvisioningTemplate template, ExtractConfiguration configuration,
            IListItem model, IList trainingLibrary, TemplateFile modelFile)
        {
            string query = string.Format(CultureInfo.InvariantCulture,
                @"<View Scope='RecursiveAll'>
                    <Query>
                      <Where>
                        <Eq>
                          <FieldRef Name='SampleModelId' LookupId='TRUE'/>
                          <Value Type='text'>{0}</Value>
                        </Eq>
                      </Where>
                    </Query>
                  </View>", model.Id);

            await trainingLibrary.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);

            // The model is referenced by list item id, which is meaningless on the target site.
            string modelToken = $"{{filelistitemid:{modelFile.Src}}}";

            foreach (IListItem trainingItem in trainingLibrary.Items.AsRequested())
            {
                TemplateFile trainingFile = await AddSyntexFileAsync(context, web, template, configuration, trainingItem).ConfigureAwait(false);
                if (trainingFile == null)
                {
                    continue;
                }

                trainingFile.Properties[FieldSampleModelId] = modelToken;

                string markups = GetStringValue(trainingItem, FieldSampleMarkups);
                if (!string.IsNullOrEmpty(markups))
                {
                    trainingFile.Properties[FieldSampleMarkups] = TokenizeSampleMarkups(markups, model.Id, modelToken);
                }

                CopyProperties(trainingItem, trainingFile, SampleFields);
            }
        }

        /// <summary>
        /// Replaces the model's list item id inside the sample markup JSON with a token.
        /// </summary>
        private static string TokenizeSampleMarkups(string sampleMarkupJson, int modelId, string tokenValue)
        {
            return sampleMarkupJson
                .Replace($"\"{modelId}\": {{", $"\"{tokenValue}\": {{")
                .Replace($"\"{modelId}\":{{", $"\"{tokenValue}\": {{")
                .Replace($"\"modelItemId\": \"{modelId}\"", $"\"modelItemId\": \"{tokenValue}\"")
                .Replace($"\"modelItemId\":{modelId}", $"\"modelItemId\": {tokenValue}");
        }

        /// <summary>
        /// Records a Syntex file in the template and copies its bytes into the template's connector.
        /// </summary>
        /// <returns>The template file entry, or the existing one when it was already added</returns>
        private async Task<TemplateFile> AddSyntexFileAsync(PnPContext context, IWeb web, ProvisioningTemplate template,
            ExtractConfiguration configuration, IListItem item)
        {
            IFile file = await item.File.GetAsync(f => f.ServerRelativeUrl, f => f.Name, f => f.Level).ConfigureAwait(false);

            if (file == null)
            {
                return null;
            }

            var fullUri = new Uri(new Uri(web.Url.ToString()), file.ServerRelativeUrl);
            string folderPath = Uri.UnescapeDataString(
                fullUri.Segments.Take(fullUri.Segments.Length - 1).Aggregate((i, x) => i + x).TrimEnd('/'));
            string fileName = Uri.UnescapeDataString(fullUri.Segments[fullUri.Segments.Length - 1]);

            string templateFolderPath = folderPath.Substring(web.ServerRelativeUrl.Length).TrimStart('/');
            string src = $"{templateFolderPath}/{fileName}";

            TemplateFile existing = template.Files.FirstOrDefault(f => f.Src.Equals(src, StringComparison.CurrentCultureIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            var templateFile = new TemplateFile
            {
                Folder = templateFolderPath,
                Src = src,
                Overwrite = true,
                Level = ToTemplateLevel(file.Level),
            };

            template.Files.Add(templateFile);

            await PersistFileAsync(context, configuration, file, templateFolderPath, fileName).ConfigureAwait(false);

            return templateFile;
        }

        /// <summary>
        /// Copies a file's bytes into the template's connector.
        /// </summary>
        private async Task PersistFileAsync(PnPContext context, ExtractConfiguration configuration, IFile file, string container, string fileName)
        {
            if (configuration?.FileConnector == null)
            {
                string message = $"No connector is configured, so '{fileName}' was recorded in the template but not exported.";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Error);
                return;
            }

            string connectorContainer = container.Trim('/').Replace("/", "\\");

            if (!string.IsNullOrEmpty(configuration.FileConnector.GetContainer()))
            {
                connectorContainer = string.Concat(configuration.FileConnector.GetContainer(), connectorContainer);
            }

            using (Stream content = await file.GetContentAsync(true).ConfigureAwait(false))
            {
                using (var buffer = new MemoryStream())
                {
                    await content.CopyToAsync(buffer).ConfigureAwait(false);
                    buffer.Position = 0;

                    configuration.FileConnector.SaveFileStream(fileName, connectorContainer, buffer);
                }
            }
        }

        private static void CopyProperties(IListItem item, TemplateFile templateFile, IEnumerable<string> fieldNames)
        {
            foreach (string fieldName in fieldNames)
            {
                string value = GetStringValue(item, fieldName);
                if (value != null)
                {
                    templateFile.Properties[fieldName] = value;
                }
            }
        }

        private static string GetStringValue(IListItem item, string fieldName)
        {
            return item.Values.TryGetValue(fieldName, out object value) ? value?.ToString() : null;
        }

        private static FileLevelModel ToTemplateLevel(PublishedStatus level)
        {
            return Enum.TryParse(level.ToString(), out FileLevelModel parsed) ? parsed : FileLevelModel.Published;
        }
    }
}
