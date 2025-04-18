using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/searches")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchService _elasticSearchService;
        private readonly ISearchHistoryService _searchHistoryService;

        public SearchController(IElasticsearchService elasticSearchService, ISearchHistoryService searchHistoryService)
        {
            _elasticSearchService = elasticSearchService;
            _searchHistoryService = searchHistoryService;
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> SuggestKeywordsAsync([FromQuery] string indexName, [FromQuery] string query)
        {
            try
            {
                var result = await _elasticSearchService.SuggestKeywordsAsync(indexName, query);
                if (result.Status)
                {
                    return Ok(result);
                }

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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _searchHistoryService.Delete(id);
                if (result.Status)
                {
                    return Ok(result);
                }

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
