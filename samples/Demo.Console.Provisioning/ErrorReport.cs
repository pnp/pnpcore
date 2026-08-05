using PnP.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Console.Provisioning
{
    public static class ErrorReport
    {
        public static string Describe(Exception exception)
        {
            if (exception == null)
            {
                return "(no exception)";
            }

            var report = new StringBuilder();
            int depth = 0;

            for (Exception current = exception; current != null; current = current.InnerException)
            {
                string prefix = depth == 0 ? string.Empty : $"--- inner exception {depth} ---";

                if (prefix.Length > 0)
                {
                    report.AppendLine();
                    report.AppendLine(prefix);
                }

                report.AppendLine($"{current.GetType().FullName}: {current.Message}");

                AppendServiceDetail(report, current);

                if (!string.IsNullOrWhiteSpace(current.StackTrace))
                {
                    report.AppendLine("Stack trace:");
                    report.AppendLine(current.StackTrace);
                }

                depth++;
            }

            return report.ToString().TrimEnd();
        }

        /// <summary>
        /// Adds the parts of a PnP Core service failure that the message does not carry.
        /// </summary>
        private static void AppendServiceDetail(StringBuilder report, Exception exception)
        {
            if (!(exception is ServiceException serviceException) || serviceException.Error == null)
            {
                // A client-side failure still carries a typed error worth showing.
                if (exception is PnPException pnpException && pnpException.Error != null)
                {
                    AppendBaseError(report, pnpException.Error);
                }

                return;
            }

            if (serviceException.Error is ServiceError error)
            {
                report.AppendLine($"  HTTP status:        {error.HttpResponseCode}");

                if (!string.IsNullOrEmpty(error.Code))
                {
                    report.AppendLine($"  Service error code: {error.Code}");
                }

                if (!string.IsNullOrEmpty(error.ClientRequestId))
                {
                    // The id to quote to Microsoft support, and the one that finds the request in
                    // the tenant's own logs.
                    report.AppendLine($"  Client request id:  {error.ClientRequestId}");
                }

                if (!string.IsNullOrEmpty(error.Message))
                {
                    report.AppendLine($"  Service message:    {error.Message}");
                }
            }

            AppendBaseError(report, serviceException.Error);

            // Last, and always: CsomError puts the text SharePoint returned in ToString() rather
            // than in Message, so an error rendered without this reads as an empty banner.
            string rendered = serviceException.Error.ToString();

            if (!string.IsNullOrWhiteSpace(rendered))
            {
                report.AppendLine("  Raw service error:");

                foreach (string line in rendered.Split('\n'))
                {
                    report.AppendLine($"    {line.TrimEnd('\r')}");
                }
            }
        }

        private static void AppendBaseError(StringBuilder report, BaseError error)
        {
            report.AppendLine($"  Error type:         {error.Type}");

            if (error.PnPCorrelationId != Guid.Empty)
            {
                report.AppendLine($"  PnP correlation id: {error.PnPCorrelationId}");
            }

            if (error.AdditionalData == null || error.AdditionalData.Count == 0)
            {
                return;
            }

            report.AppendLine("  Additional data:");

            foreach (KeyValuePair<string, object> item in error.AdditionalData)
            {
                report.AppendLine($"    {item.Key}: {item.Value}");
            }
        }
    }
}
