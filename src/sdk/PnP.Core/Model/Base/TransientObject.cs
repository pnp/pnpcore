using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PnP.Core.Model
{

    /// <summary>
    /// Delegate used to connect model validation for one or more fields
    /// </summary>
    /// <param name="propertyUpdateRequest">Information about the property being updated</param>
    internal delegate void ValidateUpdate(ref PropertyUpdateRequest propertyUpdateRequest);

    /// <summary>
    /// Public abstract class to handle the internal state of properties for domain model object
    /// </summary>
    internal abstract class TransientObject : System.Dynamic.DynamicObject
    {
        private readonly Dictionary<string, object> current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object> initial = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> changes = new HashSet<string>();

        /// <summary>
        /// Validate update handler
        /// </summary>
        [SystemProperty]
        internal ValidateUpdate ValidateUpdateHandler { get; set; } = null;

        /// <summary>
        /// Returns a list of changed properties
        /// </summary>
        [Browsable(false)]
        [SystemProperty]
        public IEnumerable<string> ChangedProperties => changes;

        public PropertyDescriptorCollection GetChangedProperties()
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);
            PropertyDescriptorCollection result = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
            foreach (PropertyDescriptor property in properties)
            {
                if (HasChanged(property.Name))
                {
                    result.Add(property);
                }
            }

            return result;
        }

        /// <summary>
        /// Are the changes done on this model instance
        /// </summary>
        [SystemProperty]
        public bool HasChanges => changes.Count > 0;

        /// <summary>
        /// Indication of logically deleted object, will be automatically removed from the collection
        /// </summary>
        [Browsable(false)]
        [SystemProperty]
        internal bool Deleted { get; set; }

        /// <summary>
        /// Id if the batch request that loaded this mode instance
        /// </summary>
        [SystemProperty]
        internal Guid BatchRequestId { get; set; }

        public virtual void Commit()
        {
            Swap(initial, current);
        }

        private void Swap(IDictionary<string, object> a, IDictionary<string, object> b)
        {
            a.Clear();
            foreach (var pair in b)
            {
                a[pair.Key] = pair.Value;

                // Also commit the changes in the TransientDictionary if needed
                if (a[pair.Key] is TransientDictionary)
                {
                    (a[pair.Key] as TransientDictionary).Commit();
                }
                // Also commit the changes in the ComplexTypeModel classes
                else if (a[pair.Key] is IComplexType)
                {
                    (a[pair.Key] as TransientObject).Commit();
                }
            }

            changes.Clear();
        }

        internal virtual object GetValue(string propertyName = "")
        {
            CheckDeleted();

            if (current.TryGetValue(propertyName, out object value))
            {
                return value;
            }

            throw new ClientException(ErrorType.PropertyNotLoaded, $"Property {propertyName} was not yet loaded");
        }

        protected virtual T GetValue<T>([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            CheckDeleted();

            if (current.TryGetValue(propertyName, out object value))
            {
                return (T)value;
            }

            throw new ClientException(ErrorType.PropertyNotLoaded, $"Property {propertyName} was not yet loaded");
        }

        protected virtual bool SetValue<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            CheckDeleted();

            if (current.TryGetValue(propertyName, out object oldValue))
            {
                if (object.Equals(oldValue, value))
                {
                    return false;
                }
            }

            // Here we can eventually add some validation logic ...

            if (current.ContainsKey(propertyName))
            {
                // Call ValidateHandler in case of an update
                bool updateField = true;
                if (ValidateUpdateHandler != null)
                {
                    var fieldUpdateRequest = new PropertyUpdateRequest(propertyName, value);

                    ValidateUpdateHandler.Invoke(ref fieldUpdateRequest);

                    if (!fieldUpdateRequest.Cancelled)
                    {
                        value = (T)fieldUpdateRequest.Value;
                    }
                    else                    
                    {
                        updateField = false;
                        if (!string.IsNullOrEmpty(fieldUpdateRequest.CancellationReason))
                        {
                            //TODO: log message
                        }
                    }
                }

                if (updateField)
                {
                    // We're changing this property
                    if (!initial.ContainsKey(propertyName))
                    {
                        initial[propertyName] = value;
                    }
                    current[propertyName] = value;
                    changes.Add(propertyName);
                }
            }
            else
            {
                // initial load
                current[propertyName] = value;
            }

            return true;
        }

        public bool HasValue(string propertyName = "")
        {
            if (current.TryGetValue(propertyName, out _))
            {
                return true;
            }

            return false;
        }

        public bool HasChanged(string propertyName = "")
        {
            if (changes.Contains(propertyName))
            {
                return true;
            }

            if (current.TryGetValue(propertyName, out object value))
            {
                // If the property is a TransientDictionary then check for changes in the dictionary
                if (value is TransientDictionary transientDictionary && 
                    transientDictionary.HasChanges)
                {
                    return true;
                }

                // If the property is a TransientObject then check for changes in the complex object
                if (value is TransientObject transientObject && 
                    transientObject.HasChanges)
                {
                    return true;
                }
            }

            return false;
        }

       
        /// <summary>
        /// Merges the results from one object with the other. Will be typically used when:
        /// - The same object was requested twice in a batch
        /// - Refresh of an added object
        /// </summary>
        /// <param name="input"><see cref="TransientObject"/> to merge with this <see cref="TransientObject"/></param>
        internal void Merge(TransientObject input)
        {
            // Copy the basic properties
            foreach(var prop in input.current)
            {
                if (prop.Value is TransientDictionary)
                {
                    (current[prop.Key] as TransientDictionary).Merge(prop.Value as TransientDictionary);
                }
                else
                {
                    SetValue(prop.Value, prop.Key);

                    // Handle the model properties
                    if (prop.Value is IDataModelParent)
                    {
                        // Ensure the merged model property parent is this model
                        (current[prop.Key] as IDataModelParent).Parent = (IDataModelParent)this;
                    }
                }
            }

            // Update the BatchRequestId, too in order to
            // refresh the object in the collection with
            // the latest requested one
            this.BatchRequestId = input.BatchRequestId;
        }

        private void CheckDeleted()
        {
            if (Deleted)
            {
                throw new ClientException(ErrorType.InstanceWasDeleted, $"This model instance was deleted, you can't use it anymore");
            }
        }

        public static dynamic ToDynamic(dynamic source)
        {
            return source;
        }
    }
}
