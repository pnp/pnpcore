using System;

namespace PnP.Core.Provisioning.Model.Drive
{
    /// <summary>
    /// Defines a DriveFile object
    /// </summary>
    public partial class DriveFile : BaseModel, IEquatable<DriveFile>
    {
        #region Public members

        /// <summary>
        /// The Name of the target DriveFile
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Source of the target DriveFile
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Defines whether to Overwrite the target DriveFile
        /// </summary>
        public bool Overwrite { get; set; }

        #endregion

        #region Constructors

        public DriveFile() : base()
        {
        }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|",
                Name?.GetHashCode() ?? 0,
                Src?.GetHashCode() ?? 0,
                Overwrite.GetHashCode()
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with DriveFile class
        /// </summary>
        /// <param name="obj">Object that represents DriveFile</param>
        /// <returns>Checks whether object is DriveFile class</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DriveFile))
            {
                return (false);
            }
            return (Equals((DriveFile)obj));
        }

        /// <summary>
        /// Compares DriveFile object based on Name, Src, and Overwrite
        /// </summary>
        /// <param name="other">User DriveFile object</param>
        /// <returns>true if the DriveFile object is equal to the current object; otherwise, false.</returns>
        public bool Equals(DriveFile other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.Name == other.Name &&
                this.Src == other.Src &&
                this.Overwrite == other.Overwrite
                );
        }

        #endregion
    }
}
