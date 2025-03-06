using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json.Nodes;
namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;

        public ShipmentService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel requestModel)
        {
            var url = $"https://services.giaohangtietkiem.vn/services/shipment/fee?" +
                      $"address={Uri.EscapeDataString(requestModel.Address ?? string.Empty)}&" +
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

        public async Task<CancelShipmentResponseModel> CancelShipmentAsync(string trackingOrder)
        {
            var url = $"https://services.giaohangtietkiem.vn/services/shipment/cancel/{trackingOrder}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var cancelResponse = JsonConvert.DeserializeObject<CancelShipmentResponseModel>(content);

                if (cancelResponse != null) return cancelResponse;
            }
            catch (HttpRequestException ex)
            {
                return new CancelShipmentResponseModel
                {
                    Success = false,
                    Message = ex.Message,
                    LogId = null
                };
            }

            return null!;
        }

        public async Task<ResponseModel> GetOrderStatusAsync(string trackingOrder)
        {
            if (string.IsNullOrEmpty(trackingOrder))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Tracking order is required.",
                    Data = null
                };
            }

            var url = $"https://services.giaohangtietkiem.vn/services/shipment/v2/{trackingOrder}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode(); 

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

                if (jsonObject?["success"]?.Value<bool>() == true)
                {
                    var orderStatusResponse = jsonObject["order"]?.ToObject<OrderStatusResponseModel>();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Success",
                        Data = orderStatusResponse
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
                        Data = null
                    };
                }
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

        public async Task<byte[]> GetShippingLabelAsync(string trackingOrder)
        {
            string url = $"https://services.giaohangtietkiem.vn/services/services/label/{trackingOrder}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                throw new Exception($"Lỗi khi in nhãn đơn hàng: {await response.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel)
        {
            if (shipmentCreateModel == null || !shipmentCreateModel.Products.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid shipment data",
                    Data = null
                };
            }

            var url = "https://services.giaohangtietkiem.vn/services/shipment/order";
            var jsonBody = JsonConvert.SerializeObject(new
            {
                products = shipmentCreateModel.Products,
                order = new
                {
                    id = shipmentCreateModel.Id,
                    pick_name = shipmentCreateModel.PickName,
                    pick_address = shipmentCreateModel.PickAddress,
                    pick_province = shipmentCreateModel.PickProvince,
                    pick_district = shipmentCreateModel.PickDistrict,
                    pick_ward = shipmentCreateModel.PickWard,
                    pick_tel = shipmentCreateModel.PickTel,
                    name = shipmentCreateModel.Name,
                    address = shipmentCreateModel.Address,
                    province = shipmentCreateModel.Province,
                    district = shipmentCreateModel.District,
                    ward = shipmentCreateModel.Ward,
                    tel = shipmentCreateModel.Tel,
                    hamlet = shipmentCreateModel.Hamlet,
                    email = shipmentCreateModel.Email ?? "phuongnam@gmail.com",
                    //return_name = shipmentCreateModel.ReturnName,
                    //return_address = shipmentCreateModel.ReturnAddress,
                    //return_province = shipmentCreateModel.ReturnProvince,
                    //return_district = shipmentCreateModel.ReturnDistrict,
                    //return_tel = shipmentCreateModel.ReturnTel,
                    //return_email = shipmentCreateModel.ReturnEmail,
                    is_freeship = shipmentCreateModel.IsFreeShip,
                    pick_date = shipmentCreateModel.PickDate,
                    deliver_date = shipmentCreateModel.DeliverDate,
                    pick_money = shipmentCreateModel.PickMoney,
                    note = shipmentCreateModel.Note,
                    value = shipmentCreateModel.Value,
                    transport = shipmentCreateModel.Transport,
                    pick_option = shipmentCreateModel.PickOption,
                    deliver_option = shipmentCreateModel.DeliverOption,
                    tags = shipmentCreateModel.Tags
                }
            }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = $"Error: {response.ReasonPhrase}. API Response: {responseContent}",
                        Data = null
                    };
                }

                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                if (jsonObject?["success"]?.Value<bool>() != true)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
                        Data = null
                    };
                }

                var orderStatusResponse = jsonObject["order"]?.ToObject<ShipmentAddResponseModel>();
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = orderStatusResponse
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
