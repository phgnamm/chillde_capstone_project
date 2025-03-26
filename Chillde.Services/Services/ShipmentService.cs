using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _ghtkUrl;

        public ShipmentService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _unitOfWork = unitOfWork;
            _ghtkUrl = configuration["GhtkSettings:BaseUrl"];
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

            var url = $"{_ghtkUrl}/shipment/v2/{trackingOrder}";
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

        //public async Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel, Guid orderId)
        //{
        //    var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
        //    if (order == null)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status404NotFound,
        //            Message = "Order not found."
        //        };
        //    }

        //    if (order.Stage != OrderStage.Shipping || order.Stage != OrderStage.Return)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status422UnprocessableEntity,
        //            Message = "Shipment can only be initiated at the delivery and return stage."
        //        };
        //    }

        //    var availableShipment = await _unitOfWork.ShipmentRepository.HasAvalaibleShipment(orderId);

        //    if (!availableShipment)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status422UnprocessableEntity,
        //            Message = "Order already had shipment."
        //        };
        //    }

        //    if (shipmentCreateModel == null || !shipmentCreateModel.Products.Any())
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Invalid shipment data",
        //            Data = null
        //        };
        //    }

        //    var url = "https://services.giaohangtietkiem.vn/services/shipment/order";
        //    var jsonBody = JsonConvert.SerializeObject(new
        //    {
        //        products = shipmentCreateModel.Products,
        //        order = new
        //        {
        //            id = shipmentCreateModel.Id,
        //            pick_name = shipmentCreateModel.PickName,
        //            pick_address = shipmentCreateModel.PickAddress,
        //            pick_province = shipmentCreateModel.PickProvince,
        //            pick_district = shipmentCreateModel.PickDistrict,
        //            pick_ward = shipmentCreateModel.PickWard,
        //            pick_tel = shipmentCreateModel.PickTel,
        //            name = shipmentCreateModel.Name,
        //            address = shipmentCreateModel.Address,
        //            province = shipmentCreateModel.Province,
        //            district = shipmentCreateModel.District,
        //            ward = shipmentCreateModel.Ward,
        //            tel = shipmentCreateModel.Tel,
        //            hamlet = shipmentCreateModel.Hamlet,
        //            email = shipmentCreateModel.Email,
        //            //return_name = shipmentCreateModel.ReturnName,
        //            //return_address = shipmentCreateModel.ReturnAddress,
        //            //return_province = shipmentCreateModel.ReturnProvince,
        //            //return_district = shipmentCreateModel.ReturnDistrict,
        //            //return_tel = shipmentCreateModel.ReturnTel,
        //            //return_email = shipmentCreateModel.ReturnEmail,
        //            is_freeship = shipmentCreateModel.IsFreeShip,
        //            pick_date = shipmentCreateModel.PickDate,
        //            deliver_date = shipmentCreateModel.DeliverDate,
        //            pick_money = shipmentCreateModel.PickMoney,
        //            note = shipmentCreateModel.Note,
        //            value = shipmentCreateModel.Value,
        //            transport = shipmentCreateModel.Transport,
        //            pick_option = shipmentCreateModel.PickOption,
        //            deliver_option = shipmentCreateModel.DeliverOption,
        //            tags = shipmentCreateModel.Tags
        //        }
        //    }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        //    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        //    {
        //        Content = content
        //    };
        //    try
        //    {
        //        var response = await _httpClient.SendAsync(requestMessage);
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var parsedJson = JsonConvert.DeserializeObject<ShipmentAddResponseModel>(responseContent);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return new ResponseModel
        //            {
        //                Code = (int)response.StatusCode,
        //                Message = parsedJson.Success,
        //                Data = parsedJson
        //            };
        //        }

        //        Shipment shipment = new()
        //        {
        //            TrackingId = parsedJson!.Order!.TrackingId.ToString(),
        //            StatusId = (ShipmentStatus)(parsedJson.Order?.StatusId ?? 0),
        //            PartnerId = parsedJson!.Order!.PartnerId,
        //            Label = parsedJson.Order.Label,
        //            Area = parsedJson.Order.Area,
        //            Fee = parsedJson.Order.Fee != null ? decimal.Parse(parsedJson.Order.Fee) : 0,
        //            InsuranceFee = parsedJson.Order.InsuranceFee != null ? decimal.Parse(parsedJson.Order.InsuranceFee) : 0,
        //            EstimatedPickTime = parsedJson.Order.EstimatedPickTime,
        //            EstimatedDeliverTime = parsedJson.Order.EstimatedDeliverTime,
        //        };

        //        await _unitOfWork.ShipmentRepository.AddAsync(shipment);
        //        await _unitOfWork.SaveChangeAsync();

        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status201Created,
        //            Message = "Success",
        //            Data = shipment
        //        };

        //        //var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
        //        //if (jsonObject?["success"]?.Value<bool>() != true)
        //        //{
        //        //    return new ResponseModel
        //        //    {
        //        //        Code = StatusCodes.Status400BadRequest,
        //        //        Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
        //        //        Data = null
        //        //    };
        //        //}

        //        //var orderStatusResponse = jsonObject["order"]?.ToObject<ShipmentAddResponseModel>();
        //        //return new ResponseModel
        //        //{
        //        //    Code = StatusCodes.Status200OK,
        //        //    Message = "Success",
        //        //    Data = orderStatusResponse
        //        //};
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = $"Error: {ex.Message}",
        //            Data = null
        //        };
        //    }
        //}

        public async Task<ResponseModel> UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus newStatus)
        {
            try
            {
                var shipment = await _unitOfWork.ShipmentRepository.GetAsync(shipmentId);
                if (shipment == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Shipment not found."
                    };
                }

                if (shipment.StatusId == newStatus)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Shipment status is already up-to-date."
                    };
                }

                shipment.StatusId = newStatus;

                _unitOfWork.ShipmentRepository.Update(shipment);
                var updateResult = await _unitOfWork.SaveChangeAsync();
                if (updateResult <= 0)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to update shipment status."
                    };
                }              

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Shipment status updated ",
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error updating shipment status: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> GetALlShipmentAsync(ShipmentFilterModel model)
        {
            Expression<Func<Shipment, bool>> filter = s =>
               s.IsDeleted == model.IsDeleted &&
               (string.IsNullOrEmpty(model.Search) ||
                s.TrackingId!.Contains(model.Search) ||
                s.PartnerId!.Contains(model.Search) ||
                s.Label!.Contains(model.Search) ||
                s.Fee!.Equals(model.Search));

            var shipmentdetails = await _unitOfWork.ShipmentRepository.GetAllAsync(
            filter: filter,
            pageIndex: model.PageIndex,
            pageSize: model.PageSize
        );

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = shipmentdetails.Data
            };
        }


    }
}
