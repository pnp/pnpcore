using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A modern SharePoint Page
    /// </summary>
    [ConcreteType(typeof(Page))]
    public interface IPage : IDataModel<IPage>, IListItemBase /*, IDataModelGet<IPage>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel, IQueryableDataModel*/
    {
        /// <summary>
        /// List of sections on this page
        /// </summary>
        List<ICanvasSection> Sections { get; }

        /// <summary>
        /// List of control on this page
        /// </summary>
        List<ICanvasControl> Controls { get; }

        /// <summary>
        /// Loads the page object model
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Translated a given web part id to a <see cref="DefaultWebPart"/> enum. Non default web parts will be returned as <see cref="DefaultWebPart.ThirdParty"/>
        /// </summary>
        /// <param name="id">Web part id to lookup</param>
        /// <returns>Corresponding <see cref="DefaultWebPart"/> enum value</returns>
        DefaultWebPart WebPartIdToDefaultWebPart(string id);

        /// <summary>
        /// Translates a given <see cref="DefaultWebPart"/> enum to it's corresponding web part id. Non default web parts will be returned as empty string
        /// </summary>
        /// <param name="webPart"><see cref="DefaultWebPart"/> enum to translate to it's id</param>
        /// <returns>The corresponding web part id</returns>
        string DefaultWebPartToWebPartId(DefaultWebPart webPart);

        /// <summary>
        /// Gets a list of available client side web parts to use having a given name
        /// </summary>
        /// <param name="name">Name of the web part to retrieve</param>
        /// <returns>List of available <see cref="IClientSideComponent"/></returns>
        Task<IEnumerable<IClientSideComponent>> AvailableClientSideComponentsAsync(string name);

        /// <summary>
        /// Gets a list of available client side web parts to use having a given name
        /// </summary>
        /// <param name="name">Name of the web part to retrieve</param>
        /// <returns>List of available <see cref="IClientSideComponent"/></returns>
        IEnumerable<IClientSideComponent> AvailableClientSideComponents(string name);
    }
}
