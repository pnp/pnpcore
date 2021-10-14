using PnP.Core.QueryModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class SyntexContentCenter : ISyntexContentCenter
    {
        public IWeb Web { get; internal set; }

        public async Task<List<ISyntexModel>> GetSyntexModelsAsync(string modelName = null)
        {
            List<ISyntexModel> models = new List<ISyntexModel>();

            IList modelLibrary = null;

            // See if we already loaded the library previously
            if (Web.Lists.Requested)
            {
                modelLibrary = Web.Lists.AsEnumerable().FirstOrDefault(p => p.TemplateType == (ListTemplateType)1328);
            }

            // if not loaded do a server roundtrip to get library
            if (modelLibrary == null)
            {
                var currentGraphFirstSetting = Web.PnPContext.GraphFirst;
                Web.PnPContext.GraphFirst = false;
                modelLibrary = await Web.Lists.Where(p => p.TemplateType == (ListTemplateType)1328).FirstOrDefaultAsync().ConfigureAwait(false);
                Web.PnPContext.GraphFirst = currentGraphFirstSetting;
            }

            if (modelLibrary != null)
            {
                string pageNameFilter = $@"
                        <BeginsWith>
                          <FieldRef Name='{PageConstants.FileLeafRef}'/>
                          <Value Type='text'><![CDATA[{modelName}]]></Value>
                        </BeginsWith>";

                string pageQuery = $@"
                <View Scope='Recursive'>
                  <ViewFields>
                    <FieldRef Name='{PageConstants.FileType}' />
                    <FieldRef Name='{PageConstants.FileLeafRef}' />
                    <FieldRef Name='{PageConstants.FileDirRef}' />
                    <FieldRef Name='{PageConstants.IdField}' />
                    <FieldRef Name='{PageConstants.DescriptionField}' />
                    <FieldRef Name='{PageConstants.ModelExplanations}' />
                    <FieldRef Name='{PageConstants.ModelDescription}' />
                    <FieldRef Name='{PageConstants.ModelSchemas}' />
                    <FieldRef Name='{PageConstants.ModelMappedClassifierName}' />
                    <FieldRef Name='{PageConstants.ModelLastTrained}' />
                    <FieldRef Name='{PageConstants.ModelSettings}' />
                    <FieldRef Name='{PageConstants.ModelConfidenceScore}' />
                    <FieldRef Name='{PageConstants.ModelAccuracy}' />
                    <FieldRef Name='{PageConstants.ModelClassifiedItemCount}' />
                    <FieldRef Name='{PageConstants.ModelMismatchedItemCount}' />
                  </ViewFields>
                  <Query>
                    <Where>
                      {{1}}
                        <Contains>
                          <FieldRef Name='File_x0020_Type'/>
                          <Value Type='text'>classifier</Value>
                        </Contains>
                        {{0}}
                      {{2}}
                    </Where>
                  </Query>
                </View>";

                if (!string.IsNullOrEmpty(modelName))
                {
                    pageQuery = string.Format(pageQuery, pageNameFilter, "<And>", "</And>");
                }
                else
                {
                    pageQuery = string.Format(pageQuery, "", "", "");
                }

                // Remove unneeded cariage returns
                pageQuery = pageQuery.Replace("\r\n", "");

                await modelLibrary.LoadListDataAsStreamAsync(new RenderListDataOptions()
                {
                    ViewXml = pageQuery,
                    RenderOptions = RenderListDataOptionsFlags.ListData
                }).ConfigureAwait(false);

                // Load the model files in batch
                var batch = Web.PnPContext.NewBatch();
                List<KeyValuePair<string, IFile>> files = new List<KeyValuePair<string, IFile>>();
                foreach (var model in modelLibrary.Items.AsRequested())
                {
                    files.Add(new KeyValuePair<string, IFile>(model.Values[PageConstants.ModelMappedClassifierName].ToString(),
                                                              await Web.GetFileByServerRelativeUrlBatchAsync(batch, $"{model.Values[PageConstants.FileDirRef]}/{model.Values[PageConstants.FileLeafRef]}").ConfigureAwait(false)));
                }
                await Web.PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

                foreach (var model in modelLibrary.Items.AsRequested())
                {
                    var modelFile = files.FirstOrDefault(p => p.Key == model.Values[PageConstants.ModelMappedClassifierName].ToString());

                    SyntexModel syntexModel = new SyntexModel()
                    {
                        ListItem = model,
                        File = modelFile.Value
                    };
                    models.Add(syntexModel);
                }
            }

            return models;
        }

        public List<ISyntexModel> GetSyntexModels(string modelName = null)
        {
            return GetSyntexModelsAsync(modelName).GetAwaiter().GetResult();
        }

    }
}
