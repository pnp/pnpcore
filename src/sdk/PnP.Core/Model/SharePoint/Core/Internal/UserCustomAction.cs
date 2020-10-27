using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// UserCustomAction class, write your custom code here
    /// </summary>
    [SharePointType("SP.UserCustomAction", Target = typeof(Web), Uri = "_api/Web/UserCustomActions('{Id}')", Get = "_api/Web/UserCustomActions", LinqGet = "_api/Web/UserCustomActions")]
    [SharePointType("SP.UserCustomAction", Target = typeof(Site), Uri = "_api/Site/UserCustomActions('{Id}')", Get = "_api/Site/UserCustomActions", LinqGet = "_api/Site/UserCustomActions")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class UserCustomAction : BaseDataModel<IUserCustomAction>, IUserCustomAction
    {
        internal const string AddUserCustomActionOptionsAdditionalInformationKey = "AddOptions";

        #region Construction
        public UserCustomAction()
        {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var addOptions = (AddUserCustomActionOptions)additionalInformation[AddUserCustomActionOptionsAdditionalInformationKey];
                var entity = EntityManager.GetClassInfo(GetType(), this);
                string endpointUrl = entity.SharePointGet;

                ExpandoObject baseAddPayload = new
                {
                    __metadata = new { type = entity.SharePointType }
                }.AsExpando();

                dynamic addParameters = baseAddPayload.MergeWith(addOptions.AsExpando(ignoreNullValues: true));
                string json = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject),
                    new JsonSerializerOptions()
                    {
                        IgnoreNullValues = true
                    });

                return new ApiCall(endpointUrl, ApiType.SPORest, json);
            };
        }
        #endregion

        #region Properties
        public Guid ClientSideComponentId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClientSideComponentProperties { get => GetValue<string>(); set => SetValue(value); }

        public string CommandUIExtension { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public string HostProperties { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Location { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string RegistrationId { get => GetValue<string>(); set => SetValue(value); }

        public UserCustomActionRegistrationType RegistrationType { get => GetValue<UserCustomActionRegistrationType>(); set => SetValue(value); }

        public UserCustomActionScope Scope { get => GetValue<UserCustomActionScope>(); set => SetValue(value); }

        public string ScriptBlock { get => GetValue<string>(); set => SetValue(value); }

        public string ScriptSrc { get => GetValue<string>(); set => SetValue(value); }

        public int Sequence { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionOfUserCustomAction { get => GetValue<string>(); set => SetValue(value); }

        //public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }

        //public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        #endregion
    }
}
