using PnP.Core.Services;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Renders an exception into text worth putting in a provisioning warning.
    /// </summary>
    internal static class ErrorText
    {
        /// <summary>
        /// A one-line-ish description of what failed and why.
        /// </summary>
        internal static string Describe(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();

            // The whole chain, not just the outermost. A handler that adds context by wrapping -
            // "Updating the site column 'X' failed" - puts the ServiceException one level down, and
            // a renderer that only looks at the top reports the wrapper's text followed by the
            // banner, which is precisely the detail it exists to recover.
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current is ServiceException serviceException && serviceException.Error is ServiceError error)
                {
                    string rendered = error.ToString();

                    if (!string.IsNullOrEmpty(rendered))
                    {
                        parts.Add(rendered);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(error.Message))
                    {
                        parts.Add(error.Message);
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(current.Message))
                {
                    parts.Add(current.Message);
                }
            }

            return string.Join(" | ", parts);
        }
    }
}
