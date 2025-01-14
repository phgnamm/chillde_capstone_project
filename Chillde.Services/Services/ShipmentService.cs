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
        private readonly string _url;
        private readonly string _shopId;
        private readonly string _token;

        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;


        public ShipmentService(HttpClient httpClient,IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _url = configuration["GhnSettings:BaseUrl"]+ "shipping-order/create";
            _shopId = configuration["GhnSettings:ShopId"];
            _token = configuration["GhnSettings:Token"];
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> AddShipmentAsync(ShipmentAddModel model)
        {
            try
            {
                //var order = await _unitOfWork.OrderRepository.GetAsync(model.OrderId);
                //if (order == null)
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status404NotFound,
                //        Message = "Order not found."
                //    };
                //}

                //var customer = await _unitOfWork.AccountRepository.GetAsync((Guid)order.CreatedById);
                //if (customer == null)
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status404NotFound,
                //        Message = "Order not found."
                //    };

                var customer = await _unitOfWork.AccountRepository.GetAsync(Guid.Parse("01941858-ee7f-78bb-a60c-aa5443f14887"));
                if (customer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                _httpClient.DefaultRequestHeaders.Clear();
                //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                _httpClient.DefaultRequestHeaders.Add("Token", _token);

                var payload = new
                {
                    payment_type_id = 2,
                    note = model.Note,
                    required_note = model.RequiredNote.ToString(),
                    from_name = model.FromName,
                    from_phone = model.FromPhone,
                    from_address = model.FromAddress,
                    from_ward_name = model.FromWard,
                    from_district_name = model.FromDistrict,
                    from_province_name = model.FromProvince,
                    return_phone = (string?)null,
                    return_address = (string?)null,
                    return_district_id = (string?)null,
                    return_ward_code = "",
                    client_order_code = "",
                    to_name = customer.FirstName + " " + customer.LastName,
                    to_phone = "0987654321",//to_phone = order.Phone,
                    to_address = "72 Thành Thái, Phường 14, Quận 10, Hồ Chí Minh, Vietnam",//to_address = order.Address,
                    to_ward_code = "20308",//to_ward_code = order.Ward,
                    to_district_id = 1444,//to_district_id = order.District,
                    cod_amount = (int?)null,
                    content = (string?)null,
                    weight = model.Weight,
                    length = model.Length,
                    width = model.Width,
                    height = model.Height,
                    pick_station_id = (int?)null,
                    deliver_station_id = (int?)null,
                    insurance_value = 0,
                    service_id = (int?)null,
                    service_type_id = 2,
                    coupon = (string?)null,
                    pick_shift = (int[]?)null,
                    items = new[]
                {
                        new
                        {
                            name = model.ItemName,
                            code = (string?)null,
                            quantity = model.ItemQuantity,
                            price = model.ItemPrice,
                            length = (int?)null,
                            width = (int?)null,
                            height = (int?)null,
                            weight = model.ItemWeight,
                            category = new { level1 = "Áo" }
                        }
                    }
                };
    //            var payload = new
    //            {
    //                payment_type_id = 2,
    //                note = "Tintest 123",
    //                required_note = "KHONGCHOXEMHANG",
    //                from_name = "TinTest124",
    //                from_phone = "0987654321",
    //                from_address = "72 Thành Thái, Phường 14, Quận 10, Hồ Chí Minh, Vietnam",
    //                from_ward_name = "Phường 14",
    //                from_district_name = "Quận 10",
    //                from_province_name = "HCM",
    //                return_phone = "0332190444",
    //                return_address = "39 NTT",
    //                return_district_id = (string)null,
    //                return_ward_code = "",
    //                client_order_code = (string)null,
    //                to_name = "TinTest124",
    //                to_phone = "0987654321",
    //                to_address = "72 Thành Thái, Phường 14, Quận 10, Hồ Chí Minh, Vietnam",
    //                to_ward_code = "20308",
    //                to_district_id = 1444,
    //                cod_amount = 200000,
    //                content = "Theo New York Times",
    //                weight = 200,
    //                length = 1,
    //                width = 19,
    //                height = 10,
    //                pick_station_id = 1444,
    //                deliver_station_id = (int?)null,
    //                insurance_value = 0,
    //                service_id = 0,
    //                service_type_id = 2,
    //                coupon = (string)null,
    //                pick_shift = new[] { 2 },
    //                items = new[]
    //{
    //    new
    //    {
    //        name = "Áo Polo",
    //        code = "Polo123",
    //        quantity = 1,
    //        price = 200000,
    //        length = 12,
    //        width = 12,
    //        height = 12,
    //        weight = 1200,
    //        category = new { level1 = "Áo" }
    //    }
    //}
    //            };



                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send POST request
                var response = await _httpClient.PostAsync(_url, content);
                //response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ShipmentResponseModel>(responseContent);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Feedback created successfully.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
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
    }
}
