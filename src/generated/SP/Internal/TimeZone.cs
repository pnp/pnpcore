using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TimeZone class, write your custom code here
    /// </summary>
    [SharePointType("SP.TimeZone", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TimeZone : BaseDataModel<ITimeZone>, ITimeZone
    {
        #region Construction
        public TimeZone()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
