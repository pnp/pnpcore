namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// StorageMetrics class, write your custom code here
    /// </summary>
    [SharePointType("SP.StorageMetrics", Target = typeof(IFolder), Uri = "_api/web/getFolderById('{Parent.Id}')/StorageMetrics", LinqGet = "_api/web/getFolderById('{Parent.Id}')/StorageMetrics")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class StorageMetrics
    {
        public StorageMetrics()
        {

        }
    }
}
