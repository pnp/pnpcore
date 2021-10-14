using System.Collections.Generic;

namespace PnP.Core.Transformation.SharePoint.Publishing
{
    /// <summary>
    /// Contains a central point for defaults for publishing page processing
    /// </summary>
    internal static class PublishingDefaults
    {
        /// <summary>
        /// Field Control Defaults for mappings
        /// </summary>
        internal static List<PageLayoutWebPartFieldEntity> WebPartFieldProperties = new List<PageLayoutWebPartFieldEntity>()
        {
            new PageLayoutWebPartFieldEntity() { TargetWebPart = "SharePointPnP.Modernization.WikiImagePart", FieldType = "Image",  Name="ImageUrl", Type="string", Functions = "ToImageUrl({PublishingPageImage})" },
            new PageLayoutWebPartFieldEntity() { TargetWebPart = "SharePointPnP.Modernization.WikiImagePart", FieldType = "Image",  Name="AlternativeText", Type="string", Functions = "ToImageAltText({PublishingPageImage})" },
            new PageLayoutWebPartFieldEntity() { TargetWebPart = "SharePointPnP.Modernization.WikiImagePart", FieldType = "Image",  Name="Anchor", Type="string", Functions = "ToImageAnchor({PublishingPageImage})" },
            new PageLayoutWebPartFieldEntity() { TargetWebPart = "SharePointPnP.Modernization.WikiImagePart", FieldType = "Image",  Name="Caption", Type="string", Functions = "ToImageCaption({PublishingImageCaption})" },

            new PageLayoutWebPartFieldEntity() { TargetWebPart = "Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart, Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", FieldType = "SummaryLinks", Name = "SummaryLinkStore", Type="string" },
            new PageLayoutWebPartFieldEntity() { TargetWebPart = "Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart, Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", FieldType = "SummaryLinks", Name = "Title", Type="string", Functions="EmptyString()" },

            new PageLayoutWebPartFieldEntity() { TargetWebPart = "SharePointPnP.Modernization.WikiTextPart", FieldType = "HTML", Name="Text", Type="string" },
        };

        /// <summary>
        /// Metadata field default mappings
        /// </summary>
        internal static List<PageLayoutMetadataEntity> MetaDataFieldToTargetMappings = new List<PageLayoutMetadataEntity>()
        {
            new PageLayoutMetadataEntity(){ FieldName = "Title", TargetFieldName="", Functions = "" },
        };

        /// <summary>
        /// Field to header mappings
        /// </summary>
        internal static List<PageLayoutHeaderFieldEntity> PageLayoutHeaderMetadata = new List<PageLayoutHeaderFieldEntity>()
        {
            new PageLayoutHeaderFieldEntity() { Type = "FullWidthImage", Alignment="Left", ShowPublishedDate = false, Name = "PublishingRollupImage", HeaderProperty = "ImageServerRelativeUrl", Functions = "ToImageUrl({PublishingRollupImage})" },
            new PageLayoutHeaderFieldEntity() { Type = "FullWidthImage", Alignment="Left", ShowPublishedDate = false, Name="ArticleByLine", HeaderProperty = "TopicHeader", Functions = "" },
            new PageLayoutHeaderFieldEntity() { Type = "FullWidthImage", Alignment="Left", ShowPublishedDate = false, Name="PublishingContact", HeaderProperty = "Authors", Functions = "ToAuthors({PublishingContact})" },
        };

        /// <summary>
        /// List of metadata fields in content types to ignore in mappings
        /// </summary>
        internal static List<string> IgnoreMetadataFields = new List<string>()
        {
            // System fields that will get their defaults via the target modern page
            "ContentType",
            "FileLeafRef",
            "Modified_x0020_By",
            "Created_x0020_By",
            // Fields that have no functional meaning anymore in modern publishing
            "RobotsNoIndex",
            "SeoBrowserTitle",
            "SeoMetaDescription",
            "SeoKeywords",
            "PublishingPageLayout",
            // We're not (yet) mapping audiences, so skip for now
            "Audience",
        };

        /// <summary>
        /// List of field controls in page to ignore in mappings
        /// </summary>
        internal static List<string> IgnoreWebPartFieldControls = new List<string>()
        {
            "HeaderStyleDefinitions",
            // Should go inside the image web part transformation, excluding for now
            "PublishingImageCaption",
            // Will be used in the header...so it will be used as preview image in the modern page...
            "PublishingRollupImage",
        };
    }

}
