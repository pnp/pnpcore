<%@ Page language="C#"   Inherits="Microsoft.SharePoint.Publishing.PublishingLayoutPage,Microsoft.SharePoint.Publishing,Version=14.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePointWebControls" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="PublishingWebControls" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="PublishingNavigation" Namespace="Microsoft.SharePoint.Publishing.Navigation" Assembly="Microsoft.SharePoint.Publishing, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceholderID="PlaceHolderAdditionalPageHead" runat="server">
	<SharePointWebControls:UIVersionedContent UIVersion="3" runat="server">
		<ContentTemplate>
			<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/pageLayouts.css %>" runat="server"/>
			<PublishingWebControls:editmodepanel runat="server" id="editmodestyles">
				<!-- Styles for edit mode only-->
				<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/zz2_editMode.css %>" runat="server"/>
			</PublishingWebControls:editmodepanel>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
	<SharePointWebControls:UIVersionedContent UIVersion="4" runat="server">
		<ContentTemplate>
			<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/page-layouts-21.css %>" runat="server"/>
			<PublishingWebControls:EditModePanel runat="server">
				<!-- Styles for edit mode only-->
				<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/edit-mode-21.css %>"
					After="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/page-layouts-21.css %>" runat="server"/>
			</PublishingWebControls:EditModePanel>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
	<SharePointWebControls:CssRegistration name="<% $SPUrl:~sitecollection/Style Library/~language/Core Styles/rca.css %>" runat="server"/>
	<SharePointWebControls:FieldValue id="PageStylesField" FieldName="HeaderStyleDefinitions" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderPageTitle" runat="server">
	<SharePointWebControls:FieldValue id="PageTitle" FieldName="Title" runat="server"/>
</asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderPageTitleInTitleArea" runat="server">
	<SharePointWebControls:UIVersionedContent UIVersion="3" runat="server">
		<ContentTemplate>
			<SharePointWebControls:TextField runat="server" id="TitleField" FieldName="Title"/>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
	<SharePointWebControls:UIVersionedContent UIVersion="4" runat="server">
		<ContentTemplate>
			<SharePointWebControls:FieldValue FieldName="Title" runat="server"/>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleBreadcrumb" runat="server"> <SharePointWebControls:VersionedPlaceHolder UIVersion="3" runat="server"> <ContentTemplate> <asp:SiteMapPath ID="siteMapPath" runat="server" SiteMapProvider="CurrentNavigation" RenderCurrentNodeAsLink="false" SkipLinkText="" CurrentNodeStyle-CssClass="current" NodeStyle-CssClass="ms-sitemapdirectional"/> </ContentTemplate> </SharePointWebControls:VersionedPlaceHolder> <SharePointWebControls:UIVersionedContent UIVersion="4" runat="server"> <ContentTemplate> <SharePointWebControls:ListSiteMapPath runat="server" SiteMapProviders="CurrentNavigation" RenderCurrentNodeAsLink="false" PathSeparator="" CssClass="s4-breadcrumb" NodeStyle-CssClass="s4-breadcrumbNode" CurrentNodeStyle-CssClass="s4-breadcrumbCurrentNode" RootNodeStyle-CssClass="s4-breadcrumbRootNode" NodeImageOffsetX=0 NodeImageOffsetY=353 NodeImageWidth=16 NodeImageHeight=16 NodeImageUrl="/_layouts/images/fgimg.png" HideInteriorRootNodes="true" SkipLinkText="" /> </ContentTemplate> </SharePointWebControls:UIVersionedContent> </asp:Content>
<asp:Content ContentPlaceholderID="PlaceHolderMain" runat="server">
	<SharePointWebControls:UIVersionedContent UIVersion="3" runat="server">
		<ContentTemplate>
			<div style="clear:both">&#160;</div>
			<table class="floatLeft" cellspacing=0 cellpadding=0>
				<tr>
					<td class="image">
						<PublishingWebControls:RichImageField id="ImageField" FieldName="PublishingPageImage" runat="server"/>
					</td>
				</tr>
				<tr>
					<td class="caption">
						<PublishingWebControls:RichHtmlField id="Caption" FieldName="PublishingImageCaption"  AllowTextMarkup="false" AllowTables="false" AllowFonts="false" PreviewValueSize="Small" runat="server"/>
					</td>
				</tr>
			</table>
			<table class="header">
				<tr>
					<td class="dateLine">
						<SharePointWebControls:datetimefield FieldName="ArticleStartDate" runat="server" id="datetimefield3"></SharePointWebControls:datetimefield>
					</td>
					<td width=100% class="byLine">
						<SharePointWebControls:TextField FieldName="ArticleByLine" runat="server"/>
					</td>
				</tr>
			</table>
			<div class="pageContent">
				<PublishingWebControls:RichHtmlField id="Content" FieldName="PublishingPageContent" runat="server"/>
			</div>
			<PublishingWebControls:editmodepanel runat="server" id="editmodepanel1">
				<!-- Add field controls here to bind custom metadata viewable and editable in edit mode only.-->
				<table cellpadding="10" cellspacing="0" align="center" class="editModePanel">
					<tr>
						<td>
							<PublishingWebControls:RichImageField id="ContentQueryImage" FieldName="PublishingRollupImage" AllowHyperLinks="false" runat="server"/>
						</td>
						<td width="200">
							<asp:label text="<%$Resources:cms,Article_rollup_image_text%>" runat="server" />
						</td>
					</tr>
				</table>
			</PublishingWebControls:editmodepanel>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
	<SharePointWebControls:UIVersionedContent UIVersion="4" runat="server">
		<ContentTemplate>
			<div class="article article-left">
				<PublishingWebControls:EditModePanel runat="server" CssClass="edit-mode-panel">
					<SharePointWebControls:TextField runat="server" FieldName="Title"/>
				</PublishingWebControls:EditModePanel>
				<div class="captioned-image">
					<div class="image">
						<PublishingWebControls:RichImageField FieldName="PublishingPageImage" runat="server"/>
					</div>
					<div class="caption">
						<PublishingWebControls:RichHtmlField FieldName="PublishingImageCaption"  AllowTextMarkup="false" AllowTables="false" AllowLists="false" AllowHeadings="false" AllowStyles="false" AllowFontColorsMenu="false" AllowParagraphFormatting="false" AllowFonts="false" PreviewValueSize="Small" AllowInsert="false" runat="server"/>
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
				</div>
                <div>
                    <WebPartPages:WebPartZone id="x00001a" runat="server" title="Main 100 1"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			        <WebPartPages:WebPartZone id="x00002b" runat="server" title="Main 100 2"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			        <WebPartPages:WebPartZone id="x00003c" runat="server" title="Main 100 3"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
			        <WebPartPages:WebPartZone id="x00004d" runat="server" title="Main 100 4"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
                </div>
				<PublishingWebControls:EditModePanel runat="server" CssClass="edit-mode-panel roll-up">
					<PublishingWebControls:RichImageField FieldName="PublishingRollupImage" AllowHyperLinks="false" runat="server" />
					<asp:Label text="<%$Resources:cms,Article_rollup_image_text%>" runat="server" />
				</PublishingWebControls:EditModePanel>
			</div>
		</ContentTemplate>
	</SharePointWebControls:UIVersionedContent>
</asp:Content>
