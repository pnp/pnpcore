using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers.V202103
{
    /// <summary>
    /// Allows resolving specific SiteCollection and SubSite types
    /// </summary>
    internal class SiteCollectionsAndSitesFromSchemaToModelTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;

        private readonly Type _targetItemType;

        public SiteCollectionsAndSitesFromSchemaToModelTypeResolver(Type targetItemType)
        {
            this._targetItemType = targetItemType;
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var itemType = typeof(List<>);
            var resultType = itemType.MakeGenericType(new Type[] { this._targetItemType });
            IList result = (IList)Activator.CreateInstance(resultType);

            var communicationSiteTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CommunicationSite, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var communicationSiteType = Type.GetType(communicationSiteTypeName, true);
            var teamSiteTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamSite, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamSiteType = Type.GetType(teamSiteTypeName, true);
            var teamSiteNoGroupTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamSiteNoGroup, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamSiteNoGroupType = Type.GetType(teamSiteNoGroupTypeName, true);
            var classicSiteTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ClassicSite, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var classicSiteType = Type.GetType(classicSiteTypeName, true);
            var teamSubSiteNoGroupTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamSubSiteNoGroup, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamSubSiteNoGroupType = Type.GetType(teamSubSiteNoGroupTypeName, true);

            var sourceCollection = source.GetPublicInstancePropertyValue("SiteCollections");
            if (sourceCollection == null)
            {
                sourceCollection = source.GetPublicInstancePropertyValue("Sites");
            }

            if (null != sourceCollection)
            {
                foreach (var i in (IEnumerable)sourceCollection)
                {
                    Object targetItem = null;

                    if (i.GetType().Name == communicationSiteType.Name)
                    {
                        targetItem = new Model.CommunicationSiteCollection();
                    }
                    else if (i.GetType().Name == teamSiteType.Name)
                    {
                        targetItem = new Model.TeamSiteCollection();
                    }
                    else if (i.GetType().Name == teamSiteNoGroupType.Name)
                    {
                        targetItem = new Model.TeamNoGroupSiteCollection();
                    }
                    else if (i.GetType().Name == classicSiteType.Name)
                    {
                        targetItem = new Model.ClassicSiteCollection();
                    }
                    else if (i.GetType().Name == teamSubSiteNoGroupType.Name)
                    {
                        targetItem = new Model.TeamNoGroupSubSite();
                    }

                    PnPObjectsMapper.MapProperties(i, targetItem, resolvers, recursive);

                    if (targetItem != null)
                    {
                        result.Add(targetItem);
                    }
                }
            }

            return (result);
        }
    }
}
