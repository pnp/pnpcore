using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Principal object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Principal : BaseDataModel<IPrincipal>, IPrincipal
    {

        #region New properties

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int PrincipalType { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}
