using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;

namespace Chillde.Services.Services
{
    public class FAQService : IFAQService
    {
        private readonly IUnitOfWork _unitOfWork;

    public FAQService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
}
