using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of TranslationNotificationRecipientUsers Domain Model objects
    /// </summary>
    internal partial class TranslationNotificationRecipientUsersCollection : QueryableDataModelCollection<ITranslationNotificationRecipientUsers>, ITranslationNotificationRecipientUsersCollection
    {
        public TranslationNotificationRecipientUsersCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}