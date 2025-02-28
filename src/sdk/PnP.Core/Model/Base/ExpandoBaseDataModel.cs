using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace PnP.Core.Model
{
    /// <summary>
    /// Internal type to represent an object that is a BaseDataModel<typeparamref name="TModel"/>
    /// and that also includes a set of dynamic properties, which can be accessed
    /// as named and typed properties, or which can be accessed through an indexer
    /// </summary>
    /// <remarks>
    /// Solution refactored starting from this document: 
    /// https://weblog.west-wind.com/posts/2012/feb/08/creating-a-dynamic-extensible-c-expando-object
    /// </remarks>
    /// <typeparam name="TModel">The actual type of the entity of the Domain Model</typeparam>
    internal class ExpandoBaseDataModel<TModel> : RecyclableBaseDataModel<TModel>, IExpandoDataModel
    {
        /// <summary>
        /// Type of the instance object
        /// </summary>
        private readonly Type instanceType;

        /// <summary>
        /// Cached array of properties for the current type
        /// </summary>
        private readonly PropertyInfo[] instanceProperties;

        /// <summary>
        /// Private field to keep track of the Values TransientDictionary creation
        /// </summary>
        private bool valuesInstantiated;

        /// <summary>
        /// Returns the overflow field name
        /// </summary>
        internal static string OverflowFieldName { get { return nameof(Values); } }

        /// <summary>
        /// String Dictionary that contains the custom properties with their dynamic values
        /// </summary>        
        public TransientDictionary Values
        {
            get
            {
                if (!valuesInstantiated)
                {
                    SetValue(new TransientDictionary());
                    valuesInstantiated = true;
                }
                return GetValue<TransientDictionary>();
            }
        }

        /// <summary>
        /// Gets the count of properties of the current expando complex type object
        /// </summary>
        public int Count => Values?.Count ?? 0;

        /// <summary>
        /// Creates an instance of the Expando object based on the
        /// actual inheriting type
        /// </summary>
        public ExpandoBaseDataModel()
        {
            // Get the reference type for the current instance
            instanceType = GetType();

            // Get all the public typed properties for the current type
            instanceProperties = instanceType.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly);
        }


        /// <summary>
        /// Try to retrieve a member by name
        /// </summary>
        /// <remarks>First try looking for a dynamic property, otherwise fall-back to typed properties</remarks>
        /// <param name="binder">The requested property</param>
        /// <param name="result">The value of the requested property, if any</param>
        /// <returns>Boolean indicating whether the member value was retrieved, or not</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // First check the Properties collection for member
            if (Values.ContainsKey(binder.Name))
            {
                result = Values[binder.Name];
                return true;
            }


            // Next check for Public properties via Reflection
            try
            {
                return GetProperty(binder.Name, out result);
            }
            catch
            {
                // We just skip any exception here and we fallback to the next statement
            }

            // Failed to retrieve a property
            result = null;
            return false;
        }


        /// <summary>
        /// Try to set a member by name
        /// </summary>
        /// <remarks>First try looking for typed properties, otherwise fall-back to a dynamic property</remarks>
        /// <param name="binder">The requested property</param>
        /// <param name="value">The value to set to the member</param>
        /// <returns>Boolean indicating whether the member set was successful, or not</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // First check to see if there's a native property to set
            try
            {
                bool result = SetProperty(binder.Name, value);
                if (result)
                {
                    return true;
                }
            }
            catch
            {
                // We just skip any exception here and we fallback to the next statement
            }

            // In case there is no matching property - set or add to dictionary
            Values[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Reflection helper method to retrieve a property
        /// </summary>
        /// <param name="name">The name of the property to retrieve a value for</param>
        /// <param name="result">The value of the property, if any</param>
        /// <returns>Defines whether the property was found or not</returns>
        protected bool GetProperty(string name, out object result)
        {
            // Setup initial result value
            result = null;

            // Try to get the member with provided name
            var member = instanceProperties
                .FirstOrDefault(m => m.Name == name);
            if (member != null)
            {
                // If any, get the value out of the instance object
                result = member.GetValue(this, null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reflection helper method to set a property value
        /// </summary>
        /// <param name="name">The name of the property to set a value to</param>
        /// <param name="value">The value to set on the property</param>
        /// <returns>Defines whether the set property value was successful or not</returns>
        protected bool SetProperty(string name, object value)
        {
            var member = instanceProperties
                .FirstOrDefault(m => m.Name == name);
            if (member != null)
            {
                // If any, set the value of the member in the instance object
                member.SetValue(this, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indexer to access the custom properties of the instance object
        /// </summary>
        /// <remarks>
        /// The getter checks the Properties dictionary first
        /// then looks in PropertyInfo for properties.
        /// The setter checks the instance properties before
        /// checking the Properties dictionary.
        /// </remarks>
        /// <param name="key">The key for the property</param>
        public object this[string key]
        {
            get
            {
                if (HasValue(key))
                {
                    return GetValue(key);
                }
                else if (Values.ContainsKey(key))
                {
                    return Values[key];
                }
                else
                {
                    throw new ClientException(ErrorType.PropertyNotLoaded,
                        string.Format(PnPCoreResources.Exception_PropertyDoesNotExist, key));
                }
            }
            set
            {
                // If the member to update is an already defined custom property
                if (Values.ContainsKey(key))
                {
                    // Just set its value
                    Values[key] = value;
                    return;
                }

                // Otherwise, check _instance for existance of typed first
                var member = instanceProperties
                    .FirstOrDefault(m => m.Name == key);
                if (member != null)
                {
                    // If we have a typed property, we use it
                    SetProperty(key, value);
                }
                else
                {
                    // If not, let's create a new custom property value
                    Values[key] = value;
                }
            }
        }


        /// <summary>
        /// Returns all the object properties, eventually including the instance typed properties
        /// </summary>
        /// <param name="includeInstanceProperties">Declares whether to include instance typed properties, by default false</param>
        /// <returns>The collection of properties for the instance</returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperties(bool includeInstanceProperties = false)
        {
            // Return the instance typed properties, if requested
            if (includeInstanceProperties)
            {
                foreach (var prop in instanceProperties)
                    yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(this, null));
            }

            // Always return the custom properties
            foreach (var key in Values.Keys)
                yield return new KeyValuePair<string, object>(key, Values[key]);
        }


        /// <summary>
        /// Checks whether a property exists in the instance type, either as a custom property or as a typed property, if requested
        /// </summary>
        /// <param name="item">The property to look for</param>
        /// <param name="includeInstanceProperties">Declares whether to include instance typed properties, by default false</param>
        /// <returns>A boolean defining whether the property exists or not</returns>
        public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
        {
            return Values.ContainsKey(item.Key) ||
                (includeInstanceProperties && instanceProperties.Any(p => p.Name == item.Key));
        }

        /// <summary>
        /// Checks whether a property exists in the instance type, either as a custom property or as a typed property, if requested
        /// </summary>
        /// <param name="key">The name of the property to look for</param>
        /// <param name="includeInstanceProperties">Declares whether to include instance typed properties, by default false</param>
        /// <returns>A boolean defining whether the property exists or not</returns>
        public bool Contains(String key, bool includeInstanceProperties = false)
        {
            return Values.ContainsKey(key) ||
                (includeInstanceProperties && instanceProperties.Any(p => p.Name == key));
        }

        /// <summary>
        /// Converts the current strongly typed object to a dynamic object
        /// </summary>
        /// <returns>The dynamic object from current strongly typed object</returns>
        public dynamic AsDynamic()
        {
            // TODO: Evaluate if we really need this method
            return TransientObject.AsDynamic(this);
        }
    }
}
