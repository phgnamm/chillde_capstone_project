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

            /*  var response = await _httpClient.GetAsync($"master-data/district?province_id={provinceId}");

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
              };*/
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
                TimeSpan.FromDays(30) // Cache districts for 30 days
            );


        }

        public async Task<ResponseModel> GetProvincesAsync(ProvinceFilterModel provinceFilterModel)
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

            if (data == null || !data.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "No provinces found",
                    Data = null
                };
            }

            if (!string.IsNullOrWhiteSpace(provinceFilterModel.Search))
            {
                data = data
                    .Where(p => p.ProvinceName != null &&
                                p.ProvinceName.Contains(provinceFilterModel.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(provinceFilterModel.Order))
            {
                if (provinceFilterModel.OrderByDescending)
                {
                    data = data.OrderByDescending(p => p.GetType().GetProperty(provinceFilterModel.Order)?.GetValue(p)).ToList();
                }
                else
                {
                    data = data.OrderBy(p => p.GetType().GetProperty(provinceFilterModel.Order)?.GetValue(p)).ToList();
                }
            }

            var totalItems = data.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)provinceFilterModel.PageSize);
            var pagedData = data
            .Skip((provinceFilterModel.PageIndex - 1) * provinceFilterModel.PageSize)
            .Take(provinceFilterModel.PageSize)
                .ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Success",
                Data = new
                {
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = provinceFilterModel.PageIndex,
                    PageSize = provinceFilterModel.PageSize,
                    Data = pagedData
                }
            };
        }

        public async Task<ResponseModel> GetWardsAsync(int districtId)
        {
            /*if (districtId <= 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid districtId",
                    Data = null
                };
            }

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
            };*/
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
                TimeSpan.FromDays(30) // Cache wards for 30 days
            );
        }
        public async Task<ResponseModel> PostShippingAddressAsync(ShippingAddressAddModel request)
        {
            // Validate input
            if (request.ProvinceId <= 0 || request.DistrictId <= 0 || string.IsNullOrEmpty(request.WardCode))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input data",
                    Data = null
                };
            }

            // Get Province Name
            var provincesResponse = await GetProvincesAsync(new ProvinceFilterModel());
            var province = ((List<ProvinceModel>)provincesResponse.Data)?.FirstOrDefault(p => p.ProvinceID == request.ProvinceId);
            if (province == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Province not found",
                    Data = null
                };
            }

            // Get District Name
            var districtsResponse = await GetDistrictsAsync(request.ProvinceId);
            var district = ((List<DistrictModel>)districtsResponse.Data)?.FirstOrDefault(d => d.DistrictID == request.DistrictId);
            if (district == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "District not found",
                    Data = null
                };
            }

            // Get Ward Name
            var wardsResponse = await GetWardsAsync(request.DistrictId);
            var ward = ((List<WardModel>)wardsResponse.Data)?.FirstOrDefault(w => w.WardCode == request.WardCode);
            if (ward == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Ward not found",
                    Data = null
                };
            }

            // Create Shipping Address
            var shippingAddress = new ShippingAddress
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                WardCode = request.WardCode,
                WardName = ward.WardName,
                DistrictId = request.DistrictId,
                DistrictName = district.DistrictName,
                ProvinceId = request.ProvinceId,
                ProvinceName = province.ProvinceName,
                IsDefault = request.IsDefault,
                CreatedBy = request.CreatedBy
            };

            // Save shipping address logic here (e.g., save to database)

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Shipping address created successfully",
                Data = shippingAddress
            };
        }

    }
}


