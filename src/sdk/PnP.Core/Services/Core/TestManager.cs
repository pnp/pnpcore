#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Services
{
    /// <summary>
    /// Test manager class, responsible for recording and mocking test data
    /// </summary>
    internal static class TestManager
    {
        private const string MockFolder = "MockData";
        private const string TestSite = "TestSite";
        private const string TestSubSite = "TestSubSite";
        private const string NoGroupTestSite = "NoGroupTestSite";

        /// <summary>
        /// Record a the request for a future mock use
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <param name="requestKey">Key we used to calculate the hash used to identify this request</param>
        /// <param name="request">Actual request that's being sent to the server</param>
        internal static void RecordRequest(PnPContext context, string requestKey, string request)
        {
            // replace possible domain names with something static to ensure the stored test data can be retrieved by other test environments
            requestKey = GeneralizeRequestKey(requestKey, context);

            string hash = SHA256(requestKey);
            string orderPrefix = GetOrderPrefix(requestKey);

            // Construct filename for storing this request
            string fileName = GetRequestFile(context, hash, orderPrefix);
            
            // Write request to a file, overwrites the existing file
            File.WriteAllText(fileName, request);
            
            // Construct filename for storing this request
            string debugFileName = GetDebugFile(context, hash, orderPrefix);

            // Write request key to a file, can be used for debugging
            File.WriteAllText(debugFileName, requestKey);
        }

        /// <summary>
        /// Record the response of a request
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <param name="requestKey">Key we used to calculate the hash used to identify this request</param>
        /// <param name="response">Response that came back from the server</param>
        internal static void RecordResponse(PnPContext context, string requestKey, string response)
        {
            requestKey = GeneralizeRequestKey(requestKey, context);
            
            string fileName = GetResponseFile(context, SHA256(requestKey), GetOrderPrefix(requestKey));

            // Write request to a file, overwrites the existing file
            File.WriteAllText(fileName, response);
        }

        /// <summary>
        /// Verifies if mock data is already available for the current request
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <param name="requestKey">Key we used to calculate the hash, used to identify the response to return</param>
        /// <returns>Returns true if the response from the mock is available</returns>
        internal static bool IsMockAvailable(PnPContext context, string requestKey)
        {
            string fileName = GetResponseFile(context, SHA256(requestKey), GetOrderPrefix(requestKey));
            return File.Exists(fileName);
        }

        /// <summary>
        /// Mocks the response for a given request
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <param name="requestKey">Key we used to calculate the hash, used to identify the response to return</param>
        /// <returns>Server response from the mock response</returns>
        internal static string MockResponse(PnPContext context, string requestKey)
        {
            requestKey = GeneralizeRequestKey(requestKey, context);

            string fileName = GetResponseFile(context, SHA256(requestKey), GetOrderPrefix(requestKey));

            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName);
            }
            else
            {
                throw new ClientException(ErrorType.OfflineDataError, $"Test [{context.TestName}] is missing response file {fileName}. Request key was {requestKey}");
            }
        }

        /// <summary>
        /// Saves test properties that need to be persisted to enable offline testing to a file
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <param name="properties">Dictionary of properties to save</param>
        internal static void SaveProperties(PnPContext context, Dictionary<string, string> properties)
        {
            string fileName = GetPropertiesFile(context);

            var bodyContent = JsonSerializer.Serialize(properties, typeof(Dictionary<string, string>), new JsonSerializerOptions { WriteIndented = false });

            // Write serialized properties to a file, overwrites the existing file if needed
            File.WriteAllText(fileName, bodyContent);
        }

        /// <summary>
        /// Loads persisted test properties into a dictionary
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        /// <returns>Dictionary of properties</returns>
        internal static Dictionary<string, string> GetProperties(PnPContext context)
        {
            string fileName = GetPropertiesFile(context);

            string body;
            if (File.Exists(fileName))
            {
                 body = File.ReadAllText(fileName);
            }
            else
            {
                throw new ClientException(ErrorType.OfflineDataError, $"Test [{context.TestName}] is missing properties file {fileName}.");
            }

            var properties = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            return properties;
        }

        /// <summary>
        /// Deletes the persisted properties
        /// </summary>
        /// <param name="context">Current PnPContext</param>
        internal static void DeleteProperties(PnPContext context)
        {
            string fileName = GetPropertiesFile(context);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);                
            }
        }

        private static string GetResponseFile(PnPContext context, string hash, string orderPrefix)
        {
            //return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}-{hash}.response");
            return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}.response");
        }

        private static string GetRequestFile(PnPContext context, string hash, string orderPrefix)
        {
            //return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}-{hash}.request");
            return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}.request");
        }

        private static string GetDebugFile(PnPContext context, string hash, string orderPrefix)
        {
            //return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}-{hash}.debug");
            return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}-{orderPrefix}.debug");
        }

        private static string GetPropertiesFile(PnPContext context)
        {
            return Path.Combine(GetPath(context), $"{context.TestName}-{context.TestId}.properties");
        }

        private static string GetPath(PnPContext context)
        {
            string mainPath = context.TestFilePath.Substring(0, context.TestFilePath.LastIndexOf(Path.DirectorySeparatorChar));
            string lastPart = context.TestFilePath.Substring(context.TestFilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            string pathName = Path.Combine(mainPath, MockFolder, lastPart);
            Directory.CreateDirectory(pathName);
            return pathName;
        }

        private static string GetOrderPrefix(string requestKey)
        {
            return string.Format("{0,5:00000}", int.Parse(requestKey.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries)[0]));
        }

        private static string SHA256(string text)
        {
            var result = default(string);

            using (var algo = new SHA256Managed())
            {
                result = GenerateHashString(algo, text);
            }

            return result;
        }

        private static string GenerateHashString(HashAlgorithm algo, string text)
        {
            // Compute hash from text parameter
            algo.ComputeHash(Encoding.UTF8.GetBytes(text));

            // Get has value in array of bytes
            var result = algo.Hash;

            // Return as hexadecimal string
            return string.Join(
                string.Empty,
                result.Select(x => x.ToString("x2")));
        }

        private static string GeneralizeRequestKey(string requestKey, PnPContext context)
        {
            if (string.IsNullOrEmpty(requestKey))
            {
                return requestKey;
            }

            // Replace TestSubSite urls
            if (context.TestUris.ContainsKey(TestSubSite))
            {
                requestKey = requestKey.Replace(context.TestUris[TestSubSite].ToString(), "https://pnprocks.sharepoint.com/sites/testsite/subsite");
                requestKey = requestKey.Replace(context.TestUris[TestSubSite].PathAndQuery, "/sites/testsite/subsite");
            }
            if (context.TestUris.ContainsKey(TestSite))
            {
                requestKey = requestKey.Replace(context.TestUris[TestSite].ToString(), "https://pnprocks.sharepoint.com/sites/testsite");
                requestKey = requestKey.Replace(context.TestUris[TestSite].PathAndQuery, "/sites/testsite");
            }
            if (context.TestUris.ContainsKey(NoGroupTestSite))
            {
                requestKey = requestKey.Replace(context.TestUris[NoGroupTestSite].ToString(), "https://pnprocks.sharepoint.com/sites/testsitenogroup");
                requestKey = requestKey.Replace(context.TestUris[NoGroupTestSite].PathAndQuery, "/sites/testsitenogroup");
            }

            requestKey =  requestKey.Replace(context.Uri.DnsSafeHost, "pnprocks.sharepoint.com");

            return requestKey;
        }

    }
}
#endif