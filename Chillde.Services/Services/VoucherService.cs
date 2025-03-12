using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;

namespace Chillde.Services.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IClaimService _claimService;
        private readonly IUnitOfWork _unitOfWork;

        public VoucherService(IClaimService claimService, IUnitOfWork unitOfWork)
        {
            _claimService = claimService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Add(VoucherAddModel voucherAddModel)
        {
            throw new NotImplementedException();
        }
    }
}
