using System;
using System.Collections.Generic;
using System.Dynamic;

namespace PnP.Core.Model.Security
{
    internal static class MailHandler
    {
        internal static void CheckErrors(MailOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Message == null)
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.Message)}");
            }

            if (string.IsNullOrEmpty(options.Message.Body))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.Message)}.{nameof(options.Message.Body)}");
            }

            if (options.Message.ToRecipients == null || options.Message.ToRecipients.Count == 0)
            {
                throw new ArgumentException(PnPCoreResources.Exception_MailHandler_NoRecipient);
            }
        }

        internal static dynamic GetMailBody(MailOptions options)
        {
            dynamic body = new ExpandoObject();

            body.saveToSentItems = options.SaveToSentItems;

            dynamic message = new ExpandoObject();

            dynamic messageBody = new ExpandoObject();

            message.subject = options.Message.Subject;
            message.importance = options.Message.Importance.ToString();

            messageBody.content = options.Message.Body;
            messageBody.contentType = options.Message.BodyContentType.ToString();

            message.body = messageBody;

            message.toRecipients = GetRecipients(options.Message.ToRecipients);
            
            if (options.Message.CcRecipients != null && options.Message.CcRecipients.Count > 0)
            {
                message.ccRecipients = GetRecipients(options.Message.CcRecipients);
            }

            if (options.Message.BccRecipients != null && options.Message.BccRecipients.Count > 0)
            {
                message.bccRecipients = GetRecipients(options.Message.BccRecipients);
            }

            if (options.Message.ReplyTo != null && options.Message.ReplyTo.Count > 0)
            {
                message.replyTo = GetRecipients(options.Message.ReplyTo);
            }

            if (options.Message.Attachments != null && options.Message.Attachments.Count > 0)
            {
                var attachmentsList = new List<dynamic>();

                foreach (var attachment in options.Message.Attachments)
                {
                    dynamic attachmentDynamic = new ExpandoObject();

                    ((IDictionary<string, object>)attachmentDynamic).Add(PnPConstants.MetaDataGraphType, "#microsoft.graph.fileAttachment");
                    attachmentDynamic.name = attachment.Name;
                    attachmentDynamic.contentType = attachment.ContentType;
                    attachmentDynamic.contentBytes = attachment.ContentBytes;

                    attachmentsList.Add(attachmentDynamic);
                }

                message.attachments = attachmentsList;
            }

            body.message = message;

            return body;
            }

        private static dynamic GetRecipients(List<RecipientOptions> recipients)
        {
            var recipientsList = new List<dynamic>();

            foreach (var toRecipient in recipients)
            {
                dynamic recipientDynamic = new ExpandoObject();

                dynamic addressDynamic = new ExpandoObject();
                addressDynamic.address = toRecipient.EmailAddress;

                recipientDynamic.emailAddress = addressDynamic;

                recipientsList.Add(recipientDynamic);
            }
            return recipientsList;
        }
    }
}
