using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a collection of Drive Items from Schema to Domain Model
    /// </summary>
    internal class DriveItemsFromSchemaToModelTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;

        private readonly Type _targetItemType;

        public DriveItemsFromSchemaToModelTypeResolver(Type targetItemType)
        {
            this._targetItemType = targetItemType;
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var itemType = typeof(List<>);
            var resultType = itemType.MakeGenericType(new Type[] { this._targetItemType });
            IList result = (IList)Activator.CreateInstance(resultType);

            var driveItems = source.GetPublicInstancePropertyValue("Items");

            var driveFolderTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.DriveFolder, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var driveFolderType = Type.GetType(driveFolderTypeName, true);
            var driveFileTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.DriveFile, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var driveFileType = Type.GetType(driveFileTypeName, true);

            if (null != driveItems)
            {
                foreach (var d in (IEnumerable)driveItems)
                {
                    if (driveFolderType.IsInstanceOfType(d) && this._targetItemType == typeof(Model.Drive.DriveFolder))
                    {
                        var targetItem = new Model.Drive.DriveFolder();
                        PnPObjectsMapper.MapProperties(d, targetItem, resolvers, recursive);
                        result.Add(targetItem);
                    }
                    else if (driveFileType.IsInstanceOfType(d) && this._targetItemType == typeof(Model.Drive.DriveFile))
                    {
                        var targetItem = new Model.Drive.DriveFile();
                        PnPObjectsMapper.MapProperties(d, targetItem, resolvers, recursive);
                        result.Add(targetItem);
                    }
                }
            }

            return (result);
        }
    }
}
