using ColorMate.BL.OutfitRatingService;
using ColorMate.Core.DTOs.ObjDetectionDto;
using ColorMate.Core.DTOs.OutfitRatingDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ColorMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OutfitRatingController : ControllerBase
    {

        private readonly IOutfitRatingService _outfitRatingService;

        public OutfitRatingController(IOutfitRatingService outfitRatingService)
        {
            _outfitRatingService = outfitRatingService;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> SendImageAndGetObjects(OutfitRatingRequestDto requestDto)
        {
            if (requestDto == null || requestDto.uploadedImage.Length <= 0)
            {
                return BadRequest("ImageBase64 is required");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("userId is null");
            }

            var result = await _outfitRatingService.GetOutfitRatingAsync(requestDto, userId);

            if (result == null)
            {
                return StatusCode(500, "outfit rating service failed");
            }

            return Ok(result);
        }

        [HttpGet("user-outfit-ratings-history")]
        public IActionResult GetUserDetections()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = _outfitRatingService.GetUserOutfitRatingsHistory(userId);

                if (result == null || !result.Any())
                {
                    return NotFound("No outfit ratings found for this user");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving outfit ratings: {ex.Message}");
            }
        }


    }
}
