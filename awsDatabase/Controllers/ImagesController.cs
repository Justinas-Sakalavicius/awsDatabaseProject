﻿using awsDatabase.Services;
using Microsoft.AspNetCore.Mvc;
using awsDatabase.DTOs;

namespace awsDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;  // This should be an interface for your image service
        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // Endpoint to show metadata for all existing images
        public async Task<IActionResult> GetAll()
        {
            var imageMetadata = await _imageService.GetAllImagesAsync();

            if (imageMetadata == null)
                return NotFound();

            return Ok(imageMetadata);
        }

         // Endpoint to upload an image
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(ImageUploadRequest request)
        {
            try
            {
                var imageReference = await _imageService.UploadImageAsync(request.File, request.Name);
                return Ok();
            }
            catch (Exception ex)
            {
                // Here you could use a logger to log the exception
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to delete an image by name
        [HttpDelete("delete/{imageName}")]
        public async Task<IActionResult> DeleteImage(string imageName)
        {
            try
            {
                await _imageService.DeleteImageAsync(imageName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to get metadata for a random image
        [HttpGet("get-random")]
        public async Task<IActionResult> GetRandomImageMetadata()
        {
            try
            {
                var imageReference = await _imageService.GetRandomImageAsync();
                return Ok(imageReference);
            }
            catch (Exception ex)
            {
                // Here you could use a logger to log the exception
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to download an image by name
        [HttpGet("download/{imageName}")]
        public async Task<IActionResult> DownloadImage(string imageName)
        {
            try
            {
                // var stream = await _imageService.DownloadImageAsync(id);

                // if (stream == null)
                // {
                //     return NotFound();
                // }

                //return File(stream, "application/octet-stream"); // returns a FileStreamResult
                return Ok();
            }
            catch (Exception ex)
            {
                // Here you could use a logger to log the exception
                return BadRequest(ex.Message);
            }
        }
    }
}
