using PnP.Core.Services;
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
            var xmlPayload = BuildXmlPayload();
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }
        #endregion

        #region Methods
        private string BuildXmlPayload()
        {
            string xml = CsomHelper.PropertyBagUpdate;

            if ((this as IDataModelParent).Parent is IFolder)
            {
                xml = xml.Replace(CsomHelper.ObjectId, CsomHelper.PropertyBagFolderObjectId);
                xml = xml.Replace(CsomHelper.PropertyName, CsomHelper.PropertyBagFileFolderPropertyName);
            }
            else if ((this as IDataModelParent).Parent is IFile)
            {
                xml = xml.Replace(CsomHelper.PropertyName, CsomHelper.PropertyBagFileFolderPropertyName);

                var fileObjectId = CsomHelper.PropertyBagFileObjectId;
                if (((this as IDataModelParent).Parent as IFile).IsPropertyAvailable(p => p.ServerRelativeUrl))
                {
                    fileObjectId = fileObjectId.Replace("{Parent.Id}", ((this as IDataModelParent).Parent as IFile).ServerRelativeUrl);
                    xml = xml.Replace(CsomHelper.ObjectId, fileObjectId);
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_FileServerRelativeUrlNotLoaded);
                }
            }
            else
            {
                xml = xml.Replace(CsomHelper.ObjectId, "");
                xml = xml.Replace(CsomHelper.PropertyName, CsomHelper.PropertyBagWebPropertyName);
            }

            int counter = 1;
            StringBuilder fieldValues = new StringBuilder();

            var entity = EntityManager.GetClassInfo(GetType(), this);
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            var changedProperties = GetChangedProperties();

            bool changeFound = false;
            foreach (PropertyDescriptor cp in changedProperties)
            {
                changeFound = true;
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
                            fieldValues.AppendLine(SetFieldValueXml(changedProp.Key, changedProp.Value, changedProp.Value?.GetType().Name, ref counter));
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        fieldValues.AppendLine(SetFieldValueXml(changedField.SharePointName, GetValue(changedField.Name), changedField.DataType.Name, ref counter));
                    }
                }
            }

            // No changes, so bail out
            if (!changeFound)
            {
                return null;
            }

            // update field values
            xml = xml.Replace("{FieldValues}", fieldValues.ToString());

            // update counter
            xml = xml.Replace("{Counter}", counter.ToString());

            return xml;
        }

        private static string SetFieldValueXml(string fieldName, object fieldValue, string fieldType, ref int counter)
        {
            string xml = CsomHelper.ListItemSystemUpdateSetFieldValue;

            xml = xml.Replace("{Counter}", counter.ToString());
            xml = xml.Replace("{FieldName}", fieldName);
            // TODO: verify complex fieldtypes
            xml = xml.Replace("{FieldValue}", fieldValue == null ? "" : CsomHelper.XmlString(TypeSpecificHandling(fieldValue.ToString(), fieldType), false));
            xml = xml.Replace("{FieldType}", fieldType ?? "Null");

            counter++;
            return xml;
        }

        private static string TypeSpecificHandling(string value, string fieldType)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fieldType) && fieldType.Equals("Boolean"))
            {
                return value.ToLowerInvariant();
            }

            return value;
        }
        #endregion

    }
}
