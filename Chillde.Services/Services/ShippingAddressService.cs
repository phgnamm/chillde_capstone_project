using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Chillde.Services.Services
{
    public class ShippingAddressService : IShippingAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IRedisHelper _redisHelper;


        public ShippingAddressService(IUnitOfWork unitOfWork, IClaimService claimService, IMapper mapper,
            IHttpClientFactory httpClientFactory, IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("GhnClient");
            _redisHelper = redisHelper;
        }

        public async Task<ResponseModel> GetDistrictsAsync(int provinceId)
        {
            string cacheKey = $"districts_{provinceId}";

            var result = await _redisHelper.GetOrSetAsync(
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
                    var data = jsonObject?["data"]?.ToObject<List<DistrictModel>>();

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = data
                    };
                },
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(30)
            );

            if (result.Code != StatusCodes.Status200OK)
            {
                await _redisHelper.InvalidateCacheByPatternAsync(cacheKey);
            }

            return result;
        }


        public async Task<ResponseModel> GetProvincesAsync()
        {
            const string cacheKey = "provinces";

            var result = await _redisHelper.GetOrSetAsync(
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
                    var data = jsonObject?["data"]?.ToObject<List<ProvinceModel>>();

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = data
                    };
                },
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(30)
            );

            if (result.Code != StatusCodes.Status200OK)
            {
                await _redisHelper.InvalidateCacheByPatternAsync(cacheKey);
            }

            return result;
        }


        public async Task<ResponseModel> GetWardsAsync(int districtId)
        {
            string cacheKey = $"wards_{districtId}";

            var result = await _redisHelper.GetOrSetAsync(
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
                    var data = jsonObject?["data"]?.ToObject<List<WardModel>>();

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = data
                    };
                },
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(30)
            );

            if (result.Code != StatusCodes.Status200OK)
            {
                await _redisHelper.InvalidateCacheByPatternAsync(cacheKey);
            }

            return result;
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
                */
                await _unitOfWork.SaveChangeAsync();

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

        public async Task<ResponseModel> GetAllAsync(ShippingAddressFilterModel shippingAddressFilterModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (currentUserId == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Data = null
                };
            }

            Expression<Func<ShippingAddress, bool>> filter = address =>
                address.CreatedById == currentUserId && 
                address.IsDeleted == shippingAddressFilterModel.IsDeleted &&
                (string.IsNullOrEmpty(shippingAddressFilterModel.Search) ||
                 address.FullName.Contains(shippingAddressFilterModel.Search) ||
                 address.PhoneNumber.Contains(shippingAddressFilterModel.Search) ||
                 address.ProvinceName!.Contains(shippingAddressFilterModel.Search) ||
                 address.DistrictName!.Contains(shippingAddressFilterModel.Search) ||
                 address.WardName!.Contains(shippingAddressFilterModel.Search));

            var shippingAddresses = await _unitOfWork.ShippingAddressRepository.GetAllAsync(
                filter: filter,
                pageIndex: shippingAddressFilterModel.PageIndex,
                pageSize: shippingAddressFilterModel.PageSize
            );

            var shippingAddressModels = _mapper.Map<List<ShippingAddressModel>>(shippingAddresses.Data);

            var result = new Pagination<ShippingAddressModel>(
                shippingAddressModels,
                shippingAddressFilterModel.PageIndex,
                shippingAddressFilterModel.PageSize,
                shippingAddresses.TotalCount
            );

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get all shipping addresses successfully",
                Data = result
            };
        }


        public async Task<ResponseModel> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid ID",
                    Data = null
                };
            }

            var currentUserId = _claimService.GetCurrentUserId;
            if (currentUserId == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Data = null
                };
            }

            var shippingAddress = await _unitOfWork.ShippingAddressRepository.GetAsync(id);

            if (shippingAddress == null || shippingAddress.IsDeleted || shippingAddress.CreatedById != currentUserId)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Shipping address not found or access denied",
                    Data = null
                };
            }

            var shippingAddressModel = _mapper.Map<ShippingAddressModel>(shippingAddress);

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Shipping address retrieved successfully",
                Data = shippingAddressModel
            };
        }


        public async Task<ResponseModel> UpdateShippingAddressAsync(Guid id, ShippingAddressUpdateModel request)
        {
            if (id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid ID",
                    Data = null
                };
            }

            var shippingAddress = await _unitOfWork.ShippingAddressRepository.GetAsync(id);

            if (shippingAddress == null || shippingAddress.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Shipping address not found",
                    Data = null
                };
            }


            _mapper.Map(request, shippingAddress);

            if (request.IsDefault)
            {
                var otherAddresses = await _unitOfWork.ShippingAddressRepository.GetAllAsync(
                    filter: sa => sa.CreatedById == shippingAddress.CreatedById && sa.Id != id && !sa.IsDeleted
                );

                foreach (var address in otherAddresses.Data)
                {
                    address.IsDefault = false;
                    _unitOfWork.ShippingAddressRepository.Update(address);
                }
            }

            shippingAddress.IsDefault = request.IsDefault;

            _unitOfWork.ShippingAddressRepository.Update(shippingAddress, isOwnerRequired: true);

            var saveResult = await _unitOfWork.SaveChangeAsync();

            if (saveResult <= 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to update the shipping address. Please try again.",
                    Data = null
                };
            }

            var updatedShippingAddress = _mapper.Map<ShippingAddressModel>(shippingAddress);

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Shipping address updated successfully",
                Data = updatedShippingAddress
            };
        }


        public async Task<ResponseModel> Delete(Guid id)
        {
            var shippingAddress = await _unitOfWork.ShippingAddressRepository.GetAsync(id);

            if (shippingAddress == null || shippingAddress.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Shipping address not found or already deleted",
                    Data = null
                };
            }


            _unitOfWork.ShippingAddressRepository.HardRemove(shippingAddress, isOwnerRequired: true);

            var saveResult = await _unitOfWork.SaveChangeAsync();

            if (saveResult <= 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to delete the shipping address. Please try again.",
                    Data = null
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Shipping address deleted successfully",
                Data = null
            };
        }
    }
}