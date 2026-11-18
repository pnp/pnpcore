using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PnP.Core.Modernization.Utilities
{
    /// <summary>
    /// Class that's responsible for loading (mapping) files
    /// </summary>
    public class FileManager: BaseTransform
    {

        #region Construction
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logObservers">Connected loggers</param>
        public FileManager(IList<ILogObserver> logObservers = null): base()
        {
            //Register any existing observers
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }
        }
        #endregion

        /// <summary>
        /// Loads a URL mapping file
        /// </summary>
        /// <param name="mappingFile">Path to the mapping file</param>
        /// <returns>A collection of URLMapping objects</returns>
        public List<UrlMapping> LoadUrlMappingFile(string mappingFile)
        {
            List<UrlMapping> urlMappings = new List<UrlMapping>();

            LogInfo(string.Format(LogStrings.LoadingUrlMappingFile, mappingFile), LogStrings.Heading_UrlRewriter);

            if (File.Exists(mappingFile))
            {
                var lines = File.ReadLines(mappingFile);

                if (lines.Any())
                {
                    string delimiter = this.DetectDelimiter(lines);

                    foreach(var line in lines)
                    {
                        var split = line.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                        if (split.Length == 2)
                        {
                            string fromUrl = split[0];
                            string toUrl = split[1];

                            if (!string.IsNullOrEmpty(fromUrl) && !string.IsNullOrEmpty(toUrl))
                            {
                                urlMappings.Add(new UrlMapping() { SourceUrl = fromUrl, TargetUrl = toUrl });
                                //LogDebug(string.Format(LogStrings.UrlMappingLoaded, fromUrl, toUrl), LogStrings.Heading_UrlRewriter);
                            }
                        }
                    }
                }
            }
            else
            {
                LogError(string.Format(LogStrings.Error_UrlMappingFileNotFound, mappingFile), LogStrings.Heading_UrlRewriter);
                throw new Exception(string.Format(LogStrings.Error_UrlMappingFileNotFound, mappingFile));
            }

            return urlMappings;
        }


        /// <summary>
        /// Loads a URL mapping file
        /// </summary>
        /// <param name="mappingFile">Path to the mapping file</param>
        /// <returns>A collection of URLMapping objects</returns>
        public List<TermMapping> LoadTermMappingFile(string mappingFile)
        {
            List<TermMapping> termMappings = new List<TermMapping>();

            LogInfo(string.Format(LogStrings.Term_LoadingMappingFile, mappingFile), LogStrings.Heading_TermMapping);

            if (File.Exists(mappingFile))
            {
                var lines = File.ReadLines(mappingFile);

                if (lines.Any())
                {
                    string delimiter = this.DetectDelimiter(lines);

                    foreach (var line in lines)
                    {
                        var split = line.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                        if (split.Length == 2)
                        {
                            string sourceTerm = split[0];
                            string targetTerm = split[1];

                            if (!string.IsNullOrEmpty(sourceTerm) && !string.IsNullOrEmpty(targetTerm))
                            {
                                termMappings.Add(new TermMapping() { SourceTerm = sourceTerm, TargetTerm = targetTerm });
                                //LogDebug(string.Format(LogStrings.Term_MappingLoaded, sourceTerm, targetTerm), LogStrings.Heading_TermMapping);
                            }
                        }
                    }
                }
            }
            else
            {
                LogError(string.Format("Term mapping file {0} not found", mappingFile), LogStrings.Heading_UrlRewriter);
                throw new Exception(string.Format("Term mapping file {0} not found", mappingFile));
            }

            return termMappings;
        }

        /// <summary>
        /// Load User Mapping File
        /// </summary>
        /// <param name="mappingFile"></param>
        /// <returns></returns>
        public List<UserMappingEntity> LoadUserMappingFile(string mappingFile)
        {
            List<UserMappingEntity> userMappings = new List<UserMappingEntity>();

            LogInfo(string.Format(LogStrings.LoadingUserMappingFile, mappingFile), LogStrings.Heading_UserMapping);

            if (File.Exists(mappingFile))
            {
                var lines = File.ReadLines(mappingFile);

                if (lines.Any())
                {
                    string delimiter = this.DetectDelimiter(lines);

                    foreach (var line in lines)
                    {
                        var split = line.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                        if (split.Length == 2)
                        {
                            string sourceUser = split[0];
                            string targetUser = split[1];

                            if (!string.IsNullOrEmpty(sourceUser) && !string.IsNullOrEmpty(targetUser))
                            {
                                userMappings.Add(new UserMappingEntity() { SourceUser = sourceUser, TargetUser = targetUser });
                                //LogDebug(string.Format(LogStrings.UserMappingLoaded, sourceUser, targetUser), LogStrings.Heading_UserMapping);
                            }
                        }
                    }
                }
            }
            else
            {
                LogError(string.Format(LogStrings.Error_UserMappingFileNotFound, mappingFile), LogStrings.Heading_UserMapping);
                throw new Exception(string.Format(LogStrings.Error_UserMappingFileNotFound, mappingFile));
            }

            return userMappings;
        }

        #region Helper methods
        private string DetectDelimiter(IEnumerable<string> lines)
        {
            if (lines.First().IndexOf(',') > 0)
            {
                return ",";
            }
            else if (lines.First().IndexOf(';') > 0)
            {
                return ";";
            }

            return "";
        }
        #endregion
    }
}
