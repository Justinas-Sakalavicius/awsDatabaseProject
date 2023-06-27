using Amazon.Runtime;
using Amazon.S3.Model;
using AutoMapper;
using awsDatabase.DTO;
using awsDatabase.Models;
using awsDatabase.Repositories;

namespace awsDatabase.Services
{
    public interface IImageService
    {
        Task<List<ImageVM>> GetAllImagesAsync();
        Task<ImageVM> UploadImageAsync(IFormFile uploadClientModel, string name);
        Task<ImageVM> DeleteImageAsync(string name);
        Task<ImageVM> GetRandomImageAsync();
        Task<byte[]> GetImageAsync(string name);
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

        public async Task<ImageVM> DeleteImageAsync(string name)
        {
            var imageReference = await _imageRepository.GetImagesByNameAsync(name);

            if (imageReference == null)
            {
                throw new Exception("Image not found.");
            }

            // Delete the file from S3
            await _s3Service.DeleteFileAsync(imageReference.First().Url);

            // Delete the reference from the database
            await _imageRepository.DeleteImageAsync(imageReference.First());

            return new ImageVM();
        }

        public async Task<List<ImageVM>> GetAllImagesAsync()
        {
            var entities = await _imageRepository.GetAllImagesAsync();
            //var viewModel = entities.Select(entity => _imageMapper.ToClientModel(entity, _s3Service)).ToList();

            return new List<ImageVM>();
        }

        public async Task<byte[]> GetImageAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ImageVM> GetRandomImageAsync()
        {
            var imageReference = await _imageRepository.GetRandomImageAsync();
            if (imageReference == null)
            {
                throw new Exception("No images found.");
            }

            var imageMetadata = new ImageVM
            {
                Name = imageReference.Name,
                FileExtension = imageReference.FileExtension,
                Size = imageReference.Size,
                UpdatedAt = imageReference.UpdatedAt,
            };

            return imageMetadata;
        }

        public async Task<ImageVM> UploadImageAsync(IFormFile file, string name)
        {
            // Process file
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(file.FileName);

            var s3Url = await _s3Service.UploadFileAsync(file, name);

            var imageReference = new Image
            {
                Name = name,
                FileExtension = Path.GetExtension(file.FileName),
                Size = memoryStream.Length,
                Url = s3Url,
                CreateAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _imageRepository.SaveImageAsync(imageReference);

            return new ImageVM();
        }

    }
}
