using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services
{
    public abstract class RequestModuleBase : IRequestModule
    {
        public virtual Guid Id => throw new NotImplementedException(); 

        public virtual bool ExecuteForSpoRest => true;

        public virtual bool ExecuteForMicrosoftGraph => true;

        public virtual bool ExecuteForSpoCsom => false;

        public virtual Action<Dictionary<string, string>> RequestHeaderHandler { get; set; } = null;
    }
}
