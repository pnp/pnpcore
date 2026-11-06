using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Type resolver for DriveFolders and DriveFiles from Model to Schema
    /// </summary>
    internal class DriveItemsFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;


        public object Resolve(object source, Dictionary<string, IResolver> resolvers = null, bool recursive = false)
        {
            Object result = null;
            Model.Drive.DriveFolderBase folder = null;

            var driveRoot = source as Model.Drive.DriveRoot;
            if (driveRoot != null)
            {
                folder = driveRoot.RootFolder;
            }
            else
            {
                folder = (Model.Drive.DriveFolderBase)source;
            }

            if (null != folder)
            {
                var driveFolderTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.DriveFolder, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var driveFolderType = Type.GetType(driveFolderTypeName, true);
                var driveFileTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.DriveFile, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var driveFileType = Type.GetType(driveFileTypeName, true);

                if ((folder.DriveFolders != null &&
                    folder.DriveFolders.Count > 0) ||
                    (folder.DriveFiles != null &&
                    folder.DriveFiles.Count > 0))
                {
                    int itemsCount = (folder.DriveFolders?.Count ?? 0) + (folder.DriveFiles?.Count ?? 0);
                    var resultingItems = new Object[itemsCount];
                    var index = 0;

                    if (folder.DriveFolders != null)
                    {
                        foreach (var df in folder.DriveFolders)
                        {
                            var targetItem = Activator.CreateInstance(driveFolderType);
                            PnPObjectsMapper.MapProperties(df, targetItem, resolvers, recursive);
                            resultingItems.SetValue(targetItem, index);
                            index++;
                        }
                    }

                    if (folder.DriveFiles != null)
                    {
                        foreach (var df in folder.DriveFiles)
                        {
                            var targetItem = Activator.CreateInstance(driveFileType);
                            PnPObjectsMapper.MapProperties(df, targetItem, resolvers, recursive);
                            resultingItems.SetValue(targetItem, index);
                            index++;
                        }
                    }

                    result = resultingItems;
                }
            }







            return (result);
        }
    }
}
