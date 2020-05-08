using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Web Domain Model objects
    /// </summary>
    internal partial class WebCollection : QueryableDataModelCollection<IWeb>, IWebCollection
    {
        public WebCollection(PnPContext context, IDataModelParent parent)
            : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        public override IWeb CreateNew()
        {
            return NewWeb();
        }

        private Web AddNewWeb()
        {
            var newWeb = NewWeb();
            this.items.Add(newWeb);
            return newWeb;
        }

        private Web NewWeb()
        {
            var newWeb = new Web
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newWeb;
        }
    }
}
