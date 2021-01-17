using System;
using System.Collections.ObjectModel;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of 'special' field values
    /// </summary>
    public interface IFieldValueCollection
    {
        /// <summary>
        /// The 'special' field values
        /// </summary>
        public ObservableCollection<IFieldValue> Values { get; }

        /// <summary>
        /// Removes <see cref="IFieldLookupValue"/> from the collection if found
        /// </summary>
        /// <param name="lookupId">Id of the <see cref="IFieldLookupValue"/> to remove</param>
        public void RemoveLookupValue(int lookupId);

        /// <summary>
        /// Removes <see cref="IFieldTaxonomyValue"/> from the collection if found
        /// </summary>
        /// <param name="termId">Id of the <see cref="IFieldTaxonomyValue"/> to remove</param>
        public void RemoveTaxonomyFieldValue(Guid termId);
    }
}
