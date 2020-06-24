using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a TimeZone object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class TimeZone : BaseDataModel<ITimeZone>, ITimeZone
    {

        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}
