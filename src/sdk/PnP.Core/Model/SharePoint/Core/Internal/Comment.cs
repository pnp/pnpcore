using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// comment class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Comments.comment", Uri = "_api/web/lists(guid'{List.Id}')/getitembyid({Parent.Id})/comments('{Id}')", LinqGet = "_api/web/lists(guid'{List.Id}')/getitembyid({Parent.Id})/comments")]
    internal sealed class Comment : BaseDataModel<IComment>, IComment
    {
        #region Construction
        public Comment()
        {
            // Handler to construct the Add request for this list
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var entity = EntityManager.GetClassInfo(GetType(), this);

                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    text = Text
                }.AsExpando();
                string body = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject));

                if (Parent != null && Parent.GetType() == typeof(CommentCollection) && Parent.Parent != null && !(Parent.Parent.GetType() == typeof(Comment)))
                {
                    // We're adding a root level comment
                    return new ApiCall(entity.SharePointLinqGet, ApiType.SPORest, body);
                }
                else
                {
                    // We're adding a reply to an existing comment
                    return new ApiCall($"{BuildBaseApiRequestForReply(entity)}/replies", ApiType.SPORest, body);
                }
            };
        }
        #endregion

        #region Properties
        public DateTime CreatedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public bool IsLikedByUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsReply { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemId { get => GetValue<int>(); set => SetValue(value); }

        public int LikeCount { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ParentId { get => GetValue<string>(); set => SetValue(value); }

        public string RelativeCreatedDate { get => GetValue<string>(); set => SetValue(value); }

        public int ReplyCount { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("text")]
        public string Text { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointSharingPrincipal Author { get => GetModelValue<ISharePointSharingPrincipal>(); }

        public ICommentLikeUserEntityCollection LikedBy { get => GetModelCollectionValue<ICommentLikeUserEntityCollection>(); }

        public ICommentCollection Replies { get => GetModelCollectionValue<ICommentCollection>(); }

        public ICommentLikeUserEntityCollection Mentions { get => GetModelCollectionValue<ICommentLikeUserEntityCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Extension methods

        #region Like/Unlike

        public async Task LikeAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            if (Parent != null && Parent.GetType() == typeof(CommentCollection) && Parent.Parent != null && !(Parent.Parent.GetType() == typeof(Comment)))
            {
                // We're liking a root level comment
                await RequestAsync(new ApiCall($"{entity.SharePointUri}/like", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                // We're liking a reply
                await RequestAsync(new ApiCall($"{BuildBaseApiRequestForLikeUnLike(entity)}/like", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void Like()
        {
            LikeAsync().GetAwaiter().GetResult();
        }

        public async Task UnlikeAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            if (Parent != null && Parent.GetType() == typeof(CommentCollection) && Parent.Parent != null && !(Parent.Parent.GetType() == typeof(Comment)))
            {
                // We're liking a root level comment
                await RequestAsync(new ApiCall($"{entity.SharePointUri}/unlike", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                // We're liking a reply
                await RequestAsync(new ApiCall($"{BuildBaseApiRequestForLikeUnLike(entity)}/unlike", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void Unlike()
        {
            UnlikeAsync().GetAwaiter().GetResult();
        }
        #endregion

        private string BuildBaseApiRequestForReply(EntityInfo entity)
        {
            Comment parentComment = Parent.Parent as Comment;
            string apiRequest = entity.SharePointUri;
            apiRequest = apiRequest.Replace("{Parent.Id}", parentComment.ItemId.ToString()).Replace("{Id}", parentComment.Id);
            return apiRequest;
        }

        private string BuildBaseApiRequestForLikeUnLike(EntityInfo entity)
        {
            Comment parentComment = Parent.Parent as Comment;
            string apiRequest = entity.SharePointUri;
            apiRequest = apiRequest.Replace("{Parent.Id}", parentComment.ItemId.ToString()).Replace("{Id}", Id);
            return apiRequest;
        }
        #endregion
    }
}
