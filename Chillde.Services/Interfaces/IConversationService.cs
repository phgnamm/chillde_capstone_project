using Chillde.Services.Models.ConversationModels;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces;

public interface IConversationService
{
    Task<ResponseModel> Add(ConversationAddModel conversationAddModel);
    Task<ResponseModel> Get(Guid id);
    Task<ResponseModel> GetAll(ConversationFilterModel conversationFilterModel);
    Task<ResponseModel> Archive(Guid id);
    Task<ResponseModel> Delete(Guid id);
    Task<ResponseModel> AddMessage(Guid conversationId, MessageAddModel messageAddModel);
    Task<ResponseModel> GetAllMessages(Guid conversationId, MessageFilterModel messageFilterModel);
    Task<ResponseModel> ReadMessages(Guid conversationId);
}