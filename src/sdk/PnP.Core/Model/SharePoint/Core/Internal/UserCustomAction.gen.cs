using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a UserCustomAction object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class UserCustomAction : BaseDataModel<IUserCustomAction>, IUserCustomAction
    {
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

        //public IUserResource DescriptionResource
        //{
        //    get
        //    {
        //        if (!NavigationPropertyInstantiated())
        //        {
        //            var propertyValue = new UserResource
        //            {
        //                PnPContext = this.PnPContext,
        //                Parent = this,
        //            };
        //            SetValue(propertyValue);
        //            InstantiateNavigationProperty();
        //        }
        //        return GetValue<IUserResource>();
        //    }
        //    set
        //    {
        //        InstantiateNavigationProperty();
        //        SetValue(value);                
        //    }
        //}


        //public IUserResource TitleResource
        //{
        //    get
        //    {
        //        if (!NavigationPropertyInstantiated())
        //        {
        //            var propertyValue = new UserResource
        //            {
        //                PnPContext = this.PnPContext,
        //                Parent = this,
        //            };
        //            SetValue(propertyValue);
        //            InstantiateNavigationProperty();
        //        }
        //        return GetValue<IUserResource>();
        //    }
        //    set
        //    {
        //        InstantiateNavigationProperty();
        //        SetValue(value);                
        //    }
        //}

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
