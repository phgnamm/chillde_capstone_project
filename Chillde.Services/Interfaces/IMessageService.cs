using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces;

public interface IMessageService
{
    Task<ResponseModel> Delete(Guid id, MessageDeleteModel messageDeleteModel);
}