using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class ShippingAddressService : IShippingAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IMapper _mapper;

        public ShippingAddressService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
        }
    }
}
