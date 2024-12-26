using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly HttpClient _httpClient;

        public ShippingAddressService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("GhnClient");
        }

        public async Task<ResponseModel> GetProvincesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("master-data/province");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = "Failed to fetch provinces from GHN",
                        Data = null
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

                var data = jsonObject["data"]?.ToObject<List<ProvinceModel>>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

    }
}
