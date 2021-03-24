using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Syntex Content Center site
    /// </summary>
    public interface ISyntexContentCenter
    {
        /// <summary>
        /// Web that represents the Syntex Content Center
        /// </summary>
        public IWeb Web { get; }

        /// <summary>
        /// Gets one or more Syntex models from the Syntex content center
        /// </summary>
        /// <param name="modelName">Name of the model to filter on, leaving empty returns all models</param>
        Task<List<ISyntexModel>> GetSyntexModelsAsync(string modelName = null);

        /// <summary>
        /// Gets one or more Syntex models from the Syntex content center
        /// </summary>
        /// <param name="modelName">Name of the model to filter on, leaving empty returns all models</param>
        List<ISyntexModel> GetSyntexModels(string modelName = null);
    }

}
