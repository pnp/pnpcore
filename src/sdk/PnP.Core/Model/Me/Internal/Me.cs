using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Me
{
    [GraphType(Uri = "me", LinqGet = "me")]
    internal sealed class Me : BaseDataModel<IMe>, IMe
    {
        #region Construction

        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public List<string> BusinessPhones { get => GetValue<List<string>>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string GivenName { get => GetValue<string>(); set => SetValue(value); }

        public string JobTitle { get => GetValue<string>(); set => SetValue(value); }

        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public string MobilePhone { get => GetValue<string>(); set => SetValue(value); }

        public string OfficeLocation { get => GetValue<string>(); set => SetValue(value); }

        public string PreferredLanguage { get => GetValue<string>(); set => SetValue(value); }

        public string SurName { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [GraphProperty("chats", Get = "me/chats")]
        public IChatCollection Chats { get => GetModelCollectionValue<IChatCollection>(); }

        #endregion

        #region

        public async Task SendMailAsync(MailOptions mailOptions)
        {
            MailHandler.CheckErrors(mailOptions);

            var body = MailHandler.GetMailBody(mailOptions);

            var apiCall = new ApiCall($"me/sendMail", ApiType.Graph, JsonSerializer.Serialize(body, PnPConstants.JsonSerializer_IgnoreNullValues));

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SendMail(MailOptions mailOptions)
        {
            SendMailAsync(mailOptions).GetAwaiter().GetResult();
        }

        #endregion
    }
}
