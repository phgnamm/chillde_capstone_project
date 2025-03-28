using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _ghtkUrl;
        private readonly string _ghtkUsername;
        private readonly string _ghtkPassword;

        public ShipmentService(IHttpClientFactory httpClientFactory,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _unitOfWork = unitOfWork;
            _ghtkUrl = configuration["GhtkSettings:BaseUrl"]!;
            _ghtkUsername = configuration["GhtkSettings:Username"]!;
            _ghtkPassword = configuration["GhtkSettings:Password"]!;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel requestModel)
        {
            var url = $"{_ghtkUrl}/shipment/fee?" +
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
            var url = $"{_ghtkUrl}/shipment/cancel/{trackingOrder}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                //response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var cancelResponse = JsonConvert.DeserializeObject<CancelShipmentResponseModel>(content);

                var shipment = _unitOfWork.ShipmentRepository.GetShipmentByPartnerIdOrLabel(trackingOrder);
                shipment!.StatusId = ShipmentStatus.Cancelled;

                _unitOfWork.ShipmentRepository.Update(shipment);
                await _unitOfWork.SaveChangeAsync();

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

        //public async Task<ResponseModel> GetOrderStatusAsync(string trackingOrder)
        //{
        //    if (string.IsNullOrEmpty(trackingOrder))
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Tracking order is required.",
        //            Data = null
        //        };
        //    }

        //    var url = $"{_ghtkUrl}/shipment/v2/{trackingOrder}";
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        //    try
        //    {
        //        var response = await _httpClient.SendAsync(requestMessage);
        //        response.EnsureSuccessStatusCode();

        //        var content = await response.Content.ReadAsStringAsync();
        //        var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

        //        if (jsonObject?["success"]?.Value<bool>() == true)
        //        {
        //            var orderStatusResponse = jsonObject["order"]?.ToObject<OrderStatusResponseModel>();
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status200OK,
        //                Message = "Success",
        //                Data = orderStatusResponse
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status400BadRequest,
        //                Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
        //                Data = loginContent
        //            };
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = ex.Message,
        //            Data = null
        //        };
        //    }
        //}

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

            var ghtkLoginUrl = "https://web-staging.ghtklab.com/api/v1/auth/login";
            var loginPayload = new
            {
                username = _ghtkUsername,
                password = _ghtkPassword,
                new_version = "true"
            };

            var ghtkJwtToken = _httpContextAccessor.HttpContext?.Session.GetString("GhtkToken").ToString();
            if (!CheckGHTKJwtValidity(ghtkJwtToken))
            {
                var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, ghtkLoginUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json")
                };
                var loginResponse = await _httpClient.SendAsync(loginRequestMessage);
                var loginContent = await loginResponse.Content.ReadAsStringAsync();
                var jwt = JsonConvert.DeserializeObject<GHTKAccountInfo>(loginContent)!.Data!.Jwt;
                ghtkJwtToken = JsonConvert.DeserializeObject<GHTKAccountInfo>(loginContent)!.Data!.Jwt;
                _httpContextAccessor.HttpContext?.Session.SetString("GhtkToken", ghtkJwtToken);
            }

            var url = $"https://web-staging.ghtklab.com/api/v1/package/package-detail?alias={trackingOrder}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ghtkJwtToken);


            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<ShipmentStatusResponseModel>(content);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = jsonObject
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

        public async Task<byte[]> GetShippingLabelAsync(string trackingOrder)
        {
            string url = $"{_ghtkUrl}/label/{trackingOrder}";
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

        public bool CheckGHTKJwtValidity(string ghtkJwtToken)
        {
            if (string.IsNullOrEmpty(ghtkJwtToken))
            {
                return false;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var token = handler.ReadJwtToken(ghtkJwtToken);
                var payload = token.Payload as IDictionary<string, object>;

                if (token == null)
                {
                    return false;
                }

                var invalidAt = payload["invalid_at"].ToString();
                using JsonDocument doc = JsonDocument.Parse(invalidAt);
                string dateTimeInvalid = doc.RootElement.GetProperty("date").GetString();

                if (DateTime.Parse(dateTimeInvalid) < DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
