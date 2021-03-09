using PnP.Core.Model.SharePoint;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class TaxonomyMultiParameter : Parameter
    {
        IFieldValueCollection FieldValueCollection { get; set; }

        internal TaxonomyMultiParameter(IFieldValueCollection fieldValueCollection)
        {
            FieldValueCollection = fieldValueCollection;
        }

        internal override string SerializeParameter()
        {
            string taxValue = string.Join(";#", FieldValueCollection.Values.Select(fldValue =>
            {
                FieldTaxonomyValue taxonomyValue = fldValue as FieldTaxonomyValue;
                return $"{taxonomyValue.WssId};#{taxonomyValue.Label}|{taxonomyValue.TermId}";
            }));

            return $"<Parameter Type=\"String\">{taxValue}</Parameter>";
        }
    }
}
