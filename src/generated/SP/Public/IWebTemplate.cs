using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a WebTemplate object
    /// </summary>
    [ConcreteType(typeof(WebTemplate))]
    public interface IWebTemplate : IDataModel<IWebTemplate>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayCategory { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsRootWebOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSubWebOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Lcid { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        #endregion

    }
}
