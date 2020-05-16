namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings
    /// </summary>
    public interface ISettings
    {
        string VersionTag { get; }
        string UserAgent { get; }
    }
}
