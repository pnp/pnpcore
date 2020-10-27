using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// WebInformation class, write your custom code here
    /// </summary>
    [SharePointType("SP.WebInformation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class WebInformation : BaseDataModel<IWebInformation>, IWebInformation
    {
        #region Construction
        public WebInformation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public int Language { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string WebTemplate { get => GetValue<string>(); set => SetValue(value); }

        public int WebTemplateId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
