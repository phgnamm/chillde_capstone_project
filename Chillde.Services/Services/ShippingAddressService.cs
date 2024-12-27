using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Services.Common;
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

        public ShippingAddressService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("GhnClient");
        }

        public async Task<ResponseModel> GetDistrictsAsync(int provinceId)
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
            if (districtId <= 0)
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
            };
        }

    }
}


