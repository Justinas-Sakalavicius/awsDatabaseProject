using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using awsDatabase.DTOs;
using System.Net;

namespace awsDatabase.Services
{
    public interface IS3Service
    {
        Task<ImageUploadResponse> UploadFileAsync(IFormFile file, string name);
        Task<ImageDeleteResponse> DeleteFileAsync(string key);
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

        public async Task<ImageUploadResponse> UploadFileAsync(IFormFile file, string name)
        {
            var response = new ImageUploadResponse();
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
                await BucketExistsAsync(BucketName);

                var result = await _s3Client.PutObjectAsync(putObjectRequest);
                response.StatusCode = (int)result.HttpStatusCode;
                response.Message = $"{name} has been uploaded successfully";
            }
            catch (AmazonS3Exception ex)
            {
                response.StatusCode = (int)ex.StatusCode;
                response.Message = $"An error occurred when uploading to S3: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"An error occurred when uploading to S3: {ex.Message}";
            }
            finally 
            { 
                stream.Close();
            }

            return response;
        }

        public async Task<ImageDeleteResponse> DeleteFileAsync(string key)
        {
            var response = new ImageDeleteResponse();
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
                response.StatusCode = (int)ex.StatusCode;
                response.Message = ($"An error occurred when deleting from S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ($"An error occurred when deleting from S3: {ex.Message}");
            }

            return response;
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
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when getting metadata from S3: {ex.Message}", ex);
            }
        }

        private async Task<bool> BucketExistsAsync(string bucket)
        {
            try
            {
                var response = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucket);
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
