using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core.Tokens
{
    /// <summary>
    /// Resolves the ID of a list in the target context based on the list title
    /// </summary>
    internal class TargetListIdByTitleToken : ITokenDefinition
    {
        public string Name { get => "TargetListIdByTitle"; }

        public string GetValue(PnPContext context, string argument)
        {
            var targetList = context.Web.Lists.GetByTitle(argument, l => l.Id);
            if (targetList != null)
            {
                return targetList.Id.ToString();
            }

            return null;
        }
    }
}
