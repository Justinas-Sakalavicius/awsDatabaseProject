using Amazon.Runtime;
using Amazon.S3.Model;
using AutoMapper;
using awsDatabase.Constant;
using awsDatabase.DTOs;
using awsDatabase.Models;
using awsDatabase.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace awsDatabase.Services
{
    public interface IImageService
    {
        Task<List<ImageResponse>> GetAllImagesAsync();
        Task<ImageResponse> UploadImageAsync(IFormFile uploadClientModel, string name);
        Task DeleteImageAsync(string name);
        Task<ImageResponse> GetRandomImageAsync();
        Task<ImageDownloadResponse> GetImageAsync(string name);
    }

    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IMapper _mapper;
        private readonly IS3Service _s3Service;

        public ImageService(
            IImageRepository repository,
            IMapper mapper,
            IS3Service s3Service)
        {
            _imageRepository = repository;
            _mapper = mapper;
            _s3Service = s3Service;
        }

        public async Task<List<ImageResponse>> GetAllImagesAsync()
        {
            var entities = await _imageRepository.GetAllImagesAsync();
            var imageResponses = _mapper.Map<List<ImageResponse>>(entities);
            return imageResponses;
        }

        public async Task<ImageResponse> UploadImageAsync(IFormFile file, string name)
        {
            // Process file
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(file.FileName);

            var imageUploadResponse = await _s3Service.UploadFileAsync(file, name);

            var imageReference = new Image
            {
                Name = name,
                FileExtension = Path.GetExtension(file.FileName),
                Size = memoryStream.Length,
                Url = $"https://{Constants.S3BucketName}.s3.eu-north-1.amazonaws.com/{name}",
                CreateAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _imageRepository.SaveImageAsync(imageReference);

            var imageResponse = _mapper.Map<ImageResponse>(imageReference);

            return imageResponse;
        }

        public async Task DeleteImageAsync(string name)
        {
            var imageReference = await _imageRepository.GetImagesByNameAsync(name);

            if (imageReference == null)
            {
                throw new Exception("Image not found.");
            }

            // Delete the file from S3
            var result = await _s3Service.DeleteFileAsync(imageReference.First().Name);

            if(result.StatusCode < 300)
            {
                // Delete the reference from the database
                await _imageRepository.DeleteImageAsync(imageReference.First());
            }
        }

        public async Task<ImageDownloadResponse> GetImageAsync(string name)
        {
            return await _s3Service.GetFileByKeyAsync(name);
        }

        public async Task<ImageResponse> GetRandomImageAsync()
        {
            var imageReference = await _imageRepository.GetRandomImageAsync();
            if (imageReference == null)
            {
                throw new Exception("No images found.");
            }

            var imageMetadata = new ImageResponse
            {
                Name = imageReference.Name,
                FileExtension = imageReference.FileExtension,
                Size = imageReference.Size,
                UpdatedAt = imageReference.UpdatedAt,
            };

            return imageMetadata;
        }
    }
}
