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
                service_id = requestModel.ServiceId,
                service_type_id = requestModel.ServiceTypeId,
                to_district_id = requestModel.ToDistrictId,
                to_ward_code = requestModel.ToWardCode,
                height = requestModel.Height,
                length = requestModel.Length,
                weight = requestModel.Weight,
                width = requestModel.Width,
                insurance_value = requestModel.InsuranceValue
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/v2/shipping-order/fee", content);

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
    }
}