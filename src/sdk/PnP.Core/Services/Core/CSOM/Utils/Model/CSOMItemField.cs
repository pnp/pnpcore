namespace PnP.Core.Services.Core.CSOM.Utils.Model
{
    internal class CSOMItemField<T>
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public T FieldValue { get; set; }

        public string SerializeFieldValue()
        {
            if(FieldValue == null)
            {
                return string.Empty;
            }
            return CsomHelper.XmlString(TypeSpecificHandling(FieldValue.ToString(), FieldType), false);
        }

        protected static string TypeSpecificHandling(string value, string fieldType)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fieldType) && fieldType.Equals("Boolean"))
            {
                return value.ToLowerInvariant();
            }

            return value;
        }
    }
    internal class CSOMItemField : CSOMItemField<object>
    {

    }
}
