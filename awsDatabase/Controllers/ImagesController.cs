using awsDatabase.Services;
using Microsoft.AspNetCore.Mvc;

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

        // Endpoint to download an image by name
        [HttpGet("{imageName}/download")]
        public async Task<IActionResult> DownloadImage(string imageName)
        {
            try
            {
                var stream = await _imageService.DownloadImageAsync(id);

                if (stream == null)
                {
                    return NotFound();
                }

                return File(stream, "application/octet-stream"); // returns a FileStreamResult
            }
            catch (Exception ex)
            {
                // Here you could use a logger to log the exception
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to show metadata for all existing images
        [HttpGet]
        public async Task<IActionResult> GetAllImageMetadata()
        {
            var imageMetadata = await _imageService.GetAllImagesAsync();

            if (imageMetadata == null)
                return NotFound();

            return Ok(imageMetadata);
        }

        // Endpoint to upload an image
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image, string name)
        {
            try
            {
                var imageReference = await _imageService.UploadImageAsync(image, name);
                return Ok();
            }
            catch (Exception ex)
            {
                // Here you could use a logger to log the exception
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to delete an image by name
        [HttpDelete("{imageName}")]
        public async Task<IActionResult> DeleteImage(string imageName)
        {
            try
            {
                var result = await _imageService.DeleteImageAsync(imageName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint to get metadata for a random image
        [HttpGet("random")]
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
    }
}
