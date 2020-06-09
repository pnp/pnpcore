using PnP.Core.QueryModel.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection : QueryableDataModelCollection<IField>, IFieldCollection
    {
        public FieldCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        public override IField CreateNew()
        {
            return NewField();
        }

        private Field AddNewField()
        {
            var newField = NewField();
            this.items.Add(newField);
            return newField;
        }

        private Field NewField()
        {
            var newField = new Field
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newField;
        }
    }
}
