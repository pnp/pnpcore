using PnP.Core.Model.Security;
using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a user field value
    /// </summary>
    public sealed class FieldUserValue : FieldLookupValue, IFieldUserValue
    {
        internal FieldUserValue() : base()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="principal">Principal describing the user or group</param>
        public FieldUserValue(ISharePointPrincipal principal) : this()
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            
            Principal = principal;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="userId">Id of the user</param>
        public FieldUserValue(int userId) : this()
        {
            if (userId < -1)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            
            LookupId = userId;
        }

        internal override string SharePointRestType { get => "SP.FieldUserValue"; }

        internal override Guid CsomType { get => Guid.Parse("c956ab54-16bd-4c18-89d2-996f57282a6f"); }

        /// <summary>
        /// Principal describing the user or group
        /// </summary>
        public ISharePointPrincipal Principal
        {
            get
            {
                return GetValue<ISharePointPrincipal>();
            }
            set
            {
                SetValue(value);

                if (value != null)
                {
                    SetValue(value.Id, nameof(LookupId));
                }
                else
                {
                    SetValue(-1, nameof(LookupId));
                }
            }
        }

        /// <summary>
        /// SIP address of the user
        /// </summary>
        public string Sip { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Title/name of the user
        /// </summary>
        public string Title { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Profile picture url for the user
        /// </summary>
        public string Picture { get => GetValue<string>(); internal set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.String)
            {
                LookupId = int.Parse(json.GetString());
            }
            else if (json.ValueKind == JsonValueKind.Number)
            {
                LookupId = json.GetInt32();
            }
            else if (json.ValueKind == JsonValueKind.Object)
            {
                /*
                    "Author": {
                        "LookupId": 11,
                        "LookupValue": "Bert Jansen (Cloud)",
                        "Email": "bert.jansen@bertonline.onmicrosoft.com"
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

                    if (json.TryGetProperty("Email", out JsonElement emailValue))
                    {
                        Email = emailValue.GetString();
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
            if (properties.TryGetValue("id", out string valueId))
            {
                if (string.IsNullOrEmpty(valueId))
                {
                    LookupId = -1;

                    // Clear changes
                    Commit();

                    return this;
                }

                LookupId = int.Parse(valueId);
            }

            if (properties.TryGetValue("email", out string valueEmail))
            {
                Email = valueEmail;
            }

            if (properties.TryGetValue("title", out string valueTitle))
            {
                Title = valueTitle; 
                // when using User fields the value is stored in the title property
                LookupValue = valueTitle;
            }

            // when using UserMulti fields the value is stored in the value property
            if (properties.TryGetValue("value", out string valueValue))
            {
                LookupValue = valueValue;
            }

            if (properties.TryGetValue("sip", out string valueSip))
            {
                Sip = valueSip;
            }

            if (properties.TryGetValue("picture", out string valuePicture))
            {
                Picture = valuePicture;
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                __metadata = new { type = SharePointRestType },
                LookupId,
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            if (HasValue("Principal"))
            {
                if (Principal == null)
                {
                    return JsonSerializer.Serialize(new List<object>());
                }

                var users = new List<object>
                {
                    new { Key = Principal.LoginName }
                };

                return JsonSerializer.Serialize(users.ToArray());
            }

            throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_MissingSharePointPrincipal);
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(LookupId)))
            {
                return "";
            }

            /*
            <Parameter TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                <Property Name="Email" Type="Null" />
                <Property Name="LookupId" Type="Int32">6</Property>
                <Property Name="LookupValue" Type="Null" />
            </Parameter>
             */
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldPropertyEmpty
                .Replace(CsomHelper.FieldName, "Email")
                .Replace(CsomHelper.FieldType, "Null"));
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
