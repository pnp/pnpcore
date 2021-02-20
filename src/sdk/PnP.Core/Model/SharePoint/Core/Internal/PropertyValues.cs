using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.Factories;
using PnP.Core.Services.Core.CSOM.Requests.Web;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// PropertyValues class
    /// </summary>
    [SharePointType("SP.PropertyValues")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class PropertyValues : ExpandoBaseDataModel<IPropertyValues>, IPropertyValues
    {
        #region Construction
        public PropertyValues()
        {
        }
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion

        #region Extension methods
        /// <summary>
        /// Get string typed property bag value. If does not contain, returns given default value.
        /// </summary>
        /// <param name="key">Key of the property bag entry to return</param>
        /// <param name="defaultValue">Default value of the property bag</param>
        /// <returns>Value of the property bag entry as string</returns>        
        public string GetString(string key, string defaultValue)
        {
            if (Values.ContainsKey(key))
            {
                return Values[key].ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        public int GetInteger(string key, int defaultValue)
        {
            if (Values.ContainsKey(key))
            {
                return int.Parse(Values[key].ToString());
            }
            else
            {
                return defaultValue;
            }
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            if (Values.ContainsKey(key))
            {
                return bool.Parse(Values[key].ToString());
            }
            else
            {
                return defaultValue;
            }
        }

        public new void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        public async new Task UpdateAsync()
        {
            UpdatePropertyBagRequest request = GetUpdatePropertyBag();
            if (request.FieldsToUpdate.Count > 0)
            {
                ApiCall updatePropertiesCall = PnPContext.GetCSOMCallForRequests(new List<IRequest<object>>()
                {
                    request
                });
                await RawRequestAsync(updatePropertiesCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }
        #endregion

        #region Methods
        internal virtual UpdatePropertyBagRequest GetUpdatePropertyBag()
        {
            UpdatePropertyBagRequest request = UpdatePropertyBagRequestFactory.GetUpdatePropertyBagRequest(this);

            var entity = EntityManager.GetClassInfo(GetType(), this);
            IEnumerable<EntityFieldInfo> fields = entity.Fields;
            foreach (PropertyDescriptor cp in ChangedProperties)
            {
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(this);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            // Let's set its value into the update message
                            request.FieldsToUpdate.Add(new CSOMItemField()
                            {
                                FieldName = changedProp.Key,
                                FieldValue = changedProp.Value,
                                FieldType = changedProp.Value?.GetType().Name
                            });
                        }
                    }
                    else
                    {
                        request.FieldsToUpdate.Add(new CSOMItemField()
                        {
                            FieldName = changedField.SharePointName,
                            FieldValue = GetValue(changedField.Name),
                            FieldType = changedField.DataType.Name
                        });
                    }
                }
            }
            return request;
        }
        #endregion

    }
}
