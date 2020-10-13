using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ViewCollection
    {
        public ViewCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        public async Task<IView> AddAsync(ViewOptions viewOptions)
        {
            if (viewOptions == null)
            {
                throw new ArgumentNullException(nameof(viewOptions));
            }

            var view = CreateNewAndAdd() as View;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { View.ViewOptionsAdditionalInformationKey, viewOptions}
            };


            return await view.AddAsync(additionalInfo).ConfigureAwait(false) as View;
        }
    }
}
