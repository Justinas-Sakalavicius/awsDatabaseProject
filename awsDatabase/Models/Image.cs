namespace awsDatabase.Models
{
    public class Image
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public long Size { get; set; }
        public string Url { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
