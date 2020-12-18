using System.Resources;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PnP.Core.Auth")]
[assembly: InternalsVisibleTo("PnP.Framework")]

#if DEBUG
[assembly: InternalsVisibleTo("PnP.Core.Test")]
[assembly: InternalsVisibleTo("PnP.Core.Auth.Test")]
[assembly: InternalsVisibleTo("PnP.Framework.Test")]
[assembly: InternalsVisibleTo("PnP.Framework.Modernization.Test")]
#endif

[assembly: NeutralResourcesLanguage("en")]