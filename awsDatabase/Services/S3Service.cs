using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace awsDatabase.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string name);
        Task DeleteFileAsync(string key);
        Task<MetadataCollection> GetFileMetadataAsync(string key);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "justinas-sakalavicius-task8-image-api";

        public S3Service(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
            //var awsOptions = new AWSOptions
            //{
            //    Region = RegionEndpoint.EUNorth1, // Change to your AWS Region
            //                                      // For local development/testing, you could hard-code credentials like this:
            //                                      // But for production use, you'd want to use IAM roles, environment variables, or deployment parameters
            //    //Credentials = new BasicAWSCredentials("<aws_access_key_id>", "<aws_secret_access_key>"),
            //};

            //_s3Client = new AmazonS3Client(awsOptions.Credentials, awsOptions.Region);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string name)
        {
            using var stream = file.OpenReadStream();
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = name,
                InputStream = stream,
                ContentType = file.ContentType
            };

            try
            {
                await _s3Client.PutObjectAsync(putObjectRequest);
                return name;
            }
            catch (AmazonS3Exception ex)
            {
                // Handle exception as needed
                throw new Exception($"An error occurred when uploading to S3: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string key)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = key
            };

            try
            {
                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An error occurred when deleting from S3: {ex.Message}", ex);
            }
        }

        public async Task<MetadataCollection> GetFileMetadataAsync(string key)
        {
            try
            {
                var metadataRequest = new GetObjectMetadataRequest
                {
                    BucketName = BucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectMetadataAsync(metadataRequest);

                return response.Metadata;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An error occurred when getting metadata from S3: {ex.Message}", ex);
            }
        }

        private async Task<bool> BucketExistsAsync(string bucket)
        {
            try
            {
                var response = await _s3Client.DoesS3BucketExistAsync(bucket);
                return response;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An error occurred when checking the bucket: {ex.Message}", ex);
            }
        }

        private async Task<bool> ObjectExistsAsync(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = BucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }
    }
}
