using PnP.Core.Provisioning.Model;
using System;
using System.Diagnostics;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    internal static class CanvasSectionTypeMapper
    {
        internal static void ValidateSchemaValue(CanvasSectionType sectionType, Type schemaEnumType)
        {
            bool isFlexibleLayoutSection = sectionType == CanvasSectionType.FlexibleLayoutSection
                || sectionType == CanvasSectionType.FlexibleLayoutVerticalSection;

            if (!Enum.IsDefined(schemaEnumType, sectionType.ToString()) && !isFlexibleLayoutSection)
            {
                throw new NotSupportedException(
                    $"Schema {schemaEnumType.Namespace} does not support canvas section type '{sectionType}'. " +
                    "Use a schema version that supports this section type.");
            }
        }

        /// <summary>
        /// Converts a model section type to the value of the schema's CanvasSectionType enum.
        /// </summary>
        internal static object ToSchemaValue(CanvasSectionType sectionType, Type schemaEnumType)
        {
            ValidateSchemaValue(sectionType, schemaEnumType);

            string name = sectionType.ToString();

            if (!Enum.IsDefined(schemaEnumType, name)
                && (sectionType == CanvasSectionType.FlexibleLayoutSection || sectionType == CanvasSectionType.FlexibleLayoutVerticalSection))
            {
                name = sectionType == CanvasSectionType.FlexibleLayoutVerticalSection
                    && Enum.IsDefined(schemaEnumType, nameof(CanvasSectionType.OneColumnVerticalSection))
                        ? nameof(CanvasSectionType.OneColumnVerticalSection)
                        : nameof(CanvasSectionType.OneColumn);

                Trace.TraceWarning($"{Constants.LOGGING_SOURCE}: schema {schemaEnumType.Namespace} has no {sectionType} section type, " +
                    $"the section is saved as {name} and loses its flexible layout. Save the template with schema version 202609 or later to keep it.");
            }

            return Enum.Parse(schemaEnumType, name);
        }
    }
}
