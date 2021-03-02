using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Syntex Content Center site
    /// </summary>
    [ConcreteType(typeof(SyntexContentCenter))]
    public interface ISyntexContentCenter : IWeb
    {
        /// <summary>
        /// Gets one or more Syntex models from the Syntex content center
        /// </summary>
        /// <param name="modelName">Name of the model to filter on, leaving empty returns all models</param>
        Task GetSyntexModelsAsync(string modelName = null);
    }

}
