using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RegionalSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RegionalSettings : BaseDataModel<IRegionalSettings>, IRegionalSettings
    {

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

        public ILanguageCollection InstalledLanguages
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new LanguageCollection
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ILanguageCollection>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public ITimeZone TimeZone
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new TimeZone
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITimeZone>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("TimeZones", Expandable = true)]
        public ITimeZoneCollection TimeZones
        {
            get
            {
                if (!HasValue(nameof(TimeZones)))
                {
                    var collection = new TimeZoneCollection(this.PnPContext, this, nameof(TimeZones));
                    SetValue(collection);
                }
                return GetValue<ITimeZoneCollection>();
            }
        }

        #endregion

    }
}
