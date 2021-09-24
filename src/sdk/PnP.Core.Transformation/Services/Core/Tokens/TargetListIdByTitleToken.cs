using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core.Tokens
{
    /// <summary>
    /// Resolves the ID of a list in the target context based on the list title
    /// </summary>
    internal class TargetListIdByTitleToken : TargetTokenDefinition
    {
        public TargetListIdByTitleToken(PnPContext targetContext): base(targetContext)
        {
            this.Name = "TargetListIdByTitle";
        }

        public override string GetValue(string argument)
        {
            var targetList = this.TargetContext.Web.Lists.GetByTitle(argument, l => l.Id);
            if (targetList != null)
            {
                return targetList.Id.ToString();
            }

            return null;
        }
    }
}
