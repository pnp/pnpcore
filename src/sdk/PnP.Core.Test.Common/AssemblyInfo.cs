using System.Resources;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("PnP.Core.Test")]
[assembly: InternalsVisibleTo("PnP.Core.Auth.Test")]
[assembly: InternalsVisibleTo("PnP.Core.Admin.Test")]
#endif

[assembly: NeutralResourcesLanguage("en")]