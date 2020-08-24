using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FileCollection
    {
        #region Add
        public async Task<IFile> AddAsync(string name, Stream content, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }


            var newFile = CreateNewAndAdd() as File;

            newFile.Name = name;

            var additionalInformation = new Dictionary<string, object>()
            {
                { File.AddFileContentAdditionalInformationKey, content },
                { File.AddFileOverwriteAdditionalInformationKey, overwrite }
            };

            return await newFile.AddAsync(additionalInformation).ConfigureAwait(false) as File;
        }

        public IFile Add(string name, Stream content, bool overwrite = false)
        {
            return AddAsync(name, content, overwrite).GetAwaiter().GetResult();
        }
        #endregion
    }
}
