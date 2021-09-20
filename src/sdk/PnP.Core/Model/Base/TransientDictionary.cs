using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;

namespace PnP.Core.Model
{
    /// <summary>
    /// Class for tracking Dictionary changes 
    /// </summary>
    public class TransientDictionary : Dictionary<string, object>
    {
        private readonly HashSet<string> changes = new HashSet<string>();

        /// <summary>
        /// Returns a list of changed properties
        /// </summary>
        internal Dictionary<string, object> ChangedProperties
        {
            get
            {
                Dictionary<string, object> changedProperties = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> value in this)
                {
                    // FieldValue/FieldValueCollection is only used as part of the ListItem TransientDictionary field
                    if (value.Value is FieldValue fieldValue && fieldValue.HasChanges)
                    {
                        changedProperties.Add(value.Key, value.Value);
                    } 
                    else if (value.Value is FieldValueCollection fieldValueCollection && fieldValueCollection.HasChanges)
                    {
                        changedProperties.Add(value.Key, value.Value);
                    }
                    // Applies to both ListItem as other places (e.g. Properties)
                    else if (changes.Contains(value.Key))
                    {
                        changedProperties.Add(value.Key, value.Value);
                    }
                }
                return changedProperties;
            }
        }

        /// <summary>
        /// Does this model instance have changes?
        /// </summary>        
        public bool HasChanges
        {
            get
            {
                if (changes.Count > 0)
                {
                    return true;
                }
                else
                {
                    foreach (KeyValuePair<string, object> value in this)
                    {
                        // FieldValue/FieldValueCollection is only used as part of the ListItem TransientDictionary field
                        if (value.Value is FieldValue fieldValue && fieldValue.HasChanges)
                        {
                            return true;
                        }
                        else if (value.Value is FieldValueCollection fieldValueCollection && fieldValueCollection.HasChanges)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
            
        /// <summary>
        /// Default constructor
        /// </summary>
        internal TransientDictionary() :
            base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Constructor that initializes based upon an existing Dictionary
        /// </summary>
        /// <param name="input"><see cref="Dictionary{TKey, TValue}"/> holding the key value pairs to add</param>
        internal TransientDictionary(Dictionary<string, object> input) :
            base(new Dictionary<string, object>(input, StringComparer.OrdinalIgnoreCase))
        {
        }

        /// <summary>
        /// Gets or sets the value of an object in this dictionary
        /// </summary>
        /// <param name="key">Key of the object to set</param>
        /// <returns></returns>
        public new object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (TryGetValue(key, out object oldValue))
                {
                    // Always handle updates to FieldValueCollection and List<string> as changes on ListItems
                    if (object.Equals(oldValue, value) && !((value is FieldValueCollection) || (value is List<string>)))
                    {
                        // no change
                        return;
                    }
                    else
                    {
                        // update existing property
                        base[key] = value;
                        changes.Add(key);
                    }
                }
                else
                {
                    // new property
                    base[key] = value;
                    changes.Add(key);
                }
            }
        }

        internal virtual void Commit()
        {
            foreach(var property in base.Values)
            {
                // If there are FieldValue or FieldValueCollection properties (in case of an ListItem) then they need to be committed as well
                if (property is FieldValue propertyFieldValue)
                {
                    propertyFieldValue.Commit();
                }
                else if (property is FieldValueCollection propertyFieldValueCollection)
                {
                    propertyFieldValueCollection.Commit();
                }
            }

            changes.Clear();
        }

        internal void Merge(TransientDictionary input)
        {
            changes.Clear();
            Clear();
            foreach (var value in input)
            {
                SystemAdd(value.Key, value.Value);
            }
        }

        /// <summary>
        /// Adds a new item to the dictionary
        /// </summary>
        /// <param name="key">Key of the item to add</param>
        /// <param name="value">Value of the item to add</param>
        public new void Add(string key, object value)
        {
            base.Add(key, value);
            changes.Add(key);
        }

        /// <summary>
        /// System add, does not mark the added property as changed
        /// </summary>
        /// <param name="key">Key of the item to add</param>
        /// <param name="value">Value of the item to add</param>
        internal void SystemAdd(string key, object value)
        {
            base.Add(key, value);
        }

        /// <summary>
        /// System update, does not mark the updated property as changed
        /// </summary>
        /// <param name="key">Key of the item to update</param>
        /// <param name="value">Value of the item to update</param>
        internal void SystemUpdate(string key, object value)
        {
            base[key] = value;
        }

        /// <summary>
        /// System add, does not mark the added property as changed
        /// </summary>
        /// <param name="values">Collection of key/value pairs to add</param>
        internal void SystemAddRange(Dictionary<string, object> values)
        {
            foreach (var v in values)
            {
                SystemAdd(v.Key, v.Value);
            }
        }
    }
}
