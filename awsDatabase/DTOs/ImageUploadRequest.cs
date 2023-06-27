
namespace awsDatabase.DTOs
{
    public class ImageUploadRequest
    {
        public IFormFile File { get; set; }
        public string Name { get; set; }
    }
}