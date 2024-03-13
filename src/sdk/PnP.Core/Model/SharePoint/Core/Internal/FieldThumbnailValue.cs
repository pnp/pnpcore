using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a location field value
    /// </summary>
    public sealed class FieldThumbnailValue
        : FieldValue, IFieldThumbnailValue
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldThumbnailValue() : base()
        {
        }

        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Empty;

        public string FileName { get => GetValue<string>(); internal set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); internal set => SetValue(value); }

        public string ServerUrl { get => GetValue<string>(); internal set => SetValue(value); }

        public object ThumbnailRenderer { get => GetValue<object>(); internal set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined)
            {
                FileName = string.Empty;
            }
            else
            {
                return FromListDataAsStream(json.ToObject<Dictionary<string, string>>());
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("fileName"))
            {
                FileName = properties["fileName"];
            }
            if (properties.ContainsKey("serverRelativeUrl"))
            {
                ServerRelativeUrl = properties["serverRelativeUrl"];
            }
            if (properties.ContainsKey("serverUrl"))
            {
                ServerUrl = properties["serverUrl"];
            }
            if (properties.ContainsKey("thumbnailRenderer"))
            {
                ThumbnailRenderer = properties["thumbnailRenderer"];
            }
            // Clear changes
            Commit();

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {

            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            return null;
        }

        internal override string ToCsomXml()
        {
            return "";
        }

    }
}
