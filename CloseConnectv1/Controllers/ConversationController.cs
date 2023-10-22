using CloseConnectv1.Data;
using CloseConnectv1.Filters;
using CloseConnectv1.Hubs;
using CloseConnectv1.Models;
using CloseConnectv1.Models.DTO;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class ConversationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ConversationHub> _conversationHubContext;
        private readonly DTOConversion _conversion;

        public ConversationController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IHubContext<ConversationHub> conversationHubContext,
            DTOConversion conversion
            )
        {
            _userManager = userManager;
            _context = context;
            _conversationHubContext = conversationHubContext;
            _conversion = conversion;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDTO payload)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (payload is null) return BadRequest();

                Conversation? conversation;

                // if this is first message sent by sender to receiver
                if (!payload.ConversationId.HasValue && payload.IsInitialMessage == 'Y')
                {
                    // create Conversation 
                    conversation = new()
                    {
                        ParticipantOneId = payload.SenderId,
                        ParticipantTwoId = payload.ReceiverId,
                        ConversationPreview = payload.MessageText[..50] + "..."
                    };

                    await _context.Conversations.AddAsync(conversation);
                    await _context.SaveChangesAsync();
                }

                // if conversation already exists
                else if (payload.ConversationId.HasValue && payload.IsInitialMessage == 'N')
                {
                    conversation = await _context.Conversations
                        .FirstOrDefaultAsync(c => c.ConversationId == payload.ConversationId);

                    if (conversation is null) return NotFound("No such conversation exists");
                }

                // wrong combination requested
                else
                {
                    return BadRequest();
                }

                // create Message with corresponding ConversationId
                Message message = new()
                {
                    ConversationId = conversation.ConversationId,
                    SenderId = payload.SenderId,
                    ReceiverId = payload.ReceiverId,
                    MessageText = payload.MessageText,
                    CreateDate = DateTime.UtcNow
                };

                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();

                // if it is existing conversation, update preview text
                if (payload.IsInitialMessage == 'N')
                {
                    if (payload.MessageText.Length <= 50)
                    {
                        conversation.ConversationPreview = payload.MessageText;
                    }
                    else
                    {
                        conversation.ConversationPreview = payload.MessageText[..50] + "...";
                    }
                }
                // update latest date
                conversation.LatestDate = message.CreateDate;
                conversation.ConversationPreviewUserId = message.SenderId;
                conversation.IsRead = false;

                await _context.SaveChangesAsync();

                ConversationDisplayDTO conversationDisplayForSender = await _conversion.ConvertToConversationDisplayDTO(conversation, payload.SenderId);
                ConversationDisplayDTO conversationDisplayForReceiver = await _conversion.ConvertToConversationDisplayDTO(conversation, payload.ReceiverId);
                ConversationDisplayDTO conversationDisplayForReceiverHome = await _conversion.ConvertToConversationDisplayDTO(conversation, payload.ReceiverId);

                conversationDisplayForSender.IsRead = true;
                conversationDisplayForReceiver.IsRead = true;


                var resultToSender = new Dictionary<string, object>
                {
                    ["message"] = message,
                    ["conversation"] = conversationDisplayForSender
                };

                var resultToReceiver = new Dictionary<string, object>
                {
                    ["message"] = message,
                    ["conversation"] = conversationDisplayForReceiver
                };

                var resultToReceiverHome = new Dictionary<string, object>
                {
                    ["message"] = message,
                    ["conversation"] = conversationDisplayForReceiverHome
                };

                //Broadcast the notification to the sender
                try
                {
                    await _conversationHubContext.Clients
                        .Client(HomeConversationHub._clientConnections[payload.ReceiverId]).SendAsync("ReceiveHomeMessage", resultToReceiverHome);

                    await _conversationHubContext.Clients
                        .Client(ConversationHub._clientConnections[payload.ReceiverId]).SendAsync("ReceiveMessage", resultToReceiver);

                }
                catch (Exception ex)
                {
                    // user is not online but handle issue gracefully
                    if (ex.Message.Equals($"The given key '{payload.ReceiverId}' was not present in the dictionary."))
                    {
                        return Ok(resultToSender);
                    }
                }

                return Ok(resultToSender);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.ToString());   
            }
        }

        [HttpGet("GetAllConversationsForCurrentUser/{userId}")]
        public async Task<IActionResult> GetAllConversationsForCurrentUser(string userId)
        {
            try
            {
                if (userId is null) return BadRequest();

                List<Conversation> conversationsFromDb = await _context.Conversations
                    .Where(c => c.ParticipantOneId.Equals(userId) || c.ParticipantTwoId.Equals(userId)).ToListAsync();

                List<ConversationDisplayDTO> displayConversations = conversationsFromDb
                    .Select(async conversation =>
                    {
                        return await _conversion.ConvertToConversationDisplayDTO(conversation, userId);
                    })
                    .Select(c => c.Result)
                    .ToList();

                return Ok(displayConversations);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("GetAllMessagesForConversation/{conversationId}/{userId}")]
        public async Task<IActionResult> GetAllMessagesForConversation(int conversationId, string userId)
        {
            try
            {
                Conversation? conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if(conversation is null) return BadRequest();

                // mark conversation as read only if it was not read yet and last message was by another user
                if(conversation.IsRead is false && !conversation.ConversationPreviewUserId.Equals(userId))
                {
                    conversation.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                List<Message> messages = await _context.Messages.Where(m => m.ConversationId == conversationId).ToListAsync();

                List<MessageDisplayDTO> displayMessages = messages
                    .Select(async message =>
                    {
                        return await _conversion.ConvertToMessageDisplayDTO(message, userId);
                    })
                    .Select(c => c.Result)
                    .ToList();

                return Ok(displayMessages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}
