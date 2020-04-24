using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("PnP.Core.Test")]
namespace PnP.Core.Services
{
    /// <summary>
    /// Mode in which the context operates
    /// </summary>
    internal enum PnPContextMode
    {
        Default = 0,
        Record = 1,
        Mock = 2
    }
}
