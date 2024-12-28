using AutoMapper;
using Chillde.Repositories.Interfaces;

namespace Chillde.Services.Common
{
    public class SeedData
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SeedData(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
