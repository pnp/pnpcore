using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    class IteratorIdProvider : IIdProvider
    {
        protected int Id { get; private set; }
        public int GetActionId()
        {
            Id++;
            return Id;
        }
    }
}
