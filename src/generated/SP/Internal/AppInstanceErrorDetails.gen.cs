using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a AppInstanceErrorDetails object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class AppInstanceErrorDetails : BaseDataModel<IAppInstanceErrorDetails>, IAppInstanceErrorDetails
    {

        #region New properties

        public Guid CorrelationId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ErrorDetail { get => GetValue<string>(); set => SetValue(value); }

        public int ErrorType { get => GetValue<int>(); set => SetValue(value); }

        public string ErrorTypeName { get => GetValue<string>(); set => SetValue(value); }

        public string ExceptionMessage { get => GetValue<string>(); set => SetValue(value); }

        public int Source { get => GetValue<int>(); set => SetValue(value); }

        public string SourceName { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
