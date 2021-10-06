using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// 
    /// </summary>
    public static class PipelineExtensions
    {
        public static TModel WithModule<TModel>(this IDataModelLoad<TModel> dataModel, IRequestModule module)
        {
            var context = (dataModel as IDataModelWithContext).PnPContext;

            if (context.RequestModules == null)
            {
                context.RequestModules = new List<IRequestModule>();
            }

            if (context.RequestModules.FirstOrDefault(p=>p.Id == module.Id) == null)
            {
                context.RequestModules.Add(module);
            }

            return (TModel)dataModel;
        }        
    }
}
