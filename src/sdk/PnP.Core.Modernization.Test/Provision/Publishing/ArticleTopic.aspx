<%@ Page language="C#"   Inherits="Microsoft.SharePoint.Publishing.PublishingLayoutPage,Microsoft.SharePoint.Publishing,Version=16.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" meta:progid="SharePoint.WebPartPage.Document" %>
<%@ Register Tagprefix="SharePointWebControls" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="PublishingWebControls" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="PublishingNavigation" Namespace="Microsoft.SharePoint.Publishing.Navigation" Assembly="Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceholderID="PlaceHolderAdditionalPageHead" runat="server">
	<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Themable/Core Styles/pagelayouts15.css %>" runat="server"/>
	<PublishingWebControls:EditModePanel runat="server">
		<!-- Styles for edit mode only-->
		<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Themable/Core Styles/editmode15.css %>"
			After="<% $SPUrl:~sitecollection/Style Library/~language/Themable/Core Styles/pagelayouts15.css %>" runat="server"/>
	</PublishingWebControls:EditModePanel>
	<SharePointWebControls:FieldValue id="PageStylesField" FieldName="HeaderStyleDefinitions" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderPageTitle" runat="server">
	<SharePointWebControls:FieldValue id="PageTitle" FieldName="Title" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderPageTitleInTitleArea" runat="server">
	<SharePointWebControls:FieldValue FieldName="Title" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleBreadcrumb" runat="server"> 
	<SharePointWebControls:ListSiteMapPath runat="server" SiteMapProviders="CurrentNavigationSwitchableProvider" RenderCurrentNodeAsLink="false" PathSeparator="" CssClass="s4-breadcrumb" NodeStyle-CssClass="s4-breadcrumbNode" CurrentNodeStyle-CssClass="s4-breadcrumbCurrentNode" RootNodeStyle-CssClass="s4-breadcrumbRootNode" NodeImageOffsetX=0 NodeImageOffsetY=289 NodeImageWidth=16 NodeImageHeight=16 NodeImageUrl="/_layouts/15/images/fgimg.png?rev=45" HideInteriorRootNodes="true" SkipLinkText=""/> </asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderMain" runat="server">
	<div class="article article-left">
		<PublishingWebControls:EditModePanel runat="server" CssClass="edit-mode-panel title-edit">
			<SharePointWebControls:TextField runat="server" FieldName="Title"/>
		</PublishingWebControls:EditModePanel>
		<div class="captioned-image">
			<div class="image">
				<PublishingWebControls:RichImageField FieldName="PublishingPageImage" runat="server"/>
			</div>
			<div class="caption">
				<PublishingWebControls:RichHtmlField FieldName="PublishingImageCaption"  AllowTextMarkup="false" AllowTables="false" AllowLists="false" AllowHeadings="false" AllowStyles="false" AllowFontColorsMenu="false" AllowParagraphFormatting="false" AllowFonts="false" PreviewValueSize="Small" AllowInsert="false" AllowEmbedding="false" AllowDragDrop="false" runat="server"/>
			</div>
		</div>
		<div class="article-header">
			<div class="date-line">
				<SharePointWebControls:DateTimeField FieldName="ArticleStartDate" runat="server"/>
			</div>
			<div class="by-line">
				<SharePointWebControls:TextField FieldName="ArticleByLine" runat="server"/>
			</div>
		</div>
		<div class="article-content">
			<PublishingWebControls:RichHtmlField FieldName="PublishingPageContent" HasInitialFocus="True" MinimumEditHeight="400px" runat="server"/>
			<WebPartPages:ContentEditorWebPart webpart="true" runat="server" __WebPartId="{DB666743-4C5B-4A21-A9CF-7A199CE19A60}">
				<WebPart xmlns="http://schemas.microsoft.com/WebPart/v2">
					<Title>Example Embedded Web Content Editor</Title>
                    <FrameType>None</FrameType>
					<PartImageLarge>/_layouts/15/images/mscontl.gif</PartImageLarge>
					<ID>g_db666743_4c5b_4a21_a9cf_7a199ce19a60</ID>
					<Content xmlns="http://schemas.microsoft.com/WebPart/v2/ContentEditor"><![CDATA[Welcome to the home of all SharePoint modernization tools and solutions. Modernization means transforming current "classic" SharePoint usage into "modern" usage, e.g. using modern site pages instead of classic wiki pages or Office 365 Group connecting of classic sites. In this repository you'll find all the SharePoint PnP open source scripts, tools and solutions that will help you modernize your SharePoint environment.]]></Content>
				</WebPart>
			</WebPartPages:ContentEditorWebPart>

            <WebPartPages:ImageWebPart runat="server" ImageLink="/sites/PnPTransformationSource/SiteCollectionImages/hero.jpg" AlternativeText="Drone flying in the air" VerticalAlignment="Middle" HorizontalAlignment="Center" BackgroundColor="transparent" Title="Let's go fly a drone" FrameType="None" SuppressWebPartChrome="False" Description="Lets take to the skies with our new drone" IsIncluded="True" ZoneID="" PartOrder="0" FrameState="Normal" AllowRemove="True" AllowZoneChange="True" AllowMinimize="True" AllowConnect="True" AllowEdit="True" AllowHide="True" IsVisible="True" CatalogIconImageUrl="/_layouts/15/images/msimagel.gif" DetailLink="" HelpLink="" HelpMode="Modeless" Dir="Default" PartImageSmall="" MissingAssembly="Cannot import this Web Part." PartImageLarge="/_layouts/15/images/msimagel.gif" IsIncludedFilter="" ExportControlledProperties="True" ConnectionID="00000000-0000-0000-0000-000000000000" ID="g_4c8dec0d_e920_4c7e_bdfa_6532864b3904" ExportMode="All" __MarkupType="vsattributemarkup" __WebPartId="{4c8dec0d-e920-4c7e-bdfa-6532864b3904}" WebPart="true" Height="" Width=""></WebPartPages:ImageWebPart>
			

			<WebPartPages:WebPartZone id="x00001a" runat="server" title="Main 100 1"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			<WebPartPages:WebPartZone id="x00002b" runat="server" title="Main 100 2"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			<WebPartPages:WebPartZone id="x00003c" runat="server" title="Main 100 3"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			<WebPartPages:WebPartZone id="x00004d" runat="server" title="Main 100 4"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
		</div>
		<PublishingWebControls:EditModePanel runat="server" CssClass="edit-mode-panel roll-up">
			<PublishingWebControls:RichImageField FieldName="PublishingRollupImage" AllowHyperLinks="false" runat="server" />
			<asp:Label text="<%$Resources:cms,Article_rollup_image_text15%>" CssClass="ms-textSmall" runat="server" />
		</PublishingWebControls:EditModePanel>
		
	</div>
</asp:Content>