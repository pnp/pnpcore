using PnP.Core.QueryModel.Model;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class FeatureCollection : QueryableDataModelCollection<IFeature>, IFeatureCollection
    {
        public FeatureCollection(PnPContext context, IDataModelParent parent, string memberName = null)
           : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        //Need to add Add - MUST ADD THESE
        public override IFeature CreateNew()
        {
            return NewFeature();
        }

        private Feature AddNewFeature()
        {
            var newFeature = NewFeature();
            this.items.Add(newFeature);
            return newFeature;
        }

        private Feature NewFeature()
        {
            var newFeature = new Feature
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newFeature;
        }

    }
}
