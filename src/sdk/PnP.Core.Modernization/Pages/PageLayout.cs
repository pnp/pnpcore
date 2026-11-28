namespace PnP.Core.Modernization.Pages
{
    /// <summary>
    /// Possible layouts used by wiki or webpart pages
    /// </summary>
    public enum PageLayout
    {
        Wiki_OneColumn = 0,
        Wiki_TwoColumns = 1,
        Wiki_TwoColumnsWithSidebar = 2,
        Wiki_TwoColumnsWithHeader = 3,
        Wiki_TwoColumnsWithHeaderAndFooter = 4,
        Wiki_ThreeColumns = 5,
        Wiki_ThreeColumnsWithHeader = 6,
        Wiki_ThreeColumnsWithHeaderAndFooter = 7,
        Wiki_Custom = 8,
        WebPart_HeaderFooterThreeColumns = 20,
        WebPart_FullPageVertical = 21,
        WebPart_HeaderLeftColumnBody = 22,
        WebPart_HeaderRightColumnBody = 23,
        WebPart_HeaderFooter2Columns4Rows = 24,
        WebPart_HeaderFooter4ColumnsTopRow = 25,
        WebPart_LeftColumnHeaderFooterTopRow3Columns = 26,
        WebPart_RightColumnHeaderFooterTopRow3Columns = 27,
        WebPart_Custom = 28,
        WebPart_2010_TwoColumnsLeft = 29,
        WebPart_DocumentWorkspace = 30,
        WebPart_ProjectSite = 31,
        WebPart_DocumentCenter = 32,
        PublishingPage_AutoDetect = 40,
        PublishingPage_AutoDetectWithVerticalColumn = 41,
        PublishingPage_TwoColumnRightVerticalSection = 42,
        PublishingPage_TwoColumnLeftVerticalSection = 43,
    }
}
