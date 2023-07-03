using awsDatabase.Data;
using awsDatabase.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace awsDatabase.Repositories
{
    public interface IImageRepository 
    {
        Task<List<Image>> GetAllImagesAsync();
        Task<List<Image>> GetImagesByNameAsync(string name);
        Task<Image> GetRandomImageAsync();
        Task<Image> SaveImageAsync(Image imageEntityModel);
        Task DeleteImageAsync(Image imageEntityModel);
    }

    public class ImageRepository : IImageRepository
    {
        private readonly ImageDbContext _context;
        public ImageRepository(ImageDbContext context)
        {
            _context = context;
        }

        public async Task<List<Image>> GetAllImagesAsync()
        {
            return await _context.Images.ToListAsync();
        }

        public async Task<List<Image>> GetImagesByNameAsync(string name)
        {
            return await _context.Images.Where(i => i.Name == name).ToListAsync();
        }

        public async Task<Image> GetRandomImageAsync()
        {
            try
            {
                return await _context.Images.OrderBy(_ => Guid.NewGuid()).Take(1).FirstAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching data from the database: {ex.Message}", ex);
            }
        }

        public async Task<Image> SaveImageAsync(Image imageEntityModel)
        {
            try
            {
                var result = await _context.Images.AddAsync(imageEntityModel);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch (DbException ex)
            {
                throw new Exception($"An DB error occurred when saving to the database: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An Unexpected error occurred when saving to the database: {ex.Message}", ex);
            }
        }

        public async Task DeleteImageAsync(Image imageEntityModel)
        {
            try
            {
                _context.Images.Remove(imageEntityModel);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting from the database: {ex.Message}", ex);
            }
        }
    }
}
