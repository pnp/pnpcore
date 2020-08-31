namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a PropertyValues object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    [SharePointType("SP.PropertyValues", Target = typeof(IFolder), Uri = "_api/web/getFolderById('{Parent.Id}')/Properties", LinqGet = "_api/web/getFolderById('{Parent.Id}')/Properties")]
    internal partial class PropertyValues : ExpandoBaseComplexType<IPropertyValues>, IPropertyValues
    {
    }
}
