using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using awsDatabase.Constant;
using awsDatabase.DTOs;
using System.Net;

namespace awsDatabase.Services
{
    public interface IS3Service
    {
        Task<ImageUploadResponse> UploadFileAsync(IFormFile file, string name);
        Task<ImageDeleteResponse> DeleteFileAsync(string key);
        Task<MetadataCollection> GetFileMetadataAsync(string key);
        Task<ImageDownloadResponse> GetFileByKeyAsync(string key);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;

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
                BucketName = Constants.S3BucketName,
                Key = name,
                InputStream = stream,
                ContentType = file.ContentType,
            };

            try
            {
                if (!await ObjectExistsAsync(name))
                {
                    putObjectRequest.Metadata.Add("update-date", DateTime.UtcNow.ToString());
                    putObjectRequest.Metadata.Add("name", name);
                    putObjectRequest.Metadata.Add("size", file.Length.ToString());
                    putObjectRequest.Metadata.Add("Content-Type", file.ContentType);

                    var result = await _s3Client.PutObjectAsync(putObjectRequest);
                    response.StatusCode = (int)result.HttpStatusCode;
                    response.Message = $"{name} has been uploaded successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.Message = $"{name} has been already in bucket";
                }
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
                BucketName = Constants.S3BucketName,
                Key = key
            };

            try
            {
                if (await ObjectExistsAsync(key))
                {
                    await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                    response.StatusCode = 200;
                    response.Message = $"{key} has been deleted successfully";
                }
                else
                {
                    response.StatusCode = 404;
                    response.Message = $"{key} has been not found in bucket";
                }
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

        public async Task<ImageDownloadResponse> GetFileByKeyAsync(string key)
        {
            try
            {
                var s3Object = await _s3Client.GetObjectAsync(Constants.S3BucketName, key);
                return new ImageDownloadResponse
                {
                    ResponseStream = s3Object.ResponseStream,
                    ContentType = s3Object.Headers.ContentType
                };
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An AmazonS3Exception error occurred when getting object from S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred when getting object from S3: {ex.Message}", ex);
            }
        }

        public async Task<MetadataCollection> GetFileMetadataAsync(string key)
        {
            try
            {
                var metadataRequest = new GetObjectMetadataRequest
                {
                    BucketName = Constants.S3BucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectMetadataAsync(metadataRequest);

                return response.Metadata;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An AmazonS3Exception error occurred when getting metadata from S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred when getting metadata from S3: {ex.Message}", ex);
            }
        }

        public async Task<bool> BucketExistsAsync(string bucket)
        {
            try
            {
                var response = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucket);
                return response;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"An AmazonS3Exception error occurred when checking the bucket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred when checking the bucket: {ex.Message}", ex);
            }
        }

        public async Task<bool> ObjectExistsAsync(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = Constants.S3BucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;

                throw new Exception($"An AmazonS3Exception error occurred when checking the object: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred when checking the object: {ex.Message}", ex);
            }
        }
    }
}
