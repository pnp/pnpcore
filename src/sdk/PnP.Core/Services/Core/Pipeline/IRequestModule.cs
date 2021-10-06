using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services
{
    public interface IRequestModule
    {
        public Guid Id { get; }

        public bool ExecuteForSpoRest { get; }

        public bool ExecuteForMicrosoftGraph { get; }

        public bool ExecuteForSpoCsom { get; }

        public Action<Dictionary<string, string>> RequestHeaderHandler { get; set; } 

    }
}
