namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List Page Render Type
    /// (e.g. https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/mt796270(v=office.15))
    /// </summary>
    public enum ListPageRenderType
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Undefined,
        MultipeWePart,
        JSLinkCustomization,
        XslLinkCustomization,
        NoSPList,
        HasBusinessDataField,
        HasTaskOutcomeField,
        HasPublishingfield,
        HasGeolocationField,
        HasCustomActionWithCode,
        HasMetadataNavFeature,
        SpecialViewType,
        ListTypeNoSupportForModernMode,
        AnonymousUser,
        ListSettingOff,
        SiteSettingOff,
        WebSettingOff,
        TenantSettingOff,
        CustomizedForm,
        DocLibNewForm,
        UnsupportedFieldTypeInForm,
        InvalidFieldTypeInForm,
        InvalidControModeInForm,
        CustomizedPage,
        ListTemplateNotSupported,
        WikiPage,
        DropOffLibrary,
        Modern
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
