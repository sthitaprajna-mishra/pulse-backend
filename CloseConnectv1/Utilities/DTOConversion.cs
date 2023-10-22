using CloseConnectv1.Models.DTO;
using CloseConnectv1.Models;
using Microsoft.AspNetCore.Identity;
using CloseConnectv1.Data;
using CloseConnectv1.Hubs;
using Microsoft.AspNetCore.SignalR;
using static CloseConnectv1.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace CloseConnectv1.Utilities
{
    public class DTOConversion
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DTOConversion(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<FriendRequestResponseDTO> ConvertToFriendRequestResponseDTOAsync(FriendRequest friendRequest)
        {
            var sender = await _userManager.FindByIdAsync(friendRequest.SenderId);

            return new FriendRequestResponseDTO
            {
                FriendRequestId = friendRequest.FriendRequestId,
                SenderId = friendRequest.SenderId,
                SenderName = sender.Name,
                SenderUserName = sender.UserName,
                SenderDPURL = sender.DisplayPictureURL,
                ReceiverId = friendRequest.ReceiverId,
                IsAccepted = friendRequest.IsAccepted,
                IsDeclined = friendRequest.IsDeclined,
                CreatedAt = friendRequest.CreatedAt
            };
        }

        public SearchUserDTO ConvertToSearchUserDTOAsync(ApplicationUser applicationUser, int relationshipCode)
        {
            var convertedObj = new SearchUserDTO
            {
                Id = applicationUser.Id,
                Name = applicationUser.Name,
                UserName = applicationUser.UserName,
                DisplayPictureURL = applicationUser.DisplayPictureURL,
                EmailConfirmed = applicationUser.EmailConfirmed,
                RelationshipWithLoggedInUser = (SearchUserRelationshipCodes)relationshipCode
            };

            convertedObj.RelationshipWithLoggedInUser = relationshipCode switch
            {
                0 => SearchUserRelationshipCodes.Friends,
                1 => SearchUserRelationshipCodes.SentRequest,
                2 => SearchUserRelationshipCodes.ReceivedRequest,
                3 => SearchUserRelationshipCodes.NotFriends,
                _ => throw new ArgumentOutOfRangeException(nameof(relationshipCode)),
            };

            return convertedObj;
        }
    
        public Draft ConvertDraftCreateDTOToDraft(DraftCreateDTO dto)
        {
            Draft draft = new()
            {
                Content = dto.Content,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                AuthorId = dto.AuthorId,
                CharacterCount = dto.CharacterCount,
            };

            return draft;
        }

        public Post ConvertPostCreateDTOToPost(PostCreateDTO dto)
        {
            Post post = new()
            {
                Content = dto.Content,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                AuthorId = dto.AuthorId,
                CharacterCount = dto.CharacterCount,
                NumberOfComments = 0,
                NumberOfLikes = 0,
                PopularityScore = 0
            };

            return post;
        }
    
        public async Task<PostDisplayDTO> ConvertToPostDisplayDTO(Post model, string userId)
        {
            if(model is null || model.AuthorId is null) return new PostDisplayDTO();

            ApplicationUser author = await _userManager.FindByIdAsync(model.AuthorId);

            bool alreadyLiked = await _context.PostLikes.AnyAsync(post => post.UserId.Equals(userId) && post.PostId == model.PostId);

            PostDisplayDTO postDisplayDTO = new()
            {
                PostId = model.PostId,
                Content = model.Content,
                CreateDate = model.CreateDate.ToLocalTime(),
                UpdateDate = model.UpdateDate.ToLocalTime(),
                IsLikedByCurrentUser = alreadyLiked,
                AuthorId = model.AuthorId,
                AuthorName = author.Name,
                AuthorUserName = author.UserName,
                AuthorDisplayPictureURL = author.DisplayPictureURL,
                CharacterCount = model.CharacterCount,
                NumberOfComments = model.NumberOfComments,
                NumberOfLikes = model.NumberOfLikes,
                PopularityScore = model.PopularityScore
            };

            return postDisplayDTO;
        }

        public Comment ConvertCommentCreateDTOToComment(CommentCreateDTO dto)
        {
            Comment comment = new()
            {
                Text = dto.Text,
                AuthorId = dto.AuthorId,
                PostId = dto.PostId,
                ParentCommentId = dto.ParentCommentId,
                NumberOfComments = 0,
                NumberOfLikes = 0,
                CreateDate = DateTime.UtcNow
            };

            return comment;
        }

        public async Task<CommentDisplayDTO> ConvertToCommentDisplayDTO(Comment model, string currentUserId)
        {
            if (model is null || model.AuthorId is null) return new CommentDisplayDTO();

            ApplicationUser author = await _userManager.FindByIdAsync(model.AuthorId);

            bool alreadyLiked = await _context.CommentLikes.AnyAsync(comment => comment.UserId.Equals(currentUserId) && comment.CommentId == model.CommentId);

            bool hasChildComments = false;

            var result = await _context.Comments.FirstOrDefaultAsync(comment => comment.ParentCommentId == model.CommentId);

            if (result is not null) hasChildComments = true;

            CommentDisplayDTO commentDisplayDTO = new()
            {
                CommentId = model.CommentId,
                Text = model.Text,
                AuthorId = model.AuthorId,
                AuthorName = author.Name,
                AuthorUserName = author.UserName,
                AuthorDisplayPictureURL = author.DisplayPictureURL,
                HasChildComments = hasChildComments,
                IsLikedByCurrentUser = alreadyLiked,
                PostId = model.PostId,
                ParentCommentId = model.ParentCommentId,
                NumberOfComments = model.NumberOfComments,
                NumberOfLikes = model.NumberOfLikes,
                CreateDate = model.CreateDate
            };

            return commentDisplayDTO;
        }
    
        public NotificationDTO ConvertToNotificationDTO(Notification notification, ApplicationUser sender)
        {
            NotificationDTO notificationDTO = new()
            {
                Id = notification.Id,
                SenderId = notification.SenderId,
                SenderName = sender.Name,
                SenderUserName = sender.UserName,
                SenderDPURL = sender.DisplayPictureURL,
                RecipientId = notification.RecipientId,
                Message = notification.Message,
                Timestamp = notification.Timestamp.ToLocalTime(),
                IsRead = notification.IsRead,
                ActionUrl = notification.ActionUrl,
            };

            return notificationDTO;
        }

        public async Task<NotificationDTO> ConvertToNotificationDTOList(Notification notification, string senderId)
        {

            ApplicationUser sender = await _userManager.FindByIdAsync(senderId);

            NotificationDTO notificationDTO = new()
            {
                Id = notification.Id,
                SenderId = notification.SenderId,
                SenderName = sender.Name,
                SenderUserName = sender.UserName,
                SenderDPURL = sender.DisplayPictureURL,
                RecipientId = notification.RecipientId,
                Message = notification.Message,
                Timestamp = notification.Timestamp.ToLocalTime(),
                IsRead = notification.IsRead,
                ActionUrl = notification.ActionUrl,
            };

            return notificationDTO;
        }

        public async Task<FriendDTO> ConvertToFriendDTOAsync(string friendId, int relationshipCode)
        {
            ApplicationUser? friendModel = await _userManager.FindByIdAsync(friendId);
            
            if (friendModel is null) return new FriendDTO();

            FriendDTO friend = new()
            {
                FriendId = friendModel.Id,
                FriendName = friendModel.Name,
                FriendDPURL = friendModel.DisplayPictureURL,
                FriendUserName = friendModel.UserName,
                RelationshipWithLoggedInUser = (SearchUserRelationshipCodes)relationshipCode
            };

            friend.RelationshipWithLoggedInUser = relationshipCode switch
            {
                0 => SearchUserRelationshipCodes.Friends,
                1 => SearchUserRelationshipCodes.SentRequest,
                2 => SearchUserRelationshipCodes.ReceivedRequest,
                3 => SearchUserRelationshipCodes.NotFriends,
                _ => throw new ArgumentOutOfRangeException(nameof(relationshipCode)),
            };

            return friend;
        }
        
        public async Task<ConversationDisplayDTO> ConvertToConversationDisplayDTO(Conversation conversation, string userId)
        {
            string otherParticipant = conversation.ParticipantOneId.Equals(userId)
                ? conversation.ParticipantTwoId : conversation.ParticipantOneId;

            ApplicationUser? otherUser = await _userManager.FindByIdAsync(otherParticipant);

            if (otherUser is null) return new ConversationDisplayDTO();

            return new()
            {
                ConversationId = conversation.ConversationId,
                ParticipantId = otherUser.Id,
                ParticipantName = otherUser.Name,
                ParticipantUserName = otherUser.UserName,
                ParticipantDPURL = otherUser.DisplayPictureURL,
                ConversationPreview = conversation.ConversationPreview,
                ConversationPreviewUserId = conversation.ConversationPreviewUserId,
                IsRead = conversation.IsRead,
                LatestDate = conversation.LatestDate.ToLocalTime()
            };
        }

        public async Task<MessageDisplayDTO> ConvertToMessageDisplayDTO(Message message, string userId)
        {
            string otherParticipant = message.SenderId.Equals(userId)
                ? message.ReceiverId : message.SenderId;

            ApplicationUser? otherUser = await _userManager.FindByIdAsync(otherParticipant);

            if (otherUser is null) return new MessageDisplayDTO();

            return new()
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                MessageText = message.MessageText,
                IsDelete = message.IsDelete,
                CreateDate = message.CreateDate.ToLocalTime(),
                ParticipantId = otherUser.Id,
                ParticipantName = otherUser.Name,
                ParticipantUserName = otherUser.UserName,
                ParticipantDPURL = otherUser.DisplayPictureURL
            };
        }
    }
}
