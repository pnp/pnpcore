namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Enum listing the SharePoint versions as used by the transformation engine
    /// </summary>
    public enum SPVersion
    {
        SPO = 0,
        SP2019 = 1,
        SP2016 = 2,
        SP2016Legacy= 3,
        SP2013 = 4, 
        SP2013Legacy = 5,
        SP2010 = 6,
        Unknown = 100
    }
}
