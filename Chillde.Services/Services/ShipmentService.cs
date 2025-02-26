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
using Chillde.Repositories.Entities;

namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly string? _shopId;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;


        public ShipmentService(IConfiguration configuration, IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)

        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClientFactory.CreateClient("GhtkClient");

        }



        public async Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel requestModel)
        {
            var url = $"https://services.giaohangtietkiem.vn/services/shipment/fee?" +
                      $"address={Uri.EscapeDataString(requestModel.Address)}&" +
                      $"province={Uri.EscapeDataString(requestModel.Province)}&" +
                      $"district={Uri.EscapeDataString(requestModel.District)}&" +
                      $"pick_province={Uri.EscapeDataString(requestModel.PickProvince)}&" +
                      $"pick_district={Uri.EscapeDataString(requestModel.PickDistrict)}&" +
                      $"weight={requestModel.Weight}&" +
                      $"value={requestModel.Value}&" +
                      $"deliver_option={requestModel.DeliverOption}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

                var shipmentData = jsonObject?["fee"]?.ToObject<ShippingFeeResponseModel>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = shipmentData
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = null
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
