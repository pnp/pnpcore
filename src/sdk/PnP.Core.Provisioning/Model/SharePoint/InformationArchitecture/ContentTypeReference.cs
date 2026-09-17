using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Defines a reference to a Content Type
    /// </summary>
    public class ContentTypeReference : BaseModel, IEquatable<ContentTypeReference>
    {
        /// <summary>
        /// The ID of the Content Type
        /// </summary>
        internal string ContentTypeId { get; set; }

        /// <summary>
        /// Specifies the name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines whether the item should be removed or not
        /// </summary>
        public bool Remove { get; set; }

        #region Comparison code

        /// <summary>
        /// Compares object with ContentTypeReference
        /// </summary>
        /// <param name="obj">Object that represents ContentTypeReference</param>
        /// <returns>true if the current object is equal to the ContentTypeReference</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ContentTypeReference))
            {
                return (false);
            }
            return (Equals((ContentTypeReference)obj));
        }

        /// <summary>
        /// Compares ContentTypeReference object based on Id, Name, and Remove properties.
        /// </summary>
        /// <param name="other">ContentTypeReference object</param>
        /// <returns>true if the ContentTypeReference object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ContentTypeReference other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.ContentTypeId == other.ContentTypeId &&
                    this.Name == other.Name &&
                    this.Remove == other.Remove
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|",
                this.ContentTypeId.GetHashCode(),
                this.Name.GetHashCode(),
                this.Remove.GetHashCode()
            ).GetHashCode());
        }

        #endregion
    }
}
