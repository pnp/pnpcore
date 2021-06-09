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
#pragma warning disable CA2243 // Attribute string literals should parse correctly
    [SharePointType("Microsoft.SharePoint.Comments.comment", Uri = "_api/web/lists(guid'{List.Id}')/getitembyid({Parent.Id})/comments('{Id}')", LinqGet = "_api/web/lists(guid'{List.Id}')/getitembyid({Parent.Id})/comments")]
#pragma warning restore CA2243 // Attribute string literals should parse correctly
    internal partial class Comment : BaseDataModel<IComment>, IComment
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
                return new ApiCall(entity.SharePointLinqGet, ApiType.SPORest, body);
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

        public ICommentUserEntityCollection LikedBy { get => GetModelCollectionValue<ICommentUserEntityCollection>(); }

        public ICommentCollection Replies { get => GetModelCollectionValue<ICommentCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }

        #endregion

        #region Extension methods

        #region Like/Unlike

        public async Task LikeAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            await RequestAsync(new ApiCall($"{entity.SharePointUri}/like", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
        }

        public void Like()
        {
            LikeAsync().GetAwaiter().GetResult();
        }

        public async Task UnlikeAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            await RequestAsync(new ApiCall($"{entity.SharePointUri}/unlike", apiType: ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
        }

        public void Unlike()
        {
            UnlikeAsync().GetAwaiter().GetResult();
        }
        #endregion

        #endregion
    }
}
