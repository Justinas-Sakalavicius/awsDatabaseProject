namespace awsDatabase.DTOs
{
    public class ImageResponse 
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string FileExtension { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
