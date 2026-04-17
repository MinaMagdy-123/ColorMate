using ColorMate.BL.ObjDetectionService;
using ColorMate.Core.DTOs;
using ColorMate.Core.DTOs.ObjDetectionDto;
using ColorMate.Core.DTOs.OutfitRatingDto;
using ColorMate.Core.Models;
using ColorMate.EF.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ColorMate.BL.OutfitRatingService
{
    public class OutfitRatingService : IOutfitRatingService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;


        public OutfitRatingService(HttpClient httpClient, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
        }

        public async Task<OutfitRatingResponseDto?> GetOutfitRatingAsync(OutfitRatingRequestDto requestDto, string userId)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = requestDto.uploadedImage.OpenReadStream();
                using var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(requestDto.uploadedImage.ContentType);

                content.Add(streamContent, "outfitImage", requestDto.uploadedImage.FileName);

                var response = await _httpClient.PostAsync(string.Empty, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new OutfitRatingResponseDto
                    {
                        Recommendation = "",
                        Score = -1
                    };
                }

                var outfitResult = await response.Content.ReadFromJsonAsync<OutfitRatingResponseDto>();

                if (outfitResult != null)
                {
                    // Save the image to database
                    byte[] imageBytes;

                    // Convert IFormFile to byte array for database storage
                    using (var memoryStream = new MemoryStream())
                    {
                        await requestDto.uploadedImage.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    var outfitWithImage = new OutfitRatingWithImage
                    {
                        OriginalImage = imageBytes,
                        ApplicationUserId = userId,
                        Recommendation = outfitResult.Recommendation,
                        Score = outfitResult.Score
                    };


                    _unitOfWork.OutfitRatingWithImages.Add(outfitWithImage);
                    _unitOfWork.Complete();
                }

                return outfitResult;
            }
            catch
            {
                return new OutfitRatingResponseDto
                {
                    Recommendation = "",
                    Score = -1
                };
            }
        }

        public List<OutfitRatingHistoryResponseDto> GetUserOutfitRatingsHistory(string userId)
        {
            var userHistory = _unitOfWork.OutfitRatingWithImages.GetAllQueryable().Where(od => od.ApplicationUserId == userId)
            .OrderByDescending(o => o.Id)
            .ToList();

            if (!userHistory.Any())
            {
                return new List<OutfitRatingHistoryResponseDto>();
            }

            var outfitHistoryList = new List<OutfitRatingHistoryResponseDto>();

            foreach (var outfit in userHistory)
            {
                // Convert byte array to Base64

                string imageBase64 = null;
                if (outfit.OriginalImage != null && outfit.OriginalImage.Length > 0)
                {
                    imageBase64 = Convert.ToBase64String(outfit.OriginalImage);
                }

                var outfitResponseDto = new OutfitRatingHistoryResponseDto
                {
                    ImageBase64 = imageBase64,
                    Recommendation = outfit.Recommendation,
                    Score = outfit.Score
                };

                outfitHistoryList.Add(outfitResponseDto);
            }

            return outfitHistoryList;


        }

    }
}
