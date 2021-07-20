using System.Resources;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PnP.Core.Auth")]
[assembly: InternalsVisibleTo("PnP.Framework")]
[assembly: InternalsVisibleTo("PnP.PowerShell")]
[assembly: InternalsVisibleTo("PnP.Core.Transformation")]
[assembly: InternalsVisibleTo("PnP.Core.Transformation.SharePoint")]

#if DEBUG
[assembly: InternalsVisibleTo("PnP.Core.Test")]
[assembly: InternalsVisibleTo("PnP.Core.Auth.Test")]
[assembly: InternalsVisibleTo("PnP.Framework.Test")]
[assembly: InternalsVisibleTo("PnP.Framework.Modernization.Test")]
[assembly: InternalsVisibleTo("PnP.PowerShell.Test")]
#endif

[assembly: NeutralResourcesLanguage("en")]