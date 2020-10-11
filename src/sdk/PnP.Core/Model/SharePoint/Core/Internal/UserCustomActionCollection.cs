using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class UserCustomActionCollection
    {
        #region Add
        public IUserCustomAction Add(AddUserCustomActionOptions options)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IUserCustomAction> AddAsync(AddUserCustomActionOptions options)
        {
            if (null == options)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var newUserCustomAction = CreateNewAndAdd() as UserCustomAction;

            // Since argument is given as options, the mapping to the object should not be done here
            //newUserCustomAction.ClientSideComponentId = options.ClientSideComponentId;
            //newUserCustomAction.ClientSideComponentProperties = options.ClientSideComponentProperties;
            //newUserCustomAction.CommandUIExtension = options.CommandUIExtension;
            //newUserCustomAction.Description = options.Description;
            //newUserCustomAction.HostProperties = options.HostProperties;
            //newUserCustomAction.ImageUrl = options.ImageUrl;
            //newUserCustomAction.Location = options.Location;
            //newUserCustomAction.Name = options.Name;
            //newUserCustomAction.RegistrationId = options.RegistrationId;
            //newUserCustomAction.RegistrationType = options.RegistrationType;
            //newUserCustomAction.ScriptBlock = options.ScriptBlock;
            //newUserCustomAction.ScriptSrc = options.ScriptSrc;
            //newUserCustomAction.Sequence = options.Sequence;
            //newUserCustomAction.Title = options.Title;
            //newUserCustomAction.Url = options.Url;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { UserCustomAction.AddUserCustomActionOptionsAdditionalInformationKey, options }
            };

            return await newUserCustomAction.AddAsync(additionalInfo).ConfigureAwait(false) as UserCustomAction;
        }

        // TODO Implement batch variants
        #endregion
    }
}
