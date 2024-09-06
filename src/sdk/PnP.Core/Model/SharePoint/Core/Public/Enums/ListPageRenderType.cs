namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List Page Render Type. Reasons why the page is rendered in classic UX, or Modern if the page is in Modern UX.
    /// (e.g. https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/mt796270(v=office.15))
    /// </summary>
    public enum ListPageRenderType
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        /// <summary>
        /// We don't have enough infomation to know the render mode. Will also happen if the linked file does not belong to a list
        /// </summary>
        Undefined = 0, 
        MultipeWePart = 1,
        JSLinkCustomization = 2,
        XslLinkCustomization = 3,
        NoSPList = 4,
        HasBusinessDataField = 5,
        HasTaskOutcomeField = 6,
        HasPublishingfield = 7,
        HasGeolocationField = 8,
        HasCustomActionWithCode = 9,
        HasMetadataNavFeature = 10,
        SpecialViewType = 11,
        ListTypeNoSupportForModernMode = 12,
        AnonymousUser = 13,
        ListSettingOff = 14,
        SiteSettingOff = 15,
        WebSettingOff = 16,
        TenantSettingOff = 17,
        CustomizedForm = 18,
        DocLibNewForm = 19,
        UnsupportedFieldTypeInForm = 20,
        InvalidFieldTypeInForm = 21,
        InvalidControModeInForm = 22,
        CustomizedPage = 23,
        ListTemplateNotSupported = 24,
        WikiPage = 25,
        DropOffLibrary = 26,
        IsUnghosted = 27,
        /// <summary>
        /// Page renders in Modern UX. All others are reasons/modes to render in classic UX
        /// </summary>
        Modern = 100,              
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
