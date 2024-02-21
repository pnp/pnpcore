using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a taxonomy field value
    /// </summary>
    public sealed class FieldTaxonomyValue : FieldValue, IFieldTaxonomyValue
    {
        internal FieldTaxonomyValue() : base()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="label">Taxonomy label</param>
        /// <param name="termId">Taxonomy term id</param>
        /// <param name="wssId">Optionally provide the wssId value</param>
        public FieldTaxonomyValue(Guid termId, string label, int wssId) : this()
        {
            if (termId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(termId));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            
            TermId = termId;
            Label = label;
            WssId = wssId;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="label">Taxonomy label</param>
        /// <param name="termId">Taxonomy term id</param>
        public FieldTaxonomyValue(Guid termId, string label) : this(termId, label, -1)
        {
        }

        internal override string SharePointRestType => "SP.Taxonomy.TaxonomyFieldValue";

        internal override Guid CsomType => Guid.Parse("19e70ed0-4177-456b-8156-015e4d163ff8");

        /// <summary>
        /// Taxonomy label
        /// </summary>
        public string Label { get => GetValue<string>(); set => SetValue(value); }

        /// <summary>
        /// Taxonomy term id
        /// </summary>
        public Guid TermId { get => GetValue<Guid>(); set => SetValue(value); }

        internal int WssId { get => GetValue<int>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Object)
            {
                if (json.TryGetProperty("TermGuid", out JsonElement termGuid))
                {
                    if (termGuid.ValueKind != JsonValueKind.Null)
                    {
                        TermId = termGuid.GetGuid();
                    }
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (json.TryGetProperty("Label", out JsonElement label))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    if (label.ValueKind != JsonValueKind.Null)
                    {
                        Label = label.GetString();
                    }
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (json.TryGetProperty("WssId", out JsonElement wssId))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    if (wssId.ValueKind != JsonValueKind.Null)
                    {
                        WssId = wssId.GetInt32();
                    }
                }
            }
            else if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null)
            {
                TermId = Guid.Empty;
                Label = null;
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (!properties.ContainsKey("TermID"))
            {
                TermId = Guid.Empty;
                Label = null;
            }
            else
            {
                if (properties.TryGetValue("Label", out string valueLabel))
                {
                    Label = valueLabel;
                }

                if (properties.TryGetValue("TermID", out string valueTermId))
                {
                    TermId = Guid.Parse(valueTermId);
                }

                WssId = -1;
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
                Label,
                TermGuid = TermId,
                WssId
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            if (!HasValue(nameof(TermId)) || !HasValue(nameof(Label)))
            {
                return "";
            }

            return $"{Label}|{TermId};";
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(TermId)) || !HasValue(nameof(Label)))
            {
                return "";
            }

            /*
            <Parameter TypeId="{19e70ed0-4177-456b-8156-015e4d163ff8}">
                <Property Name="Label" Type="String">MBI</Property>
                <Property Name="TermGuid" Type="String">1824510b-00e1-40ac-8294-528b1c9421e0</Property>
                <Property Name="WssId" Type="Int32">-1</Property>
            </Parameter>
             */
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "Label")
                .Replace(CsomHelper.FieldType, Label.GetType().Name)
                .Replace(CsomHelper.FieldValue, Label != null ? CsomHelper.XmlString(Label) : "Null"));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "TermGuid")
                .Replace(CsomHelper.FieldType, "String")
                .Replace(CsomHelper.FieldValue, TermId.ToString()));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "WssId")
                .Replace(CsomHelper.FieldType, WssId.GetType().Name)
                .Replace(CsomHelper.FieldValue, WssId.ToString()));
            return sb.ToString();
        }

    }
}
