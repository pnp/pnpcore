namespace PnP.Core.Model.SharePoint
{
    internal class SyntexModelPublicationResult: ISyntexModelPublicationResult
    {
        public string ErrorMessage { get; internal set; }

        public ISyntexModelPublication Publication { get; internal set; }

        public int StatusCode { get; internal set; }

        public bool Succeeded
        {
            get
            {
                if (StatusCode >= 200 && StatusCode < 300)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
