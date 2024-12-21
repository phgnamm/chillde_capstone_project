using Chillde.Services.Interfaces;
using Chillde.Services.Models.LanguageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/languages")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Get a language by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _languageService.GetAsync(id);
                if (result == null)
                {
                    return NotFound(new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found."
                    });
                }

                return Ok(result);
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

        /// <summary>
        /// Get all languages.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _languageService.GetAllAsync();
                return Ok(result);
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

        /// <summary>
        /// Create a new language.
        /// </summary>
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] LanguageAddModel languageCreateModel)
        {
            try
            {
                if (languageCreateModel == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid language data."
                    });
                }

                var result = await _languageService.AddAsync(languageCreateModel);
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

        /// <summary>
        /// Update an existing language.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LanguageAddModel languageUpdateModel)
        {
            try
            {
                var existingLanguage = await _languageService.GetAsync(id);
                if (existingLanguage == null)
                {
                    return NotFound(new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found."
                    });
                }

                await _languageService.Update(id, languageUpdateModel);
                return NoContent();
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

        /// <summary>
        /// Delete a language.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var existingLanguage = await _languageService.GetAsync(id);
                if (existingLanguage == null)
                {
                    return NotFound(new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found."
                    });
                }

                await _languageService.DeleteAsync(id);
                return NoContent();
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
