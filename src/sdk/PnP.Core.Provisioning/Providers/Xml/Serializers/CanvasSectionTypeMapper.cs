using PnP.Core.Provisioning.Model;
using System;
using System.Diagnostics;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    internal static class CanvasSectionTypeMapper
    {
        /// <summary>
        /// Converts a model section type to the value of the schema's CanvasSectionType enum.
        /// </summary>
        internal static object ToSchemaValue(CanvasSectionType sectionType, Type schemaEnumType)
        {
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
