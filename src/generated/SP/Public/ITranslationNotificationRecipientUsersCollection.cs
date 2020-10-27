using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of TranslationNotificationRecipientUsers objects
    /// </summary>
    [ConcreteType(typeof(TranslationNotificationRecipientUsersCollection))]
    public interface ITranslationNotificationRecipientUsersCollection : IQueryable<ITranslationNotificationRecipientUsers>, IDataModelCollection<ITranslationNotificationRecipientUsers>
    {
    }
}