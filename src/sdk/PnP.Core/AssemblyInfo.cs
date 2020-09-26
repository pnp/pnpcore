using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PnP.Core.Auth")]

#if DEBUG
[assembly: InternalsVisibleTo("PnP.Core.Test")]
[assembly: InternalsVisibleTo("PnP.Core.Auth.Test")]
#endif
