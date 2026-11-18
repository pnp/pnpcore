using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Modernization.Telemetry
{
    /// <summary>
    /// Class handling telemetry
    /// </summary>
    public class PageTelemetry
    {
        private readonly TelemetryClient telemetryClient;
        private readonly TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();       
        private Guid aadTenantId;
        private string version;

        private const string PageTransformed = "PageTransformed";
        private const string EngineVersion = "Version";
        private const string AADTenantId = "AADTenantId";
        private const string SourceVersion = "SourceVersion";
        private const string TargetVersion = "TargetVersion";
        private const string SourceVersionNumber = "SourceVersionNumber";
        private const string TargetVersionNumber = "TargetVersionNumber";
        private const string PageType = "PageType";
        private const string Duration = "Duration";
        private const string CrossFarm = "CrossFarm";
        private const string CrossSite = "CrossSite";

        #region Construction
        /// <summary>
        /// Instantiates the telemetry client
        /// </summary>
        public PageTelemetry(string version)
        {
            try
            {
                this.version = version;

#pragma warning disable CS0618 // Type or member is obsolete
                this.telemetryConfiguration.InstrumentationKey = "373400f5-a9cc-48f3-8298-3fd7f4c063d6";
#pragma warning restore CS0618 // Type or member is obsolete

                this.telemetryClient = new TelemetryClient(this.telemetryConfiguration);
                
                this.telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                this.telemetryClient.Context.Cloud.RoleInstance = "SharePointPnPPageTransformation";
                this.telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            }
            catch
            {
                this.telemetryClient = null;
            }
        }
        #endregion

        /// <summary>
        /// Sends a transformation done event
        /// </summary>
        /// <param name="duration">Duration of the page transformation</param>
        /// <param name="pageType">Type of page that was transformed</param>
        /// <param name="baseTransformationInformation">Information about the transformation</param>
        public void LogTransformationDone(TimeSpan duration, string pageType, BaseTransformationInformation baseTransformationInformation)
        {
            if (this.telemetryClient == null)
            {
                return;
            }

            try
            {
                // Prepare event data
                Dictionary<string, string> properties = new Dictionary<string, string>(10);
                Dictionary<string, double> metrics = new Dictionary<string, double>(5);

                // Populate properties

                // Page transformation engine version
                properties.Add(EngineVersion, this.version);
                // Type of page being transformed
                properties.Add(PageType, pageType);
                // In-Place upgrade or cross farm
                properties.Add(CrossSite, baseTransformationInformation.IsCrossSiteTransformation.ToString());
                // Type of transform (intra or cross farm)
                properties.Add(CrossFarm, baseTransformationInformation.IsCrossFarmTransformation.ToString());
                // SharePoint Environments
                properties.Add(SourceVersion, NormalizeSharePointVersions(baseTransformationInformation.SourceVersion).ToString());
                properties.Add(SourceVersionNumber, baseTransformationInformation.SourceVersionNumber);
                properties.Add(TargetVersion, NormalizeSharePointVersions(baseTransformationInformation.TargetVersion).ToString());
                properties.Add(TargetVersionNumber, baseTransformationInformation.TargetVersionNumber);
                // Azure AD tenant
                properties.Add(AADTenantId, this.aadTenantId.ToString());

                // Populate metrics
                if (duration != TimeSpan.Zero)
                {
                    // How long did it take to transform this page
                    metrics.Add(Duration, duration.TotalSeconds);
                }

                // Send the event
                this.telemetryClient.TrackEvent(PageTransformed, properties, metrics);
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        /// <summary>
        /// Logs a page transformation error
        /// </summary>
        /// <param name="ex">Exception object</param>
        /// <param name="location">Location that generated this error</param>
        public void LogError(Exception ex, string location)
        {
            if (this.telemetryClient == null || ex == null)
            {
                return;
            }

            try
            {
                // Prepare event data
                Dictionary<string, string> properties = new Dictionary<string, string>();
                Dictionary<string, double> metrics = new Dictionary<string, double>();

                if (!string.IsNullOrEmpty(location))
                {
                    properties.Add("Location", location);
                }
                // Azure AD tenant
                properties.Add(AADTenantId, this.aadTenantId.ToString());

                this.telemetryClient.TrackException(ex, properties, metrics);
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        /// <summary>
        /// Ensure telemetry data is send to server
        /// </summary>
        public void Flush()
        {
            try
            {
                // before exit, flush the remaining data
                this.telemetryClient.Flush();
            }
            catch
            {
                // Eat all exceptions
            }
        }

        #region Helper methods
        internal Guid LoadAADTenantId(PnPContext context)
        {
            
            // Load from cache if possible, if not obtain aad tenant id
            Uri spSiteUri = new Uri(context.Uri.AbsoluteUri);
            Uri urlUri = new Uri($"{spSiteUri.Scheme}://{spSiteUri.DnsSafeHost}");

            var tenantIdFromCache = CacheManager.Instance.GetAADTenantId(urlUri);
            if (tenantIdFromCache != Guid.Empty)
            {
                this.aadTenantId = tenantIdFromCache;
                return tenantIdFromCache;
            }
            try
            {
                var realm = context.Web.GetAuthenticationRealm();
                CacheManager.Instance.SetAADTenantId(realm, urlUri);
                this.aadTenantId = realm;
                return realm;
            }
            catch (Exception)
            {
                // Telemetry must not prevent a page transformation.
            }

            return Guid.Empty;
        }

        private static SPVersion NormalizeSharePointVersions(SPVersion spVersion)
        {
            if (spVersion == SPVersion.SP2013Legacy)
            {
                spVersion = SPVersion.SP2013;
            }
            if (spVersion == SPVersion.SP2016Legacy)
            {
                spVersion = SPVersion.SP2016;
            }

            return spVersion;
        }
        #endregion
    }
}
