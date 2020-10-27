using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RegionalSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.RegionalSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RegionalSettings : BaseDataModel<IRegionalSettings>, IRegionalSettings
    {
        #region Construction
        public RegionalSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AM { get => GetValue<string>(); set => SetValue(value); }

        public int CollationLCID { get => GetValue<int>(); set => SetValue(value); }

        public int DateFormat { get => GetValue<int>(); set => SetValue(value); }

        public string DateSeparator { get => GetValue<string>(); set => SetValue(value); }

        public string DecimalSeparator { get => GetValue<string>(); set => SetValue(value); }

        public string DigitGrouping { get => GetValue<string>(); set => SetValue(value); }

        public int FirstDayOfWeek { get => GetValue<int>(); set => SetValue(value); }

        public bool IsEastAsia { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRightToLeft { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsUIRightToLeft { get => GetValue<bool>(); set => SetValue(value); }

        public string ListSeparator { get => GetValue<string>(); set => SetValue(value); }

        public int LocaleId { get => GetValue<int>(); set => SetValue(value); }

        public string NegativeSign { get => GetValue<string>(); set => SetValue(value); }

        public int NegNumberMode { get => GetValue<int>(); set => SetValue(value); }

        public string PM { get => GetValue<string>(); set => SetValue(value); }

        public string PositiveSign { get => GetValue<string>(); set => SetValue(value); }

        public bool ShowWeeks { get => GetValue<bool>(); set => SetValue(value); }

        public string ThousandSeparator { get => GetValue<string>(); set => SetValue(value); }

        public bool Time24 { get => GetValue<bool>(); set => SetValue(value); }

        public int TimeMarkerPosition { get => GetValue<int>(); set => SetValue(value); }

        public string TimeSeparator { get => GetValue<string>(); set => SetValue(value); }

        public ILanguageCollection InstalledLanguages { get => GetModelValue<ILanguageCollection>(); }


        public ITimeZone TimeZone { get => GetModelValue<ITimeZone>(); }


        public ITimeZoneCollection TimeZones { get => GetModelCollectionValue<ITimeZoneCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
