using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FileVersion object
    /// </summary>
    [ConcreteType(typeof(FileVersion))]
    public interface IFileVersion : IComplexType
    {
        /// <summary>
        /// Gets a value that specifies the check-in comment.
        /// </summary>
        public string CheckInComment { get; }

        /// <summary>
        /// Gets a value that specifies the creation date and time for the file version.
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// Gets the internal identifier for the file version.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets a value that specifies whether the file version is the current version.
        /// </summary>
        public bool IsCurrentVersion { get; }

        /// <summary>
        /// Gets the size of this version of the file.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets a value that specifies the relative URL of the file version based on the URL for the site or subsite.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets a value that specifies the implementation specific identifier of the file.
        /// </summary>
        public string VersionLabel { get; }

        //// TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser CreatedBy { get; }
    }
}
