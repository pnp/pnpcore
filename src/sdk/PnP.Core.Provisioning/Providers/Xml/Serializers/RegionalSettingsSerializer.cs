using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CalendarType = PnP.Core.Provisioning.Model.CalendarType;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Regional Settings
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 400, DeserializationSequence = 400,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class RegionalSettingsSerializer : PnPBaseSchemaSerializer<Model.RegionalSettings>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var regionalSettings = persistence.GetPublicInstancePropertyValue("RegionalSettings");

            if (regionalSettings != null)
            {
                template.RegionalSettings = new Model.RegionalSettings();
                var expressions = new Dictionary<Expression<Func<Model.RegionalSettings, Object>>, IResolver>
                {
                    { s => s.WorkDayStartHour, new WorkHourFromSchemaToModelValueResolver() },
                    { s => s.WorkDayEndHour, new WorkHourFromSchemaToModelValueResolver() },
                    { s => s.CalendarType, new CalendarTypeFromSchemaToModelValueResolver() },
                    { s => s.AlternateCalendarType, new CalendarTypeFromSchemaToModelValueResolver() },
                    {
                        s => s.TimeZone,
                        new ExpressionValueResolver((s, v) =>
            !String.IsNullOrEmpty(v as string) ? Int32.Parse(v as string) : 0
                )
                    }
                };

                PnPObjectsMapper.MapProperties(regionalSettings, template.RegionalSettings, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.RegionalSettings != null)
            {
                var regionalSettingsType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.RegionalSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var target = Activator.CreateInstance(regionalSettingsType, true);
                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{regionalSettingsType}.AdjustHijriDaysSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.AlternateCalendarTypeSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.CalendarTypeSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.CollationSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.FirstDayOfWeekSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.FirstWeekOfYearSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.LocaleIdSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.ShowWeeksSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.Time24Specified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.WorkDayEndHourSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.WorkDaysSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.WorkDayStartHourSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{regionalSettingsType}.WorkDayStartHour", new ExpressionValueResolver<Model.WorkHour>(v => v.FromTemplateToSchemaWorkHourV201605()) },
                    { $"{regionalSettingsType}.WorkDayEndHour", new ExpressionValueResolver<Model.WorkHour>(v => v.FromTemplateToSchemaWorkHourV201605()) },
                    { $"{regionalSettingsType}.CalendarType", new ExpressionValueResolver<CalendarType>(v => v.FromTemplateToSchemaCalendarTypeV201605()) },
                    { $"{regionalSettingsType}.AlternateCalendarType", new ExpressionValueResolver<CalendarType>(v => v.FromTemplateToSchemaCalendarTypeV201605()) }
                };


                PnPObjectsMapper.MapProperties(template.RegionalSettings, target, expressions, recursive: true);

                persistence.GetPublicInstanceProperty("RegionalSettings").SetValue(persistence, target);
            }
        }
    }
}
