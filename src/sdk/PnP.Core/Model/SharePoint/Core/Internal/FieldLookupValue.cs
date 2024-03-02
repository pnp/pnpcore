using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a lookup field value
    /// </summary>
    public class FieldLookupValue : FieldValue, IFieldLookupValue
    {
        internal FieldLookupValue() : base()
        {
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="lookupId">Id of the looked-up item</param>
        public FieldLookupValue(int lookupId) : this()
        {
            if (lookupId < -1)
            {
                throw new ArgumentNullException(nameof(lookupId));
            }
            
            LookupId = lookupId;
        }
        
        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Parse("f1d34cc0-9b50-4a78-be78-d5facfcccfb7");

        /// <summary>
        /// Id of the looked-up item
        /// </summary>
        public int LookupId
        {
            get
            {
                if (!HasValue())
                {
                    return -1;
                }

                return GetValue<int>();
            }

            set => SetValue(value);
        }

        /// <summary>
        /// Value of the key property of the looked-up item
        /// </summary>
        public string LookupValue { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Is the value a secret value?
        /// </summary>
        public bool IsSecretFieldValue { get => GetValue<bool>(); internal set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number)
            {
                LookupId = json.GetInt32();
            }
            else if (json.ValueKind == JsonValueKind.Object)
            {
                /*
                "Lookup1": {
                        "LookupId": 686,
                        "LookupValue": "Test"
                    },
                */
                if (json.TryGetProperty("LookupId", out JsonElement lookupIdValue))
                {
                    LookupId = lookupIdValue.GetInt32();
                    IsSecretFieldValue = false;
                    
                    if (json.TryGetProperty("LookupValue", out JsonElement lookupValue))
                    {
                        LookupValue = lookupValue.GetString();
                    }
                }
            }
            else
            {
                LookupId = -1;
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (!properties.TryGetValue("lookupId", out string value))
            {
                LookupId = -1;                
                IsSecretFieldValue = false;
                
                // If additional lookup properties are loaded they're returned as string, but using type "Lookup"
                //
                // "LookupSingle_x003a_Created": "30\\u002f03\\u002f2020 21:14",
                // "LookupSingle_x003a_Created.": "2020-03-30T19:14:57Z",
                // 
                if (properties.Count == 1)
                {
                    LookupValue = properties.First().Value?.ToString();
                }
            }
            else
            {
                LookupId = int.Parse(value);

                if (properties.TryGetValue("lookupValue", out string valueLookupValue))
                {
                    LookupValue = valueLookupValue;
                }

                if (properties.TryGetValue("isSecretFieldValue", out string valueIsSecretValue))
                {
                    IsSecretFieldValue = bool.Parse(valueIsSecretValue);
                }
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                LookupId
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            return $"{LookupId}";
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(LookupId)))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "LookupId")
                .Replace(CsomHelper.FieldType, LookupId.GetType().Name)
                .Replace(CsomHelper.FieldValue, LookupId.ToString()));
            sb.Append(CsomHelper.ListItemSpecialFieldPropertyEmpty
                .Replace(CsomHelper.FieldName, "LookupValue")
                .Replace(CsomHelper.FieldType, "Null"));
            return sb.ToString();
        }
    }
}
