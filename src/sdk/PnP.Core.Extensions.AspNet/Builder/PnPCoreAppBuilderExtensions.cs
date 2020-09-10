using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Extensions.AspNet.Builder
{
    public static class PnPCoreAppBuilderExtensions
    {
        public static IApplicationBuilder UsePnPCore(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // return app.UseMiddleware<>();
            return app;
        }
    }
}
