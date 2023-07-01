
namespace awsDatabase.DTOs;
public class ImageUploadRequest
{
    public string Name { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
}