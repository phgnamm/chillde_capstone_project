using Chillde.Services.Interfaces;
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
        public async Task<IActionResult> GetProvinces()
        {
            var response = await _shippingAddressService.GetProvincesAsync();

            if (response.Status)
            {
                return Ok(response);
            }

            return StatusCode(response.Code, response);
        }
    }

}
