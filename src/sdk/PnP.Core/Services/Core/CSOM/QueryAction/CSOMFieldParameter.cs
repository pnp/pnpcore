using PnP.Core.Model.SharePoint.Core.Internal;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class CSOMFieldParameter : Parameter
    {
        internal ICSOMField Field { get; set; }

        public CSOMFieldParameter(ICSOMField field)
        {
            Field = field;
            TypeId = field.CsomType.ToString();
        }

        internal override string SerializeValue()
        {
            return Field.ToCsomXml();
        }
    }
}
