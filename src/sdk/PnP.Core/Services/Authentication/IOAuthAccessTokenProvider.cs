using System;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    public interface IOAuthAccessTokenProvider
    {
        Task<string> GetAccessTokenAsync(Uri resourceUri);
    }
}
