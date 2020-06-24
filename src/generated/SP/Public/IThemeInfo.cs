using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ThemeInfo object
    /// </summary>
    [ConcreteType(typeof(ThemeInfo))]
    public interface IThemeInfo : IDataModel<IThemeInfo>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AccessibleDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThemeBackgroundImageUri { get; set; }

        #endregion

    }
}
