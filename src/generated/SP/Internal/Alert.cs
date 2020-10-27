using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Alert class, write your custom code here
    /// </summary>
    [SharePointType("SP.Alert", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Alert : BaseDataModel<IAlert>, IAlert
    {
        #region Construction
        public Alert()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int AlertFrequency { get => GetValue<int>(); set => SetValue(value); }

        public string AlertTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime AlertTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public int AlertType { get => GetValue<int>(); set => SetValue(value); }

        public bool AlwaysNotify { get => GetValue<bool>(); set => SetValue(value); }

        public int DeliveryChannels { get => GetValue<int>(); set => SetValue(value); }

        public int EventType { get => GetValue<int>(); set => SetValue(value); }

        public string Filter { get => GetValue<string>(); set => SetValue(value); }

        public Guid ID { get => GetValue<Guid>(); set => SetValue(value); }

        public int ItemID { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListID { get => GetValue<Guid>(); set => SetValue(value); }

        public string ListUrl { get => GetValue<string>(); set => SetValue(value); }

        public int Status { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int UserId { get => GetValue<int>(); set => SetValue(value); }

        public IPropertyValues AllProperties { get => GetModelValue<IPropertyValues>(); }


        public IListItem Item { get => GetModelValue<IListItem>(); }


        public IList List { get => GetModelValue<IList>(); }


        public IUser User { get => GetModelValue<IUser>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
