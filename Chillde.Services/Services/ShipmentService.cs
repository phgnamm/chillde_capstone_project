using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Interfaces;
using System.Text;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly string? _shopId;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;


        public ShipmentService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)

        {
            _shopId = configuration["GhnSettings:ShopId"];
            _unitOfWork = unitOfWork;
            _httpClient = httpClientFactory.CreateClient("GhnClient");

        }

        

        public async Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel? requestModel)
        {
            if (requestModel == null || requestModel.Weight <= 0 || string.IsNullOrEmpty(requestModel.ToWardCode))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid shipping fee request",
                    Data = null
                };
            }

            var requestPayload = new
            {
                from_district_id = requestModel.FromDistrictId,
                from_ward_code = requestModel.FromWardCode,
                service_id = requestModel.ServiceId,
                service_type_id = requestModel.ServiceTypeId,
                to_district_id = requestModel.ToDistrictId,
                to_ward_code = requestModel.ToWardCode,
                height = requestModel.Height > 0 ? requestModel.Height : null,
                length = requestModel.Length > 0 ? requestModel.Length : null,
                width = requestModel.Width > 0 ? requestModel.Width : null,
                weight = requestModel.Weight,
                insurance_value = requestModel.InsuranceValue > 0 ? (int?)requestModel.InsuranceValue : null,
                cod_failed_amount = requestModel.CodFailedAmount > 0 ? (int?)requestModel.CodFailedAmount : null,
                coupon = requestModel.Coupon
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8,
                "application/json");

            try
            {
             //   _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                var response = await _httpClient.PostAsync("v2/shipping-order/fee", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = "Failed to calculate shipping fee",
                        Data = null
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                var feeData = jsonObject?["data"]?.ToObject<ShippingFeeResponseModel>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Shipping fee calculated successfully",
                    Data = feeData
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while calculating shipping fee",
                    Data = ex.Message
                };
            }
        }
        public async Task<ResponseModel> GetShipmentDetailAsync(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Order code is required",
                    Data = null
                };
            }

            var requestPayload = new { order_code = orderCode };
            var content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                var response = await _httpClient.PostAsync("v2/shipping-order/detail", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = "Failed to retrieve shipment details",
                        Data = null
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                var shipmentData = jsonObject?["data"]?.ToObject<ShipmentDetailResponseModel>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Shipment details retrieved successfully",
                    Data = shipmentData
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while retrieving shipment details",
                    Data = ex.Message
                };
            }
        }

        public async Task<ResponseModel> SwitchToReturnStatusAsync(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Order code is required",
                    Data = null
                };
            }

            var requestPayload = new { order_codes = new[] { orderCode } };
            var content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                var response = await _httpClient.PostAsync("/v2/switch-status/return", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = "Failed to switch status to return",
                        Data = null
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                var switchStatusData = jsonObject?["data"]?.ToObject<List<SwitchStatusResponseModel>>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Status switched to return successfully",
                    Data = switchStatusData
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while switching status to return",
                    Data = ex.Message
                };
            }
        }
    }
}
