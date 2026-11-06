using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Teams;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Teams settings
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201903,
        SerializationSequence = 100, DeserializationSequence = 100,
        Scope = SerializerScope.Tenant)]
    internal class TeamsSerializer : PnPBaseSchemaSerializer<ProvisioningTeams>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var teams = persistence.GetPublicInstancePropertyValue("Teams");

            if (teams != null)
            {
                var expressions = new Dictionary<Expression<Func<ProvisioningTeams, Object>>, IResolver>
                {

                    { t => t.TeamTemplates, new TeamTemplatesFromSchemaToModelTypeResolver() },
                    {
                        t => t.TeamTemplates[0].JsonTemplate,
                        new ExpressionValueResolver((s, v) =>
{
                    return ((s.GetPublicInstancePropertyValue("Text") as String[])?.Aggregate(String.Empty, (acc, next) => acc += (next != null ? next : String.Empty)));
})
                    },

                    { t => t.Teams, new TeamsFromSchemaToModelTypeResolver() },
                    {
                        t => t.Teams[0].FunSettings,
                        new ComplexTypeFromSchemaToModelTypeResolver<TeamFunSettings>("FunSettings")
                    },
                    {
                        t => t.Teams[0].GuestSettings,
                        new ComplexTypeFromSchemaToModelTypeResolver<TeamGuestSettings>("GuestSettings")
                    },
                    {
                        t => t.Teams[0].MemberSettings,
                        new ComplexTypeFromSchemaToModelTypeResolver<TeamMemberSettings>("MembersSettings")
                    },
                    {
                        t => t.Teams[0].MessagingSettings,
                        new ComplexTypeFromSchemaToModelTypeResolver<TeamMessagingSettings>("MessagingSettings")
                    },
                    {
                        t => t.Teams[0].Security,
                        new TeamSecurityFromSchemaToModelTypeResolver()
                    },

                    {
                        t => t.Teams[0].Channels[0].Tabs[0].Configuration,
                        new ComplexTypeFromSchemaToModelTypeResolver<TeamTabConfiguration>("Configuration")
                    },

                    { t => t.Teams[0].Channels[0].Messages[0].Message, new ExpressionValueResolver((s, v) => s) }
                };

                PnPObjectsMapper.MapProperties(teams, template.ParentHierarchy.Teams, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.ParentHierarchy?.Teams != null &&
                (template.ParentHierarchy?.Teams?.Apps != null ||
                template.ParentHierarchy?.Teams?.Teams != null ||
                template.ParentHierarchy?.Teams?.TeamTemplates != null))
            {
                var teamsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Teams, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var teamsType = Type.GetType(teamsTypeName, false);

                if (teamsType != null)
                {
                    var teamTemplateTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamTemplateType = Type.GetType(teamTemplateTypeName, true);
                    var teamWithSettingTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamWithSettingType = Type.GetType(teamWithSettingTypeName, true);
                    var teamChannelTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamChannel, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamChannelType = Type.GetType(teamChannelTypeName, true);

                    var teamFunSettingsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettingsFunSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamFunSettingsType = Type.GetType(teamFunSettingsTypeName, true);
                    var teamGuestSettingsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettingsGuestSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamGuestSettingsType = Type.GetType(teamGuestSettingsTypeName, true);
                    var teamMembersSettingsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettingsMembersSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamMembersSettingsType = Type.GetType(teamMembersSettingsTypeName, true);
                    var teamMessagingSettingsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamWithSettingsMessagingSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamMessagingSettingsType = Type.GetType(teamMessagingSettingsTypeName, true);
                    var teamChannelTabTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamChannelTabsTab, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamChannelTabType = Type.GetType(teamChannelTabTypeName, true);
                    var teamChannelTabConfigurationTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.TeamChannelTabsTabConfiguration, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var teamChannelTabConfigurationType = Type.GetType(teamChannelTabConfigurationTypeName, true);

                    var target = Activator.CreateInstance(teamsType, true);

                    var resolvers = new Dictionary<String, IResolver>
                    {

                        {
                            $"{teamsType}.Items",
                            new TeamsItemsFromModelToSchemaTypeResolver()
                        },

                        {
                            $"{teamTemplateType}.Text",
                            new ExpressionValueResolver((s, v) =>
{
                        return (new String[1] { (s as TeamTemplate)?.JsonTemplate });
})
                        },

                        {
                            $"{teamWithSettingType}.Security",
                            new TeamSecurityFromModelToSchemaTypeResolver()
                        },

                        {
                            $"{teamWithSettingType}.FunSettings",
                            new ComplexTypeFromModelToSchemaTypeResolver(teamFunSettingsType, "FunSettings")
                        },
                        {
                            $"{teamWithSettingType}.GuestSettings",
                            new ComplexTypeFromModelToSchemaTypeResolver(teamGuestSettingsType, "GuestSettings")
                        },
                        {
                            $"{teamWithSettingType}.MembersSettings",
                            new ComplexTypeFromModelToSchemaTypeResolver(teamMembersSettingsType, "MemberSettings")
                        },
                        {
                            $"{teamWithSettingType}.MessagingSettings",
                            new ComplexTypeFromModelToSchemaTypeResolver(teamMessagingSettingsType, "MessagingSettings")
                        },

                        {
                            $"{teamChannelType}.Messages",
                            new ExpressionValueResolver((s, v) =>
{
                        return ((s as TeamChannel)?.Messages.Count > 0 ? (s as TeamChannel)?.Messages.Select(m => m.Message).ToArray() : null);
})
                        },

                        {
                            $"{teamChannelTabType}.Configuration",
                            new ComplexTypeFromModelToSchemaTypeResolver(teamChannelTabConfigurationType, "Configuration")
                        }
                    };

                    PnPObjectsMapper.MapProperties(template.ParentHierarchy.Teams, target, resolvers, recursive: true);

                    if (target != null &&
                        (target.GetPublicInstancePropertyValue("Apps") != null ||
                        target.GetPublicInstancePropertyValue("Items") != null))
                    {
                        persistence.GetPublicInstanceProperty("Teams").SetValue(persistence, target);
                    }
                }
            }
        }
    }
}
