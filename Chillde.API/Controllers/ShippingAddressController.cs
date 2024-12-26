using Chillde.Services.Interfaces;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/shippingaddresses")]
    [ApiController]
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressService _shippingAddressService;

        public ShippingAddressController(IShippingAddressService shippingAddressService)
        {
            _shippingAddressService = shippingAddressService;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces([FromQuery] ProvinceFilterModel provinceFilterModel)
        {
            var response = await _shippingAddressService.GetProvincesAsync(provinceFilterModel);

            if (response.Status)
            {
                return Ok(response);
            }

            return StatusCode(response.Code, response);
        }
    }

}
