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
    [SharePointType("SP.UserCustomAction", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/UserCustomActions('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/UserCustomActions", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/UserCustomActions")]
    internal sealed class UserCustomAction : BaseDataModel<IUserCustomAction>, IUserCustomAction
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

                // Skip the basemodel from merging initially as the it will render wrong JSON
                dynamic addParameters = baseAddPayload.MergeWith(addOptions.AsExpando(ignoreProperties: new[] { "Rights" }, ignoreNullValues: true));

                // Maps the BasePermissions model directly on the payload

                if (addOptions.Rights != null)
                {
                    var rightsEntity = EntityManager.GetClassInfo(addOptions.Rights.GetType(), this);

                    ExpandoObject rightsPayload = new
                    {
                        __metadata = new { type = rightsEntity.SharePointType },
                        Low = addOptions.Rights.Low.ToString(),
                        High = addOptions.Rights.High.ToString()
                    }.AsExpando();

                    addParameters.Rights = rightsPayload;
                }

                string json = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);


                return new ApiCall(endpointUrl, ApiType.SPORest, json);
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            UpdateApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
#pragma warning restore CS1998
                var rightsEntity = EntityManager.GetClassInfo(Rights.GetType(), this);

                ExpandoObject rightsPayload = new
                {
                    __metadata = new { type = rightsEntity.SharePointType },
                    Low = Rights.Low.ToString(),
                    High = Rights.High.ToString()
                }.AsExpando();

                var updateProps = new
                {
                    __metadata = new
                    {
                        type = "SP.UserCustomAction"
                    },
                    Rights = rightsPayload,
                    ClientSideComponentId,
                    ClientSideComponentProperties,
                    CommandUIExtension,
                    Description,
                    Group,
                    HostProperties,
                    ImageUrl,
                    Location,
                    Name,
                    RegistrationId,
                    RegistrationType,
                    ScriptBlock,
                    ScriptSrc,
                    Sequence,
                    Title,
                    Url
                };

                var jsonBody = JsonSerializer.Serialize(updateProps, PnPConstants.JsonSerializer_IgnoreNullValues);
                return new ApiCallRequest(new ApiCall(apiCallRequest.ApiCall.Request, apiCallRequest.ApiCall.Type, jsonBody));

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

        public IBasePermissions Rights { get => GetModelValue<IBasePermissions>(); set => SetModelValue(value); }

        //public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }

        //public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion
    }
}
