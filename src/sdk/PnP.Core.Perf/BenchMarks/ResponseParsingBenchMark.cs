using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Perf.BenchMarks
{
    [MemoryDiagnoser]
    public class ResponseParsingBenchMark
    {
        private string fileContentsSmall;
        private string fileContentsLarge;        

        public ResponseParsingBenchMark()
        {
#if DEBUG
            fileContentsSmall = File.ReadAllText("../../../BenchMarks/smallrestbatch.response");
            fileContentsLarge = File.ReadAllText("../../../BenchMarks/largerestbatch.response");
#else
            fileContentsSmall = File.ReadAllText(@"D:\github\pnpcore\src\sdk\PnP.Core.Perf\BenchMarks\smallrestbatch.response");
            fileContentsLarge = File.ReadAllText(@"D:\github\pnpcore\src\sdk\PnP.Core.Perf\BenchMarks\largerestbatch.response");
#endif
        }

        [Benchmark(Baseline = true)]
        public string Current()
        {
            CurrentProcessSharePointRestBatchResponseContent(fileContentsSmall);
            CurrentProcessSharePointRestBatchResponseContent(fileContentsLarge);
            return null;
        }

        [Benchmark]
        public string Tuned()
        {
            TunedProcessSharePointRestBatchResponseContent(fileContentsSmall);
            TunedProcessSharePointRestBatchResponseContent (fileContentsLarge);
            return null;
        }

        /*

        |  Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
        |-------- |----------:|----------:|----------:|----------:|------:|--------:|---------:|---------:|---------:|----------:|
        | Current | 17.861 ms | 0.2921 ms | 0.2732 ms | 17.921 ms |  1.00 |    0.00 | 687.5000 | 687.5000 | 687.5000 |     35 MB |
        |   Tuned |  7.092 ms | 0.2919 ms | 0.8606 ms |  6.610 ms |  0.45 |    0.03 | 585.9375 | 578.1250 | 578.1250 |     17 MB |

        */
        /// <summary>
        /// https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
        /// https://timiskhakov.github.io/posts/exploring-spans-and-pipelines
        /// 
        /// </summary>
        /// <param name="batchResponse"></param>
        public string TunedProcessSharePointRestBatchResponseContent(string batchResponse)
        {
            int counter = -1;
            var httpStatusCode = HttpStatusCode.Continue;

            bool responseContentOpen = false;

            StringBuilder responseContent = new StringBuilder();

            foreach (ReadOnlySpan<char> line in batchResponse.SplitLines())
            {
                // Signals the start/end of a response
                // --batchresponse_6ed85e4b-869f-428e-90c9-19038f964718
                if (line.StartsWith("--batchresponse_"))
                {
                    // Reponse was closed, let's store the result 
                    if (responseContentOpen)
                    {
                        httpStatusCode = 0;
                        responseContentOpen = false;
                        //responseContent = new StringBuilder();
                    }

                    counter++;
                }
                // Response status code
                else if (line.StartsWith("HTTP/1.1 "))
                {
                    // HTTP/1.1 200 OK
                    //if (int.TryParse(line.Substring(9, 3), out int parsedHttpStatusCode))
                    if (int.TryParse(line.Slice(9,3), out int parsedHttpStatusCode))
                    {
                        httpStatusCode = (HttpStatusCode)parsedHttpStatusCode;
                    }
                    else
                    {
                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 0, PnPCoreResources.Exception_SharePointRest_UnexpectedResult);
                    }
                }
                // First real content returned, lines before are ignored
                else if ((line.StartsWith("{") || httpStatusCode == HttpStatusCode.NoContent) && !responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
                    responseContent.Append(line).AppendLine();
                    responseContentOpen = true;
                }
                // More content is being returned, so let's append it
                else if (responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
                    responseContent.Append(line).AppendLine();
                }
            }

            return responseContent.ToString();
        }


        public string CurrentProcessSharePointRestBatchResponseContent(string batchResponse)
        {
            var responseLines = batchResponse.Split(new char[] { '\n' });
            int counter = -1;
            var httpStatusCode = HttpStatusCode.Continue;

            bool responseContentOpen = false;
            StringBuilder responseContent = new StringBuilder();
            foreach (var line in responseLines)
            {
                // Signals the start/end of a response
                // --batchresponse_6ed85e4b-869f-428e-90c9-19038f964718
                if (line.StartsWith("--batchresponse_"))
                {
                    // Reponse was closed, let's store the result 
                    if (responseContentOpen)
                    {
                        httpStatusCode = 0;
                        responseContentOpen = false;
                        //responseContent = new StringBuilder();
                    }

                    counter++;
                }
                // Response status code
                else if (line.StartsWith("HTTP/1.1 "))
                {
                    // HTTP/1.1 200 OK
                    if (int.TryParse(line.Substring(9, 3), out int parsedHttpStatusCode))
                    {
                        httpStatusCode = (HttpStatusCode)parsedHttpStatusCode;
                    }
                    else
                    {
                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 0, PnPCoreResources.Exception_SharePointRest_UnexpectedResult);
                    }
                }
                // First real content returned, lines before are ignored
                else if ((line.StartsWith("{") || httpStatusCode == HttpStatusCode.NoContent) && !responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
                    responseContent.AppendLine(line.TrimEnd('\r'));
                    responseContentOpen = true;
                }
                // More content is being returned, so let's append it
                else if (responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
                    responseContent.AppendLine(line.TrimEnd('\r'));
                }
            }

            return responseContent.ToString();
        }

        private static bool HttpRequestSucceeded(HttpStatusCode httpStatusCode)
        {
            // See https://restfulapi.net/http-status-codes/
            // For now let's fail all except the 200 range
            return (httpStatusCode >= HttpStatusCode.OK &&
                httpStatusCode < HttpStatusCode.Ambiguous);
        }

    }
}
