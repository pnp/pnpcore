using PnP.Core.Provisioning.Model;
using System;
using System.Diagnostics;
using System.Linq;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    internal static class CanvasSectionTypeMapper
    {
        private const string FlexibleLayoutSectionFactor = "\"sectionFactor\":100";

        /// <summary>
        /// Restores the flexible layout sections that schemas without flexible layout section types store as the
        /// one-column sections they are built on. The flexible layout is recognized from the control data.
        /// </summary>
        internal static void RestoreFlexibleLayoutSections(BaseClientSidePage page, Type schemaEnumType)
        {
            if (page?.Sections == null || Enum.IsDefined(schemaEnumType, nameof(CanvasSectionType.FlexibleLayoutSection)))
            {
                return;
            }

            foreach (var section in page.Sections.Where(s => s.Type == CanvasSectionType.OneColumn || s.Type == CanvasSectionType.OneColumnVerticalSection))
            {
                if (section.Controls != null && section.Controls.Any(c => !string.IsNullOrWhiteSpace(c.JsonControlData) && c.JsonControlData.Contains(FlexibleLayoutSectionFactor)))
                {
                    section.Type = section.Type == CanvasSectionType.OneColumn ? CanvasSectionType.FlexibleLayoutSection : CanvasSectionType.FlexibleLayoutVerticalSection;
                }
            }
        }

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
                    $"the section is saved as {name} and its flexible layout is kept only in the control data. Save the template with schema version 202609 or later to keep it.");
            }

            return Enum.Parse(schemaEnumType, name);
        }
    }
}
