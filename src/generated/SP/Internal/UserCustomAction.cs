using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// UserCustomAction class, write your custom code here
    /// </summary>
    [SharePointType("SP.UserCustomAction", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class UserCustomAction : BaseDataModel<IUserCustomAction>, IUserCustomAction
    {
        #region Construction
        public UserCustomAction()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

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

        public int RegistrationType { get => GetValue<int>(); set => SetValue(value); }

        public int Scope { get => GetValue<int>(); set => SetValue(value); }

        public string ScriptBlock { get => GetValue<string>(); set => SetValue(value); }

        public string ScriptSrc { get => GetValue<string>(); set => SetValue(value); }

        public int Sequence { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionOfUserCustomAction { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #region New properties

        public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }


        public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
