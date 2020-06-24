using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of TranslationNotificationRecipientUsers objects
    /// </summary>
    public interface ITranslationNotificationRecipientUsersCollection : IQueryable<ITranslationNotificationRecipientUsers>, IDataModelCollection<ITranslationNotificationRecipientUsers>
    {
    }
}