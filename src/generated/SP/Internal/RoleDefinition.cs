using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RoleDefinition class, write your custom code here
    /// </summary>
    [SharePointType("SP.RoleDefinition", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RoleDefinition : BaseDataModel<IRoleDefinition>, IRoleDefinition
    {
        #region Construction
        public RoleDefinition()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public int Order { get => GetValue<int>(); set => SetValue(value); }

        public int RoleTypeKind { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
