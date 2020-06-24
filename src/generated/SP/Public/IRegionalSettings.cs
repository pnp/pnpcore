using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RegionalSettings object
    /// </summary>
    [ConcreteType(typeof(RegionalSettings))]
    public interface IRegionalSettings : IDataModel<IRegionalSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AM { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CollationLCID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DateFormat { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DateSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DecimalSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DigitGrouping { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FirstDayOfWeek { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsEastAsia { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsRightToLeft { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsUIRightToLeft { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LocaleId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string NegativeSign { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int NegNumberMode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PM { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PositiveSign { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowWeeks { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThousandSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Time24 { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int TimeMarkerPosition { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TimeSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ILanguageCollection InstalledLanguages { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITimeZone TimeZone { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITimeZoneCollection TimeZones { get; }

        #endregion

    }
}
