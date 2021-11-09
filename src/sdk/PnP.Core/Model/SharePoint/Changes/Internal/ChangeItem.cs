using System;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ChangeItem")]
    internal sealed class ChangeItem : Change, IChangeItem
    {
        public ChangeActivityType ActivityType { get => GetValue<ChangeActivityType>(); set => SetValue(value); }
        public IContentType ContentTypeId { get => GetValue<IContentType>(); set => SetValue(value); }
        public string Editor { get => GetValue<string>(); set => SetValue(value); }
        public string EditorEmailHint { get => GetValue<string>(); set => SetValue(value); }
        public string EditorLoginName { get => GetValue<string>(); set => SetValue(value); }
        public FileSystemObjectType FileSystemObjectType { get => GetValue<FileSystemObjectType>(); set => SetValue(value); }
        public string FileType { get => GetValue<string>(); set => SetValue(value); }
        public string Hashtag { get => GetValue<string>(); set => SetValue(value); }
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }
        public int ItemId { get => GetValue<int>(); set => SetValue(value); }
        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }
        public string ListTitle { get => GetValue<string>(); set => SetValue(value); }
        public bool MoveWasForRecycle { get => GetValue<bool>(); set => SetValue(value); }
        public bool MoveWasForRestore { get => GetValue<bool>(); set => SetValue(value); }
        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }
        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }
    }
}