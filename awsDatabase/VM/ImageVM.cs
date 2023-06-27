using Amazon.S3.Model;

namespace awsDatabase.DTO
{
    public class ImageVM
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public string FileExtension { get; set; }

        public DateTime UpdatedAt { get; set; }

        public byte[] Bitmap { get; set; }

        public MetadataCollection MetadataCollection { get; set; }
    }
}
