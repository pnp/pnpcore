namespace PnP.Core.Services
{
    public interface ISettings
    {
        string VersionTag { get; }
        string UserAgent { get; }
    }
}
