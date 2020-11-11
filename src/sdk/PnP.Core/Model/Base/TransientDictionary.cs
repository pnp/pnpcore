using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// Class tracks Dictionary changes 
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
                return this
                    .Where(i => changes.Contains(i.Key))
                    .ToDictionary(i => i.Key, i => i.Value);
            }
        }

        /// <summary>
        /// Does this model instance have changes?
        /// </summary>
        public bool HasChanges => changes.Count > 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TransientDictionary() :
            base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Constructor that initializes based upon an existing Dictionary
        /// </summary>
        /// <param name="input"><see cref="Dictionary{TKey, TValue}"/> holding the key value pairs to add</param>
        public TransientDictionary(Dictionary<string, object> input) :
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
                    if (object.Equals(oldValue, value))
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

        internal void MarkAsChanged(string key)
        {
            if (!changes.Contains(key))
            {
                changes.Add(key);
            }
        }
    }
}
