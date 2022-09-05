using System.Runtime.Serialization;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Event time zones
    /// </summary>
    public enum EventTimeZone
    {
        /// <summary>
        /// Etc/GMT+12
        /// </summary>
        [EnumMember(Value = "Etc/GMT+12")]
        ETCGMTPlus12,

        /// <summary>
        /// Etc/GMT+11
        /// </summary>
        [EnumMember(Value = "Etc/GMT+11")]
        ETCGMTPlus11,

        /// <summary>
        /// Pacific/Honolulu
        /// </summary>
        [EnumMember(Value = "Pacific/Honolulu,")]
        PacificHonolulu,

        /// <summary>
        /// America/Anchorage
        /// </summary>
        [EnumMember(Value = "America/Anchorage")]
        AmericaAnchorage,

        /// <summary>
        /// America/Santa_Isabel
        /// </summary>
        [EnumMember(Value = "America/Santa_Isabel")]
        AmericaSanta_Isabel,

        /// <summary>
        /// America/Los_Angeles
        /// </summary>
        [EnumMember(Value = "America/Los_Angeles")]
        AmericaLos_Angeles,

        /// <summary>
        /// America/Phoenix,
        /// </summary>
        [EnumMember(Value = "America/Phoenix,")]
        AmericaPhoenix,

        /// <summary>
        ///  America/Chihuahua
        /// </summary>
        [EnumMember(Value = " America/Chihuahua")]
        AmericaChihuahua,

        /// <summary>
        ///  America/Denver
        /// </summary>
        [EnumMember(Value = " America/Denver")]
        AmericaDenver,

        /// <summary>
        ///  America/Guatemala
        /// </summary>
        [EnumMember(Value = " America/Guatemala")]
        AmericaGuatemala,

        /// <summary>
        ///  America/Chicago
        /// </summary>
        [EnumMember(Value = " America/Chicago")]
        AmericaChicago,

        /// <summary>
        ///  America/Mexico_City
        /// </summary>
        [EnumMember(Value = " America/Mexico_City")]
        AmericaMexico_City,

        /// <summary>
        ///  America/Regina
        /// </summary>
        [EnumMember(Value = " America/Regina")]
        AmericaRegina,

        /// <summary>
        ///  America/Bogota
        /// </summary>
        [EnumMember(Value = " America/Bogota")]
        AmericaBogota,

        /// <summary>
        ///  America/New_York
        /// </summary>
        [EnumMember(Value = " America/New_York")]
        AmericaNew_York,

        /// <summary>
        ///  America/Indiana/Indianapolis
        /// </summary>
        [EnumMember(Value = " America/Indiana/Indianapolis")]
        AmericaIndianaIndianapolis,

        /// <summary>
        ///  America/Caracas
        /// </summary>
        [EnumMember(Value = " America/Caracas")]
        AmericaCaracas,

        /// <summary>
        ///  America/Asuncion
        /// </summary>
        [EnumMember(Value = " America/Asuncion")]
        AmericaAsuncion,

        /// <summary>
        ///  America/Halifax
        /// </summary>
        [EnumMember(Value = " America/Halifax")]
        AmericaHalifax,

        /// <summary>
        ///  America/Cuiaba
        /// </summary>
        [EnumMember(Value = " America/Cuiaba")]
        AmericaCuiaba,

        /// <summary>
        ///  America/La_Paz
        /// </summary>
        [EnumMember(Value = " America/La_Paz")]
        AmericaLa_Paz,

        /// <summary>
        ///  America/Santiago
        /// </summary>
        [EnumMember(Value = " America/Santiago")]
        AmericaSantiago,

        /// <summary>
        ///  America/St_Johns
        /// </summary>
        [EnumMember(Value = " America/St_Johns")]
        AmericaSt_Johns,

        /// <summary>
        ///  America/Sao_Paulo
        /// </summary>
        [EnumMember(Value = " America/Sao_Paulo")]
        AmericaSao_Paulo,

        /// <summary>
        ///  America/Argentina/Buenos_Aires
        /// </summary>
        [EnumMember(Value = " America/Argentina/Buenos_Aires")]
        AmericaArgentinaBuenos_Aires,

        /// <summary>
        ///  America/Cayenne
        /// </summary>
        [EnumMember(Value = " America/Cayenne")]
        AmericaCayenne,

        /// <summary>
        ///  America/Godthab
        /// </summary>
        [EnumMember(Value = " America/Godthab")]
        AmericaGodthab,

        /// <summary>
        ///  America/Montevideo
        /// </summary>
        [EnumMember(Value = " America/Montevideo")]
        AmericaMontevideo,

        /// <summary>
        ///  America/Bahia
        /// </summary>
        [EnumMember(Value = " America/Bahia")]
        AmericaBahia,

        /// <summary>
        ///  Etc/GMT+2
        /// </summary>
        [EnumMember(Value = " Etc/GMT+2")]
        EtcGMT2,

        /// <summary>
        ///  Atlantic/Azores
        /// </summary>
        [EnumMember(Value = " Atlantic/Azores")]
        AtlanticAzores,

        /// <summary>
        ///  Atlantic/Cape_Verde
        /// </summary>
        [EnumMember(Value = " Atlantic/Cape_Verde")]
        AtlanticCape_Verde,

        /// <summary>
        ///  Africa/Casablanca
        /// </summary>
        [EnumMember(Value = " Africa/Casablanca")]
        AfricaCasablanca,

        /// <summary>
        ///  Etc/GMT
        /// </summary>
        [EnumMember(Value = " Etc/GMT")]
        EtcGMT,

        /// <summary>
        ///  Europe/London
        /// </summary>
        [EnumMember(Value = " Europe/London")]
        EuropeLondon,

        /// <summary>
        ///  Atlantic/Reykjavik
        /// </summary>
        [EnumMember(Value = " Atlantic/Reykjavik")]
        AtlanticReykjavik,

        /// <summary>
        ///  Europe/Berlin
        /// </summary>
        [EnumMember(Value = " Europe/Berlin")]
        EuropeBerlin,

        /// <summary>
        ///  Europe/Budapest
        /// </summary>
        [EnumMember(Value = " Europe/Budapest")]
        EuropeBudapest,

        /// <summary>
        ///  Europe/Paris
        /// </summary>
        [EnumMember(Value = " Europe/Paris")]
        EuropeParis,

        /// <summary>
        ///  Europe/Warsaw
        /// </summary>
        [EnumMember(Value = " Europe/Warsaw")]
        EuropeWarsaw,

        /// <summary>
        ///  Africa/Lagos
        /// </summary>
        [EnumMember(Value = " Africa/Lagos")]
        AfricaLagos,

        /// <summary>
        ///  Africa/Windhoek
        /// </summary>
        [EnumMember(Value = " Africa/Windhoek")]
        AfricaWindhoek,

        /// <summary>
        ///  Europe/Bucharest
        /// </summary>
        [EnumMember(Value = " Europe/Bucharest")]
        EuropeBucharest,

        /// <summary>
        ///  Asia/Beirut
        /// </summary>
        [EnumMember(Value = " Asia/Beirut")]
        AsiaBeirut,

        /// <summary>
        ///  Africa/Cairo
        /// </summary>
        [EnumMember(Value = " Africa/Cairo")]
        AfricaCairo,

        /// <summary>
        ///  Asia/Damascus
        /// </summary>
        [EnumMember(Value = " Asia/Damascus")]
        AsiaDamascus,

        /// <summary>
        ///  Africa/Johannesburg
        /// </summary>
        [EnumMember(Value = " Africa/Johannesburg")]
        AfricaJohannesburg,

        /// <summary>
        ///  Europe/Kyiv (Kiev)
        /// </summary>
        [EnumMember(Value = " Europe/Kyiv (Kiev)")]
        EuropeKyivKiev,

        /// <summary>
        ///  Europe/Istanbul
        /// </summary>
        [EnumMember(Value = " Europe/Istanbul")]
        EuropeIstanbul,

        /// <summary>
        ///  Asia/Jerusalem
        /// </summary>
        [EnumMember(Value = " Asia/Jerusalem")]
        AsiaJerusalem,

        /// <summary>
        ///  Asia/Amman
        /// </summary>
        [EnumMember(Value = " Asia/Amman")]
        AsiaAmman,

        /// <summary>
        ///  Asia/Baghdad
        /// </summary>
        [EnumMember(Value = " Asia/Baghdad")]
        AsiaBaghdad,

        /// <summary>
        ///  Europe/Kaliningrad
        /// </summary>
        [EnumMember(Value = " Europe/Kaliningrad")]
        EuropeKaliningrad,

        /// <summary>
        ///  Asia/Riyadh
        /// </summary>
        [EnumMember(Value = " Asia/Riyadh")]
        AsiaRiyadh,

        /// <summary>
        ///  Africa/Nairobi
        /// </summary>
        [EnumMember(Value = " Africa/Nairobi")]
        AfricaNairobi,

        /// <summary>
        ///  Asia/Tehran
        /// </summary>
        [EnumMember(Value = " Asia/Tehran")]
        AsiaTehran,

        /// <summary>
        ///  Asia/Dubai
        /// </summary>
        [EnumMember(Value = " Asia/Dubai")]
        AsiaDubai,

        /// <summary>
        ///  Asia/Baku
        /// </summary>
        [EnumMember(Value = " Asia/Baku")]
        AsiaBaku,

        /// <summary>
        ///  Europe/Moscow
        /// </summary>
        [EnumMember(Value = " Europe/Moscow")]
        EuropeMoscow,

        /// <summary>
        ///  Indian/Mauritius
        /// </summary>
        [EnumMember(Value = " Indian/Mauritius")]
        IndianMauritius,

        /// <summary>
        ///  Asia/Tbilisi
        /// </summary>
        [EnumMember(Value = " Asia/Tbilisi")]
        AsiaTbilisi,

        /// <summary>
        ///  Asia/Yerevan
        /// </summary>
        [EnumMember(Value = " Asia/Yerevan")]
        AsiaYerevan,

        /// <summary>
        ///  Asia/Kabul
        /// </summary>
        [EnumMember(Value = " Asia/Kabul")]
        AsiaKabul,

        /// <summary>
        ///  Asia/Karachi
        /// </summary>
        [EnumMember(Value = " Asia/Karachi")]
        AsiaKarachi,

        /// <summary>
        ///  Asia/Toshkent (Tashkent)
        /// </summary>
        [EnumMember(Value = " Asia/Toshkent (Tashkent)")]
        AsiaToshkentTashkent,

        /// <summary>
        ///  Asia/Kolkata
        /// </summary>
        [EnumMember(Value = " Asia/Kolkata")]
        AsiaKolkata,

        /// <summary>
        ///  Asia/Colombo
        /// </summary>
        [EnumMember(Value = " Asia/Colombo")]
        AsiaColombo,

        /// <summary>
        ///  Asia/Kathmandu
        /// </summary>
        [EnumMember(Value = " Asia/Kathmandu")]
        AsiaKathmandu,

        /// <summary>
        ///  Asia/Astana (Almaty)
        /// </summary>
        [EnumMember(Value = " Asia/Astana (Almaty)")]
        AsiaAstanaAlmaty,

        /// <summary>
        ///  Asia/Dhaka
        /// </summary>
        [EnumMember(Value = " Asia/Dhaka")]
        AsiaDhaka,

        /// <summary>
        ///  Asia/Yekaterinburg
        /// </summary>
        [EnumMember(Value = " Asia/Yekaterinburg")]
        AsiaYekaterinburg,

        /// <summary>
        ///  Asia/Yangon (Rangoon)
        /// </summary>
        [EnumMember(Value = " Asia/Yangon (Rangoon)")]
        AsiaYangonRangoon,

        /// <summary>
        ///  Asia/Bangkok
        /// </summary>
        [EnumMember(Value = " Asia/Bangkok")]
        AsiaBangkok,

        /// <summary>
        ///  Asia/Novosibirsk
        /// </summary>
        [EnumMember(Value = " Asia/Novosibirsk")]
        AsiaNovosibirsk,

        /// <summary>
        ///  Asia/Shanghai
        /// </summary>
        [EnumMember(Value = " Asia/Shanghai")]
        AsiaShanghai,

        /// <summary>
        ///  Asia/Krasnoyarsk
        /// </summary>
        [EnumMember(Value = " Asia/Krasnoyarsk")]
        AsiaKrasnoyarsk,

        /// <summary>
        ///  Asia/Singapore
        /// </summary>
        [EnumMember(Value = " Asia/Singapore")]
        AsiaSingapore,

        /// <summary>
        ///  Australia/Perth
        /// </summary>
        [EnumMember(Value = " Australia/Perth")]
        AustraliaPerth,

        /// <summary>
        ///  Asia/Taipei
        /// </summary>
        [EnumMember(Value = " Asia/Taipei")]
        AsiaTaipei,

        /// <summary>
        ///  Asia/Ulaanbaatar
        /// </summary>
        [EnumMember(Value = " Asia/Ulaanbaatar")]
        AsiaUlaanbaatar,

        /// <summary>
        ///  Asia/Irkutsk
        /// </summary>
        [EnumMember(Value = " Asia/Irkutsk")]
        AsiaIrkutsk,

        /// <summary>
        ///  Asia/Tokyo
        /// </summary>
        [EnumMember(Value = " Asia/Tokyo")]
        AsiaTokyo,

        /// <summary>
        ///  Asia/Seoul
        /// </summary>
        [EnumMember(Value = " Asia/Seoul")]
        AsiaSeoul,

        /// <summary>
        ///  Australia/Adelaide
        /// </summary>
        [EnumMember(Value = " Australia/Adelaide")]
        AustraliaAdelaide,

        /// <summary>
        ///  Australia/Darwin
        /// </summary>
        [EnumMember(Value = " Australia/Darwin")]
        AustraliaDarwin,

        /// <summary>
        ///  Australia/Brisbane
        /// </summary>
        [EnumMember(Value = " Australia/Brisbane")]
        AustraliaBrisbane,

        /// <summary>
        ///  Australia/Sydney
        /// </summary>
        [EnumMember(Value = " Australia/Sydney")]
        AustraliaSydney,

        /// <summary>
        ///  Pacific/Port_Moresby
        /// </summary>
        [EnumMember(Value = " Pacific/Port_Moresby")]
        PacificPort_Moresby,

        /// <summary>
        ///  Australia/Hobart
        /// </summary>
        [EnumMember(Value = " Australia/Hobart")]
        AustraliaHobart,

        /// <summary>
        ///  Asia/Yakutsk
        /// </summary>
        [EnumMember(Value = " Asia/Yakutsk")]
        AsiaYakutsk,

        /// <summary>
        ///  Pacific/Guadalcanal
        /// </summary>
        [EnumMember(Value = " Pacific/Guadalcanal")]
        PacificGuadalcanal,

        /// <summary>
        ///  Asia/Vladivostok
        /// </summary>
        [EnumMember(Value = " Asia/Vladivostok")]
        AsiaVladivostok,

        /// <summary>
        ///  Pacific/Auckland
        /// </summary>
        [EnumMember(Value = " Pacific/Auckland")]
        PacificAuckland,

        /// <summary>
        ///  Etc/GMT-12
        /// </summary>
        [EnumMember(Value = " Etc/GMT-12")]
        EtcGMT12,

        /// <summary>
        ///  Pacific/Fiji
        /// </summary>
        [EnumMember(Value = " Pacific/Fiji")]
        PacificFiji,

        /// <summary>
        ///  Asia/Magadan
        /// </summary>
        [EnumMember(Value = " Asia/Magadan")]
        AsiaMagadan,

        /// <summary>
        ///  Pacific/Tongatapu
        /// </summary>
        [EnumMember(Value = " Pacific/Tongatapu")]
        PacificTongatapu,

        /// <summary>
        ///  Pacific/Apia
        /// </summary>
        [EnumMember(Value = " Pacific/Apia")]
        PacificApia,

        /// <summary>
        ///  Pacific/Kiritimati
        /// </summary>
        [EnumMember(Value = " Pacific/Kiritimati")]
        PacificKiritimati,

        /// <summary>
        /// Dateline Standard Time
        /// </summary>
        [EnumMember(Value = "Dateline Standard Time")]
        DatelineStandardTime,

        /// <summary>
        /// UTC-11
        /// </summary>
        [EnumMember(Value = "UTC-11")]
        UTC11,

        /// <summary>
        /// Aleutian Standard Time
        /// </summary>
        [EnumMember(Value = "Aleutian Standard Time")]
        AleutianStandardTime,

        /// <summary>
        /// Hawaiian Standard Time
        /// </summary>
        [EnumMember(Value = "Hawaiian Standard Time")]
        HawaiianStandardTime,

        /// <summary>
        /// Marquesas Standard Time
        /// </summary>
        [EnumMember(Value = "Marquesas Standard Time")]
        MarquesasStandardTime,

        /// <summary>
        /// Alaskan Standard Time
        /// </summary>
        [EnumMember(Value = "Alaskan Standard Time")]
        AlaskanStandardTime,

        /// <summary>
        /// UTC-09
        /// </summary>
        [EnumMember(Value = "UTC-09")]
        UTC09,

        /// <summary>
        /// Pacific Standard Time (Mexico)
        /// </summary>
        [EnumMember(Value = "Pacific Standard Time (Mexico)")]
        PacificStandardTimeMexico,

        /// <summary>
        /// UTC-08
        /// </summary>
        [EnumMember(Value = "UTC-08")]
        UTC08,

        /// <summary>
        /// Pacific Standard Time
        /// </summary>
        [EnumMember(Value = "Pacific Standard Time")]
        PacificStandardTime,

        /// <summary>
        /// US Mountain Standard Time
        /// </summary>
        [EnumMember(Value = "US Mountain Standard Time")]
        USMountainStandardTime,

        /// <summary>
        ///  La Paz
        /// </summary>
        [EnumMember(Value = " La Paz")]
        LaPaz,

        /// <summary>
        /// Mountain Standard Time (Mexico)
        /// </summary>
        [EnumMember(Value = "Mountain Standard Time (Mexico)")]
        MountainStandardTimeMexico,

        /// <summary>
        /// Mountain Standard Time
        /// </summary>
        [EnumMember(Value = "Mountain Standard Time")]
        MountainStandardTime,

        /// <summary>
        /// Yukon Standard Time
        /// </summary>
        [EnumMember(Value = "Yukon Standard Time")]
        YukonStandardTime,

        /// <summary>
        /// Central America Standard Time
        /// </summary>
        [EnumMember(Value = "Central America Standard Time")]
        CentralAmericaStandardTime,

        /// <summary>
        /// Central Standard Time
        /// </summary>
        [EnumMember(Value = "Central Standard Time")]
        CentralStandardTime,

        /// <summary>
        /// Easter Island Standard Time
        /// </summary>
        [EnumMember(Value = "Easter Island Standard Time")]
        EasterIslandStandardTime,

        /// <summary>
        ///  Mexico City
        /// </summary>
        [EnumMember(Value = " Mexico City")]
        MexicoCity,

        /// <summary>
        /// Central Standard Time (Mexico)
        /// </summary>
        [EnumMember(Value = "Central Standard Time (Mexico)")]
        CentralStandardTimeMexico,

        /// <summary>
        /// Canada Central Standard Time
        /// </summary>
        [EnumMember(Value = "Canada Central Standard Time")]
        CanadaCentralStandardTime,

        /// <summary>
        ///  Lima
        /// </summary>
        [EnumMember(Value = " Lima")]
        Lima,

        /// <summary>
        ///  Rio Branco
        /// </summary>
        [EnumMember(Value = " Rio Branco")]
        RioBranco,

        /// <summary>
        /// (UTC-05:00) Chetumal
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Chetumal")]
        UTC0500Chetumal,

        /// <summary>
        /// (UTC-05:00) Eastern Time (US + Canada)
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Eastern Time (US & Canada)")]
        UTC0500EasternTimeUSCanada,

        /// <summary>
        /// (UTC-05:00) Haiti
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Haiti")]
        UTC0500Haiti,

        /// <summary>
        /// (UTC-05:00) Havana
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Havana")]
        UTC0500Havana,

        /// <summary>
        /// (UTC-05:00) Indiana (East)
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Indiana (East)")]
        UTC0500IndianaEast,

        /// <summary>
        /// (UTC-05:00) Turks and Caicos
        /// </summary>
        [EnumMember(Value = "(UTC-05:00) Turks and Caicos")]
        UTC0500TurksandCaicos,

        /// <summary>
        /// (UTC-04:00) Asuncion
        /// </summary>
        [EnumMember(Value = "(UTC-04:00) Asuncion")]
        UTC0400Asuncion,

        /// <summary>
        /// (UTC-04:00) Atlantic Time (Canada)
        /// </summary>
        [EnumMember(Value = "(UTC-04:00) Atlantic Time (Canada)")]
        UTC0400AtlanticTimeCanada,

        /// <summary>
        /// (UTC-04:00) Caracas
        /// </summary>
        [EnumMember(Value = "(UTC-04:00) Caracas")]
        UTC0400Caracas,

        /// <summary>
        /// (UTC-04:00) Cuiaba
        /// </summary>
        [EnumMember(Value = "(UTC-04:00) Cuiaba")]
        UTC0400Cuiaba,

        /// <summary>
        /// (UTC-04:00) Georgetown
        /// </summary>
        [EnumMember(Value = "(UTC-04:00) Georgetown")]
        UTC0400Georgetown,

        /// <summary>
        ///  Manaus
        /// </summary>
        [EnumMember(Value = " Manaus")]
        Manaus,

        /// <summary>
        /// SA Western Standard Time
        /// </summary>
        [EnumMember(Value = "SA Western Standard Time")]
        SAWesternStandardTime,

        /// <summary>
        /// Pacific SA Standard Time
        /// </summary>
        [EnumMember(Value = "Pacific SA Standard Time")]
        PacificSAStandardTime,

        /// <summary>
        /// Newfoundland Standard Time
        /// </summary>
        [EnumMember(Value = "Newfoundland Standard Time")]
        NewfoundlandStandardTime,

        /// <summary>
        /// Tocantins Standard Time
        /// </summary>
        [EnumMember(Value = "Tocantins Standard Time")]
        TocantinsStandardTime,

        /// <summary>
        /// E. South America Standard Time
        /// </summary>
        [EnumMember(Value = "E. South America Standard Time")]
        ESouthAmericaStandardTime,

        /// <summary>
        ///  Fortaleza
        /// </summary>
        [EnumMember(Value = " Fortaleza")]
        Fortaleza,

        /// <summary>
        /// (UTC-03:00) City of Buenos Aires
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) City of Buenos Aires")]
        UTC0300CityofBuenosAires,

        /// <summary>
        /// (UTC-03:00) Greenland
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) Greenland")]
        UTC0300Greenland,

        /// <summary>
        /// (UTC-03:00) Montevideo
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) Montevideo")]
        UTC0300Montevideo,

        /// <summary>
        /// (UTC-03:00) Punta Arenas
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) Punta Arenas")]
        UTC0300PuntaArenas,

        /// <summary>
        /// (UTC-03:00) Saint Pierre and Miquelon
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) Saint Pierre and Miquelon")]
        UTC0300SaintPierreandMiquelon,

        /// <summary>
        /// (UTC-03:00) Salvador
        /// </summary>
        [EnumMember(Value = "(UTC-03:00) Salvador")]
        UTC0300Salvador,

        /// <summary>
        /// (UTC-02:00) Co-ordinated Universal Time-02
        /// </summary>
        [EnumMember(Value = "(UTC-02:00) Co-ordinated Universal Time-02")]
        UTC0200CoordinatedUniversalTime02,

        /// <summary>
        /// (UTC-01:00) Azores
        /// </summary>
        [EnumMember(Value = "(UTC-01:00) Azores")]
        UTC0100Azores,

        /// <summary>
        /// (UTC-01:00) Cabo Verde Is.
        /// </summary>
        [EnumMember(Value = "(UTC-01:00) Cabo Verde Is.")]
        UTC0100CaboVerdeIs,

        /// <summary>
        /// (UTC) Co-ordinated Universal Time
        /// </summary>
        [EnumMember(Value = "(UTC) Co-ordinated Universal Time")]
        UTCCoordinatedUniversalTime,

        /// <summary>
        /// (UTC+00:00) Dublin
        /// </summary>
        [EnumMember(Value = "(UTC+00:00) Dublin")]
        UTC0000Dublin,

        /// <summary>
        ///  Lisbon
        /// </summary>
        [EnumMember(Value = " Lisbon")]
        Lisbon,

        /// <summary>
        /// GMT Standard Time
        /// </summary>
        [EnumMember(Value = "GMT Standard Time")]
        GMTStandardTime,

        /// <summary>
        ///  Reykjavik
        /// </summary>
        [EnumMember(Value = " Reykjavik")]
        Reykjavik,

        /// <summary>
        /// (UTC+00:00) Sao Tome
        /// </summary>
        [EnumMember(Value = "(UTC+00:00) Sao Tome")]
        UTC0000SaoTome,

        /// <summary>
        /// (UTC+01:00) Casablanca
        /// </summary>
        [EnumMember(Value = "(UTC+01:00) Casablanca")]
        UTC0100Casablanca,

        /// <summary>
        /// (UTC+01:00) Amsterdam
        /// </summary>
        [EnumMember(Value = "(UTC+01:00) Amsterdam")]
        UTC0100Amsterdam,

        /// <summary>
        ///  Bern
        /// </summary>
        [EnumMember(Value = " Bern")]
        Bern,

        /// <summary>
        ///  Stockholm
        /// </summary>
        [EnumMember(Value = " Stockholm")]
        Stockholm,

        /// <summary>
        /// W. Europe Standard Time
        /// </summary>
        [EnumMember(Value = "W. Europe Standard Time")]
        WEuropeStandardTime,

        /// <summary>
        ///  Bratislava
        /// </summary>
        [EnumMember(Value = " Bratislava")]
        Bratislava,

        /// <summary>
        ///  Ljubljana
        /// </summary>
        [EnumMember(Value = " Ljubljana")]
        Ljubljana,

        /// <summary>
        /// Central Europe Standard Time
        /// </summary>
        [EnumMember(Value = "Central Europe Standard Time")]
        CentralEuropeStandardTime,

        /// <summary>
        ///  Copenhagen
        /// </summary>
        [EnumMember(Value = " Copenhagen")]
        Copenhagen,

        /// <summary>
        ///  Paris
        /// </summary>
        [EnumMember(Value = " Paris")]
        Paris,

        /// <summary>
        /// (UTC+01:00) Sarajevo
        /// </summary>
        [EnumMember(Value = "(UTC+01:00) Sarajevo")]
        UTC0100Sarajevo,

        /// <summary>
        ///  Warsaw
        /// </summary>
        [EnumMember(Value = " Warsaw")]
        Warsaw,

        /// <summary>
        /// Central European Standard Time
        /// </summary>
        [EnumMember(Value = "Central European Standard Time")]
        CentralEuropeanStandardTime,

        /// <summary>
        /// W. Central Africa Standard Time
        /// </summary>
        [EnumMember(Value = "W. Central Africa Standard Time")]
        WCentralAfricaStandardTime,

        /// <summary>
        /// Jordan Standard Time
        /// </summary>
        [EnumMember(Value = "Jordan Standard Time")]
        JordanStandardTime,

        /// <summary>
        ///  Bucharest
        /// </summary>
        [EnumMember(Value = " Bucharest")]
        Bucharest,

        /// <summary>
        /// (UTC+02:00) Beirut
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Beirut")]
        UTC0200Beirut,

        /// <summary>
        /// (UTC+02:00) Cairo
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Cairo")]
        UTC0200Cairo,

        /// <summary>
        /// (UTC+02:00) Chisinau
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Chisinau")]
        UTC0200Chisinau,

        /// <summary>
        /// (UTC+02:00) Damascus
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Damascus")]
        UTC0200Damascus,

        /// <summary>
        /// (UTC+02:00) Gaza
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Gaza")]
        UTC0200Gaza,

        /// <summary>
        /// West Bank Standard Time
        /// </summary>
        [EnumMember(Value = "West Bank Standard Time")]
        WestBankStandardTime,

        /// <summary>
        ///  Pretoria
        /// </summary>
        [EnumMember(Value = " Pretoria")]
        Pretoria,

        /// <summary>
        /// (UTC+02:00) Helsinki
        /// </summary>
        [EnumMember(Value = "(UTC+02:00) Helsinki")]
        UTC0200Helsinki,

        /// <summary>
        ///  Riga
        /// </summary>
        [EnumMember(Value = " Riga")]
        Riga,

        /// <summary>
        ///  Tallinn
        /// </summary>
        [EnumMember(Value = " Tallinn")]
        Tallinn,

        /// <summary>
        /// FLE Standard Time
        /// </summary>
        [EnumMember(Value = "FLE Standard Time")]
        FLEStandardTime,

        /// <summary>
        /// Israel Standard Time
        /// </summary>
        [EnumMember(Value = "Israel Standard Time")]
        IsraelStandardTime,

        /// <summary>
        /// South Sudan Standard Time
        /// </summary>
        [EnumMember(Value = "South Sudan Standard Time")]
        SouthSudanStandardTime,

        /// <summary>
        /// Kaliningrad Standard Time
        /// </summary>
        [EnumMember(Value = "Kaliningrad Standard Time")]
        KaliningradStandardTime,

        /// <summary>
        /// Sudan Standard Time
        /// </summary>
        [EnumMember(Value = "Sudan Standard Time")]
        SudanStandardTime,

        /// <summary>
        /// Libya Standard Time
        /// </summary>
        [EnumMember(Value = "Libya Standard Time")]
        LibyaStandardTime,

        /// <summary>
        /// Namibia Standard Time
        /// </summary>
        [EnumMember(Value = "Namibia Standard Time")]
        NamibiaStandardTime,

        /// <summary>
        /// Arabic Standard Time
        /// </summary>
        [EnumMember(Value = "Arabic Standard Time")]
        ArabicStandardTime,

        /// <summary>
        /// Turkey Standard Time
        /// </summary>
        [EnumMember(Value = "Turkey Standard Time")]
        TurkeyStandardTime,

        /// <summary>
        ///  Riyadh
        /// </summary>
        [EnumMember(Value = " Riyadh")]
        Riyadh,

        /// <summary>
        /// (UTC+03:00) Minsk
        /// </summary>
        [EnumMember(Value = "(UTC+03:00) Minsk")]
        UTC0300Minsk,

        /// <summary>
        /// (UTC+03:00) Moscow
        /// </summary>
        [EnumMember(Value = "(UTC+03:00) Moscow")]
        UTC0300Moscow,

        /// <summary>
        /// Russian Standard Time
        /// </summary>
        [EnumMember(Value = "Russian Standard Time")]
        RussianStandardTime,

        /// <summary>
        /// E. Africa Standard Time
        /// </summary>
        [EnumMember(Value = "E. Africa Standard Time")]
        EAfricaStandardTime,

        /// <summary>
        /// Volgograd Standard Time
        /// </summary>
        [EnumMember(Value = "Volgograd Standard Time")]
        VolgogradStandardTime,

        /// <summary>
        /// Iran Standard Time
        /// </summary>
        [EnumMember(Value = "Iran Standard Time")]
        IranStandardTime,

        /// <summary>
        ///  Muscat
        /// </summary>
        [EnumMember(Value = " Muscat")]
        Muscat,

        /// <summary>
        /// (UTC+04:00) Astrakhan
        /// </summary>
        [EnumMember(Value = "(UTC+04:00) Astrakhan")]
        UTC0400Astrakhan,

        /// <summary>
        /// Astrakhan Standard Time
        /// </summary>
        [EnumMember(Value = "Astrakhan Standard Time")]
        AstrakhanStandardTime,

        /// <summary>
        /// Azerbaijan Standard Time
        /// </summary>
        [EnumMember(Value = "Azerbaijan Standard Time")]
        AzerbaijanStandardTime,

        /// <summary>
        ///  Samara
        /// </summary>
        [EnumMember(Value = " Samara")]
        Samara,

        /// <summary>
        /// (UTC+04:00) Port Louis
        /// </summary>
        [EnumMember(Value = "(UTC+04:00) Port Louis")]
        UTC0400PortLouis,

        /// <summary>
        /// (UTC+04:00) Saratov
        /// </summary>
        [EnumMember(Value = "(UTC+04:00) Saratov")]
        UTC0400Saratov,

        /// <summary>
        /// (UTC+04:00) Tbilisi
        /// </summary>
        [EnumMember(Value = "(UTC+04:00) Tbilisi")]
        UTC0400Tbilisi,

        /// <summary>
        /// (UTC+04:00) Yerevan
        /// </summary>
        [EnumMember(Value = "(UTC+04:00) Yerevan")]
        UTC0400Yerevan,

        /// <summary>
        /// (UTC+04:30) Kabul
        /// </summary>
        [EnumMember(Value = "(UTC+04:30) Kabul")]
        UTC0430Kabul,

        /// <summary>
        /// (UTC+05:00) Ashgabat
        /// </summary>
        [EnumMember(Value = "(UTC+05:00) Ashgabat")]
        UTC0500Ashgabat,

        /// <summary>
        /// West Asia Standard Time
        /// </summary>
        [EnumMember(Value = "West Asia Standard Time")]
        WestAsiaStandardTime,

        /// <summary>
        /// Ekaterinburg Standard Time
        /// </summary>
        [EnumMember(Value = "Ekaterinburg Standard Time")]
        EkaterinburgStandardTime,

        /// <summary>
        ///  Karachi
        /// </summary>
        [EnumMember(Value = " Karachi")]
        Karachi,

        /// <summary>
        /// (UTC+05:00) Qyzylorda
        /// </summary>
        [EnumMember(Value = "(UTC+05:00) Qyzylorda")]
        UTC0500Qyzylorda,

        /// <summary>
        /// (UTC+05:30) Chennai
        /// </summary>
        [EnumMember(Value = "(UTC+05:30) Chennai")]
        UTC0530Chennai,

        /// <summary>
        ///  Mumbai
        /// </summary>
        [EnumMember(Value = " Mumbai")]
        Mumbai,

        /// <summary>
        /// India Standard Time
        /// </summary>
        [EnumMember(Value = "India Standard Time")]
        IndiaStandardTime,

        /// <summary>
        /// Sri Lanka Standard Time
        /// </summary>
        [EnumMember(Value = "Sri Lanka Standard Time")]
        SriLankaStandardTime,

        /// <summary>
        /// Nepal Standard Time
        /// </summary>
        [EnumMember(Value = "Nepal Standard Time")]
        NepalStandardTime,

        /// <summary>
        /// Central Asia Standard Time
        /// </summary>
        [EnumMember(Value = "Central Asia Standard Time")]
        CentralAsiaStandardTime,

        /// <summary>
        /// Bangladesh Standard Time
        /// </summary>
        [EnumMember(Value = "Bangladesh Standard Time")]
        BangladeshStandardTime,

        /// <summary>
        /// Omsk Standard Time
        /// </summary>
        [EnumMember(Value = "Omsk Standard Time")]
        OmskStandardTime,

        /// <summary>
        /// Myanmar Standard Time
        /// </summary>
        [EnumMember(Value = "Myanmar Standard Time")]
        MyanmarStandardTime,

        /// <summary>
        ///  Hanoi
        /// </summary>
        [EnumMember(Value = " Hanoi")]
        Hanoi,

        /// <summary>
        /// SE Asia Standard Time
        /// </summary>
        [EnumMember(Value = "SE Asia Standard Time")]
        SEAsiaStandardTime,

        /// <summary>
        ///  Gorno-Altaysk
        /// </summary>
        [EnumMember(Value = " Gorno-Altaysk")]
        GornoAltaysk,

        /// <summary>
        /// (UTC+07:00) Hovd
        /// </summary>
        [EnumMember(Value = "(UTC+07:00) Hovd")]
        UTC0700Hovd,

        /// <summary>
        /// (UTC+07:00) Krasnoyarsk
        /// </summary>
        [EnumMember(Value = "(UTC+07:00) Krasnoyarsk")]
        UTC0700Krasnoyarsk,

        /// <summary>
        /// (UTC+07:00) Novosibirsk
        /// </summary>
        [EnumMember(Value = "(UTC+07:00) Novosibirsk")]
        UTC0700Novosibirsk,

        /// <summary>
        /// (UTC+07:00) Tomsk
        /// </summary>
        [EnumMember(Value = "(UTC+07:00) Tomsk")]
        UTC0700Tomsk,

        /// <summary>
        /// (UTC+08:00) Beijing
        /// </summary>
        [EnumMember(Value = "(UTC+08:00) Beijing")]
        UTC0800Beijing,

        /// <summary>
        ///  Hong Kong SAR
        /// </summary>
        [EnumMember(Value = " Hong Kong SAR")]
        HongKongSAR,

        /// <summary>
        /// China Standard Time
        /// </summary>
        [EnumMember(Value = "China Standard Time")]
        ChinaStandardTime,

        /// <summary>
        /// North Asia East Standard Time
        /// </summary>
        [EnumMember(Value = "North Asia East Standard Time")]
        NorthAsiaEastStandardTime,

        /// <summary>
        ///  Singapore
        /// </summary>
        [EnumMember(Value = " Singapore")]
        Singapore,

        /// <summary>
        /// (UTC+08:00) Perth
        /// </summary>
        [EnumMember(Value = "(UTC+08:00) Perth")]
        UTC0800Perth,

        /// <summary>
        /// (UTC+08:00) Taipei
        /// </summary>
        [EnumMember(Value = "(UTC+08:00) Taipei")]
        UTC0800Taipei,

        /// <summary>
        /// (UTC+08:00) Ulaanbaatar
        /// </summary>
        [EnumMember(Value = "(UTC+08:00) Ulaanbaatar")]
        UTC0800Ulaanbaatar,

        /// <summary>
        /// (UTC+08:45) Eucla
        /// </summary>
        [EnumMember(Value = "(UTC+08:45) Eucla")]
        UTC0845Eucla,

        /// <summary>
        /// (UTC+09:00) Chita
        /// </summary>
        [EnumMember(Value = "(UTC+09:00) Chita")]
        UTC0900Chita,

        /// <summary>
        /// (UTC+09:00) Osaka
        /// </summary>
        [EnumMember(Value = "(UTC+09:00) Osaka")]
        UTC0900Osaka,

        /// <summary>
        ///  Tokyo
        /// </summary>
        [EnumMember(Value = " Tokyo")]
        Tokyo,

        /// <summary>
        /// (UTC+09:00) Pyongyang
        /// </summary>
        [EnumMember(Value = "(UTC+09:00) Pyongyang")]
        UTC0900Pyongyang,

        /// <summary>
        /// (UTC+09:00) Seoul
        /// </summary>
        [EnumMember(Value = "(UTC+09:00) Seoul")]
        UTC0900Seoul,

        /// <summary>
        /// (UTC+09:00) Yakutsk
        /// </summary>
        [EnumMember(Value = "(UTC+09:00) Yakutsk")]
        UTC0900Yakutsk,

        /// <summary>
        /// (UTC+09:30) Adelaide
        /// </summary>
        [EnumMember(Value = "(UTC+09:30) Adelaide")]
        UTC0930Adelaide,

        /// <summary>
        /// (UTC+09:30) Darwin
        /// </summary>
        [EnumMember(Value = "(UTC+09:30) Darwin")]
        UTC0930Darwin,

        /// <summary>
        /// (UTC+10:00) Brisbane
        /// </summary>
        [EnumMember(Value = "(UTC+10:00) Brisbane")]
        UTC1000Brisbane,

        /// <summary>
        /// (UTC+10:00) Canberra
        /// </summary>
        [EnumMember(Value = "(UTC+10:00) Canberra")]
        UTC1000Canberra,

        /// <summary>
        ///  Sydney
        /// </summary>
        [EnumMember(Value = " Sydney")]
        Sydney,

        /// <summary>
        /// (UTC+10:00) Guam
        /// </summary>
        [EnumMember(Value = "(UTC+10:00) Guam")]
        UTC1000Guam,

        /// <summary>
        /// West Pacific Standard Time
        /// </summary>
        [EnumMember(Value = "West Pacific Standard Time")]
        WestPacificStandardTime,

        /// <summary>
        /// Tasmania Standard Time
        /// </summary>
        [EnumMember(Value = "Tasmania Standard Time")]
        TasmaniaStandardTime,

        /// <summary>
        /// Vladivostok Standard Time
        /// </summary>
        [EnumMember(Value = "Vladivostok Standard Time")]
        VladivostokStandardTime,

        /// <summary>
        /// Lord Howe Standard Time
        /// </summary>
        [EnumMember(Value = "Lord Howe Standard Time")]
        LordHoweStandardTime,

        /// <summary>
        /// Bougainville Standard Time
        /// </summary>
        [EnumMember(Value = "Bougainville Standard Time")]
        BougainvilleStandardTime,

        /// <summary>
        /// Russia Time Zone 10
        /// </summary>
        [EnumMember(Value = "Russia Time Zone 10")]
        RussiaTimeZone10,

        /// <summary>
        /// Magadan Standard Time
        /// </summary>
        [EnumMember(Value = "Magadan Standard Time")]
        MagadanStandardTime,

        /// <summary>
        /// Norfolk Standard Time
        /// </summary>
        [EnumMember(Value = "Norfolk Standard Time")]
        NorfolkStandardTime,

        /// <summary>
        /// Sakhalin Standard Time
        /// </summary>
        [EnumMember(Value = "Sakhalin Standard Time")]
        SakhalinStandardTime,

        /// <summary>
        ///  New Caledonia
        /// </summary>
        [EnumMember(Value = " New Caledonia")]
        NewCaledonia,

        /// <summary>
        /// (UTC+12:00) Anadyr
        /// </summary>
        [EnumMember(Value = "(UTC+12:00) Anadyr")]
        UTC1200Anadyr,

        /// <summary>
        /// Russia Time Zone 11
        /// </summary>
        [EnumMember(Value = "Russia Time Zone 11")]
        RussiaTimeZone11,

        /// <summary>
        ///  Wellington
        /// </summary>
        [EnumMember(Value = " Wellington")]
        Wellington,

        /// <summary>
        /// (UTC+12:00) Co-ordinated Universal Time+12
        /// </summary>
        [EnumMember(Value = "(UTC+12:00) Co-ordinated Universal Time+12")]
        UTC1200CoordinatedUniversalTime12,

        /// <summary>
        /// (UTC+12:00) Fiji
        /// </summary>
        [EnumMember(Value = "(UTC+12:00) Fiji")]
        UTC1200Fiji,

        /// <summary>
        /// (UTC+12:45) Chatham Islands
        /// </summary>
        [EnumMember(Value = "(UTC+12:45) Chatham Islands")]
        UTC1245ChathamIslands,

        /// <summary>
        /// (UTC+13:00) Co-ordinated Universal Time+13
        /// </summary>
        [EnumMember(Value = "(UTC+13:00) Co-ordinated Universal Time+13")]
        UTC1300CoordinatedUniversalTime13,

        /// <summary>
        /// (UTC+13:00) Nuku'alofa
        /// </summary>
        [EnumMember(Value = "(UTC+13:00) Nuku'alofa")]
        UTC1300Nukualofa,

        /// <summary>
        /// (UTC+13:00) Samoa
        /// </summary>
        [EnumMember(Value = "(UTC+13:00) Samoa")]
        UTC1300Samoa,

        /// <summary>
        /// (UTC+14:00) Kiritimati Island
        /// </summary>
        [EnumMember(Value = "(UTC+14:00) Kiritimati Island")]
        UTC1400KiritimatiIsland,


    }
}
