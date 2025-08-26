using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using PhoSocial.API.Services;
using PhoSocial.API.Models;
using System;

namespace PhoSocial.API.Hubs
{
    [Authorize]
    public class PostHub : Hub
    {
        private readonly IFeedService _feedService;
        public PostHub(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public async Task LikePost(Guid postId)
        {
            var userId = Context.User?.FindFirst("id")?.Value;
            if (userId == null) return;
            await _feedService.LikePostAsync(postId, Guid.Parse(userId));
            await Clients.All.SendAsync("PostLiked", postId);
        }

        public async Task CommentPost(Guid postId, string content)
        {
            var userId = Context.User?.FindFirst("id")?.Value;
            if (userId == null) return;
            await _feedService.CommentPostAsync(postId, Guid.Parse(userId), content);
            await Clients.All.SendAsync("PostCommented", postId);
        }
    }
}
