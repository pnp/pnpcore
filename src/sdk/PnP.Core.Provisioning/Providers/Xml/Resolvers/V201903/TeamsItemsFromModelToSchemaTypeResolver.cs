using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves the Teams and Team Templates from the Model to the Schema
    /// </summary>
    internal class TeamsItemsFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            Object result = null;

            var teamsItemsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.BaseTeam, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamsItemsType = Type.GetType(teamsItemsTypeName, true);
            var teamTemplateTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamTemplateType = Type.GetType(teamTemplateTypeName, true);
            var teamWithSettingTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var teamWithSettingType = Type.GetType(teamWithSettingTypeName, true);

            var teams = source as Model.Teams.ProvisioningTeams;
            var teamsTemplates = teams?.TeamTemplates;
            var teamsWithSettings = teams?.Teams;

            if ((teamsTemplates != null && teamsTemplates.Count > 0) ||
                (teamsWithSettings != null && teamsWithSettings.Count > 0))
            {
                var resultingItems = Array.CreateInstance(teamsItemsType, teamsTemplates.Count + teamsWithSettings.Count);
                var index = 0;

                foreach (var tt in teamsTemplates)
                {
                    var targetItem = Activator.CreateInstance(teamTemplateType);
                    PnPObjectsMapper.MapProperties(tt, targetItem, resolvers, recursive);
                    resultingItems.SetValue(targetItem, index);
                    index++;
                }

                foreach (var ts in teamsWithSettings)
                {
                    var targetItem = Activator.CreateInstance(teamWithSettingType);
                    PnPObjectsMapper.MapProperties(ts, targetItem, resolvers, recursive);
                    resultingItems.SetValue(targetItem, index);
                    index++;
                }

                result = resultingItems;
            }

            return (result);
        }
    }
}
