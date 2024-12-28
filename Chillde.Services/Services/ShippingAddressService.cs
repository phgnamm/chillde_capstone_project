using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        private readonly IRedisHelper _redisHelper;


        public ShippingAddressService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper, IHttpClientFactory httpClientFactory, IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("GhnClient");
            _redisHelper = redisHelper;

        }

        public async Task<ResponseModel> GetDistrictsAsync(int provinceId)
        {
            string cacheKey = $"districts_{provinceId}";
            return await _redisHelper.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _httpClient.GetAsync($"master-data/district?province_id={provinceId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        return new ResponseModel
                        {
                            Code = (int)response.StatusCode,
                            Message = "Failed to fetch districts from GHN",
                            Data = null
                        };
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(content);
                    var data = jsonObject["data"]?.ToObject<List<DistrictModel>>();

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = data
                    };
                },
                TimeSpan.FromDays(30)
            );


        }

        public async Task<ResponseModel> GetProvincesAsync()
        {
            const string cacheKey = "provinces";
            return await _redisHelper.GetOrSetAsync(
                cacheKey,
                async () =>
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
                },
                TimeSpan.FromDays(30)
            );
        }

        public async Task<ResponseModel> GetWardsAsync(int districtId)
        {
            string cacheKey = $"wards_{districtId}";
            return await _redisHelper.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _httpClient.GetAsync($"master-data/ward?district_id={districtId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        return new ResponseModel
                        {
                            Code = (int)response.StatusCode,
                            Message = "Failed to fetch wards from API",
                            Data = null
                        };
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(content);
                    var data = jsonObject["data"]?.ToObject<List<WardModel>>();

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = data
                    };
                },
                TimeSpan.FromDays(30)
            );
        }
        public async Task<ResponseModel> AddShippingAddressAsync(ShippingAddressAddModel request)
        {
            if (request.ProvinceId <= 0 || request.DistrictId <= 0 || string.IsNullOrEmpty(request.WardCode))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input data",
                    Data = null
                };
            }

            try
            {
                var province = await GetProvinceByIdAsync(request.ProvinceId);
                if (province == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Province not found",
                        Data = null
                    };
                }

                var district = await GetDistrictByIdAsync(request.ProvinceId, request.DistrictId);
                if (district == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "District not found",
                        Data = null
                    };
                }

                var ward = await GetWardByCodeAsync(request.DistrictId, request.WardCode);
                if (ward == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Ward not found",
                        Data = null
                    };
                }

                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                var shippingAddress = _mapper.Map<ShippingAddress>(request);
                shippingAddress.ProvinceName = province.ProvinceName;
                shippingAddress.DistrictName = district.DistrictName;
                shippingAddress.WardName = ward.WardName;
                shippingAddress.IsDefault = false;
                await _unitOfWork.ShippingAddressRepository.AddAsync(shippingAddress);
/*                shippingAddress.CreatedById = Guid.Parse("01940b23-5d7f-75fb-856d-3a6d99bc013e");
*/              await _unitOfWork.SaveChangeAsync();

                var responseModel = _mapper.Map<ShippingAddressModel>(shippingAddress);          
                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Shipping address created successfully",
                    Data = responseModel
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }
        private async Task<ProvinceModel?> GetProvinceByIdAsync(int provinceId)
        {
            var provincesResponse = await GetProvincesAsync();
            if (provincesResponse.Data is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var provinces = JsonConvert.DeserializeObject<List<ProvinceModel>>(jsonElement.ToString());
                return provinces?.FirstOrDefault(p => p.ProvinceID == provinceId);
            }

            return null;
        }

        private async Task<DistrictModel?> GetDistrictByIdAsync(int provinceId, int districtId)
        {
            var districtsResponse = await GetDistrictsAsync(provinceId);
            if (districtsResponse.Data is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var districts = JsonConvert.DeserializeObject<List<DistrictModel>>(jsonElement.ToString());
                return districts?.FirstOrDefault(d => d.DistrictID == districtId);
            }

            return null;
        }

        private async Task<WardModel?> GetWardByCodeAsync(int districtId, string wardCode)
        {
            var wardsResponse = await GetWardsAsync(districtId);
            if (wardsResponse.Data is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var wards = JsonConvert.DeserializeObject<List<WardModel>>(jsonElement.ToString());
                return wards?.FirstOrDefault(w => w.WardCode == wardCode);
            }

            return null;
        }

    }
}


