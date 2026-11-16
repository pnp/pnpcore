using Microsoft.SharePoint.Client;
using PnP.Core.Modernization.Transform;
using System.IO;
using File = Microsoft.SharePoint.Client.File;

namespace PnP.Core.Modernization.Extensions
{
    public static class FileFolderExtensions
    {

        /// <summary>
        /// Returns a file as string
        /// </summary>
        /// <param name="web">The Web to process</param>
        /// <param name="serverRelativeUrl">The server relative URL to the file</param>
        /// <returns>The file contents as a string</returns>
        /// <remarks>#
        /// 
        ///     Based on https://github.com/SharePoint/PnP-Sites-Core/blob/master/Core/OfficeDevPnP.Core/Extensions/FileFolderExtensions.cs
        ///     Modified to force onpremises support
        ///     
        /// </remarks>
        public static string GetFileByServerRelativeUrlAsString(this Web web, string serverRelativeUrl)
        {

            var file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
            var context = web.Context;
            context.Load(file);
            context.ExecuteQueryRetry();

            var spVersion = BaseTransform.GetVersion(context);

            Stream sourceStream;
            if (spVersion == SPVersion.SP2010)
            {
                throw new System.Exception("SharePoint 2010 is not supported");
                //sourceStream = new MemoryStream();

                //if (context.HasPendingRequest)
                //{
                //    context.ExecuteQueryRetry();
                //}

                //var fileBinary = File.OpenBinaryDirect((ClientContext)context, serverRelativeUrl);
                //context.ExecuteQueryRetry();
                //Stream tempSourceStream = fileBinary.Stream;

                //CopyStream(tempSourceStream, sourceStream);
                //sourceStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                ClientResult<Stream> stream = file.OpenBinaryStream();
                web.Context.ExecuteQueryRetry();
                sourceStream = stream.Value;
            }
            string returnString = string.Empty;

            using (Stream memStream = new MemoryStream())
            {
                CopyStream(sourceStream, memStream);
                memStream.Position = 0;

                using (var reader = new StreamReader(memStream))
                {
                    returnString = reader.ReadToEnd();
                }
            }

            return returnString;
        }

        #region Helper methods
        private static void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[32768];
            int bytesRead;

            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);

            } while (bytesRead != 0);
        }
        #endregion
    }
}
