namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RegionalSettings object
    /// </summary>
    [ConcreteType(typeof(RegionalSettings))]
    public interface IRegionalSettings : IDataModel<IRegionalSettings>, IDataModelGet<IRegionalSettings>, IDataModelLoad<IRegionalSettings>, IQueryableDataModel
    {

        #region Properties

        /// <summary>
        /// Gets the string that is used to represent A.M..
        /// </summary>
        public string AM { get; }

        /// <summary>
        /// Gets the locale identifier (LCID) for the collation rules that are used on the site.
        /// </summary>
        public int CollationLCID { get; }

        /// <summary>
        /// Gets the date format that is used.
        /// </summary>
        public int DateFormat { get; }

        /// <summary>
        /// Gets the separator that is used for dates.
        /// </summary>
        public string DateSeparator { get; }

        /// <summary>
        /// Gets the separator that is used for decimals.
        /// </summary>
        public string DecimalSeparator { get; }

        /// <summary>
        /// Gets the separator that is used in grouping digits.
        /// </summary>
        public string DigitGrouping { get; }

        /// <summary>
        /// Gets the first day of the week used in calendars.
        /// </summary>
        public int FirstDayOfWeek { get; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the Web site locale is an East Asian locale.
        /// </summary>
        public bool IsEastAsia { get; }

        /// <summary>
        /// Gets the separator that is used for decimals.
        /// </summary>
        public bool IsRightToLeft { get; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the Web site user interface (UI) uses a right-to-left language.
        /// </summary>
        public bool IsUIRightToLeft { get; }

        /// <summary>
        /// Gets the separator used in lists.
        /// </summary>
        public string ListSeparator { get; }

        /// <summary>
        /// Gets or sets the locale ID in use.
        /// </summary>
        public int LocaleId { get; }

        /// <summary>
        /// Gets the symbol that is used to represent a negative sign.
        /// </summary>
        public string NegativeSign { get; }

        /// <summary>
        /// Gets the negative number mode that is used for displaying negative numbers in calculated fields based on the locale of the server.
        /// </summary>
        public int NegNumberMode { get; }

        /// <summary>
        /// Gets the string that is used to represent P.M..
        /// </summary>
        public string PM { get; }

        /// <summary>
        /// Gets the symbol that is used to represent a positive sign.
        /// </summary>
        public string PositiveSign { get; }

        /// <summary>
        /// Gets Boolean value that specifies whether to display the week number in day or week views of a calendar.
        /// </summary>
        public bool ShowWeeks { get; }

        /// <summary>
        /// Gets the thousand separator used for numbers.
        /// </summary>
        public string ThousandSeparator { get; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether to use a 24-hour time format in representing the hours of the day.
        /// </summary>
        public bool Time24 { get; }

        /// <summary>
        /// Gets a value that indicates whether A.M. and P.M. appear before or after the time string.
        /// </summary>
        public int TimeMarkerPosition { get; }

        /// <summary>
        /// Gets the time separator that is used.
        /// </summary>
        public string TimeSeparator { get; }

        /// <summary>
        /// Gets all properties of this entity
        /// </summary>
        public object All { get; }

        /// <summary>
        /// Gets the time zone that is used.
        /// </summary>
        public ITimeZone TimeZone { get; }

        /// <summary>
        /// Gets the collection of time zones used in SharePoint Online.
        /// </summary>
        public ITimeZoneCollection TimeZones { get; }

        #endregion

    }
}
