using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Defines a reference to a Field
    /// </summary>
    public class FieldReference: BaseModel, IEquatable<FieldReference>
    {
        /// <summary>
        /// Specifies the ID of the item
        /// </summary>
        public Guid Id { get; set; }

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
        /// Compares object with FieldReference
        /// </summary>
        /// <param name="obj">Object that represents FieldReference</param>
        /// <returns>true if the current object is equal to the FieldReference</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is FieldReference))
            {
                return (false);
            }
            return (Equals((FieldReference)obj));
        }

        /// <summary>
        /// Compares FieldReference object based on Id, Name, and Remove properties.
        /// </summary>
        /// <param name="other">FieldReference object</param>
        /// <returns>true if the FieldReference object is equal to the current object; otherwise, false.</returns>
        public bool Equals(FieldReference other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.Id == other.Id &&
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
                this.Id.GetHashCode(),
                this.Name.GetHashCode(),
                this.Remove.GetHashCode()
            ).GetHashCode());
        }

        #endregion
    }
}
