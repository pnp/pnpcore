using System;
using System.Collections;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a collection type from Domain Model to Schema
    /// </summary>
    internal class CollectionFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;

        private readonly Type _targetItemType;

        public CollectionFromModelToSchemaTypeResolver(Type targetItemType)
        {
            this._targetItemType = targetItemType;
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            object result = null;
            if (null != source)
            {
                var sourceList = (IList)source; // This throws an exception if source is empty

                if (sourceList.Count > 0)
                {
                    var resultArray = Array.CreateInstance(this._targetItemType, sourceList.Count);

                    var index = 0;
                    foreach (var i in sourceList)
                    {
                        var targetItem = Activator.CreateInstance(this._targetItemType, true);
                        PnPObjectsMapper.MapProperties(i, targetItem, resolvers, recursive);
                        resultArray.SetValue(targetItem, index);
                        index++;
                    }
                    result = resultArray;
                }
            }
            return (result);
        }
    }
}
