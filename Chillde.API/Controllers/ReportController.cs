using Chillde.Services.Interfaces;
using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("{reportId}/reject")]
        public async Task<IActionResult> Reject(Guid reportId, [FromBody] ReportRejectOrAcceptModel reportRejectModel)
        {
            try
            {
                var result = await _reportService.Reject(reportId, reportRejectModel);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("{reportId}/accept")]
        public async Task<IActionResult> Accept(Guid reportId, [FromBody] ReportRejectOrAcceptModel reportRejectModel)
        {
            try
            {
                var result = await _reportService.Accept(reportId, reportRejectModel);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ReportFilterModel reportFilterModel)
        {
            try
            {
                var result = await _reportService.GetAll(reportFilterModel);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }
    }
}
