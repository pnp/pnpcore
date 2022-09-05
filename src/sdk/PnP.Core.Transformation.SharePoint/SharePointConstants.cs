using System;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// SharePoint constants
    /// </summary>
    internal static class SharePointConstants
    {
        // Client tag and user agent
        public const string TransformationClientTag = "SPDev:PageTransformator";
        public const string TransformationUserAgent = "";
        // Web Properties
        public const string WebPropertyKeyPagesListId = "__PagesListId";

        // Schemas
        public const string PageTransformationSchema = "http://schemas.dev.office.com/PnP/2018/01/PageTransformationSchema";
        public const string PageLayoutMappingSchema = "http://schemas.dev.office.com/PnP/2019/03/PublishingPageTransformationSchema";

        // Fields
        public const string FileRefField = "FileRef";
        public const string FileDirRefField = "FileDirRef";
        public const string FileLeafRefField = "FileLeafRef";
        public const string FileTitleField = "Title";
        public const string ClientSideApplicationIdField = "ClientSideApplicationId";
        public const string HtmlFileTypeField = "HTML_x0020_File_x0020_Type";
        public const string WikiField = "WikiField";
        public const string ModifiedField = "Modified";
        public const string ModifiedByField = "Editor";
        public const string CreatedField = "Created";
        public const string CreatedByField = "Author";
        public const string ContentTypeIdField = "ContentTypeId";
        public const string PublishingPageLayoutField = "PublishingPageLayout";
        public const string AudienceField = "Audience";
        public const string PublishingRollupImageField = "PublishingRollupImage";
        public const string TitleField = "Title";
        public const string PublishingAssociatedContentTypeField = "PublishingAssociatedContentType";
        public const string SPSitePageFlagsField = "_SPSitePageFlags";
        public const string PromotedStateField = "PromotedState";
        public const string FirstPublishedDateField = "FirstPublishedDate";
        public const string BannerImageUrlField = "BannerImageUrl";
        public const string CanvasContentField = "CanvasContent1";
        public const string IDField = "ID";
        public const string BodyField = "Body"; // Blog pages
        public const string PublishedDateField = "PublishedDate"; // Blog pages
        public const string FileTypeField = "File_x0020_Type"; // Delve blog pages

        // Content Type ID's
        public const string PageLayoutBaseContentTypeId = "0x01010007FF3E057FA8AB4AA42FCB67B453FFC1"; //Page Layout Content Type Id
        public const string ModernPageContentTypeId = "0x0101009D1CB255DA76424F860D91F20E6C4118";

        // Field ID's
        public static readonly Guid PostCategory = new Guid("38bea83b-350a-1a6e-f34a-93a6af31338b");

        // Features
        public static readonly Guid FeatureId_Web_ModernPage = new Guid("B6917CB1-93A0-4B97-A84D-7CF49975D4EC");

        // Mapping properties
        public const string UseCommunityScriptEditorMappingProperty = "UseCommunityScriptEditor";
        public const string SummaryLinksToQuickLinksMappingProperty = "SummaryLinksToQuickLinks";

        // Queries
        public const string CAMLQueryByExtension = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <Contains>
                        <FieldRef Name='File_x0020_Type'/>
                        <Value Type='text'>aspx</Value>
                      </Contains>
                    </Where>
                  </Query>
                </View>";
        public const string CAMLQueryByExtensionAndName = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <And>
                        <Contains>
                          <FieldRef Name='File_x0020_Type'/>
                          <Value Type='text'>aspx</Value>
                        </Contains>
                        <BeginsWith>
                          <FieldRef Name='FileLeafRef'/>
                          <Value Type='text'>{0}</Value>
                        </BeginsWith>
                      </And>
                    </Where>
                  </Query>
                </View>";
        public const string CAMLQueryByNameForBlog = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <BeginsWith>
                        <FieldRef Name='Title'/>
                        <Value Type='text'>{0}</Value>
                      </BeginsWith>
                    </Where>
                  </Query>
                </View>";

        // Cross site assets transfers that are currently allowed
        public static string[] AllowedAssetFileExtensions = new string[] { "png", "jpg", "gif", "mp4", "mpeg" };
        // Cross site assets transfer blocked files that are referred to
        public static string[] BlockedAssetFileExtensions = new string[] { "aspx" };
        // OOB Publishing Portal page layouts - these layouts are already included in the default mapping file
        public static string[] OobPublishingPageLayouts = new string[] { "ArticleLeft", "ArticleLinks", "ArticleRight", "BlankWebPartPage", "ProjectPage", "WelcomeLinks",
                                                                         "PageFromDocLayout", "WelcomeSplash", "ErrorLayout", "EnterpriseWiki", "CatalogArticle", "CatalogWelcome",
                                                                         "PageLayoutTemplate", "RedirectPageLayout", "VariationRootPageLayout" };


        public const string HeaderTitle = "HeaderTitle";
    }
}
