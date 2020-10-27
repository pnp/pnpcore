using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItem", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ListItem : BaseDataModel<IListItem>, IListItem
    {
        #region Construction
        public ListItem()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public bool CommentsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #region New properties

        public int CommentsDisabledScope { get => GetValue<int>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public int FileSystemObjectType { get => GetValue<int>(); set => SetValue(value); }

        public string IconOverlay { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRedirectedEmbedUri { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRedirectedEmbedUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Client_Title { get => GetValue<string>(); set => SetValue(value); }

        public IAttachmentCollection AttachmentFiles { get => GetModelCollectionValue<IAttachmentCollection>(); }


        public IContentType ContentType { get => GetModelValue<IContentType>(); }


        public IDlpPolicyTip GetDlpPolicyTip { get => GetModelValue<IDlpPolicyTip>(); }


        public IFieldStringValues FieldValuesAsHtml { get => GetModelValue<IFieldStringValues>(); }


        public IFieldStringValues FieldValuesAsText { get => GetModelValue<IFieldStringValues>(); }


        public IFieldStringValues FieldValuesForEdit { get => GetModelValue<IFieldStringValues>(); }


        public IFile File { get => GetModelValue<IFile>(); }


        public IFolder Folder { get => GetModelValue<IFolder>(); }


        public IlikedByInformation LikedByInformation { get => GetModelValue<IlikedByInformation>(); }


        public IList ParentList { get => GetModelValue<IList>(); }


        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }


        public IListItemVersionCollection Versions { get => GetModelCollectionValue<IListItemVersionCollection>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
