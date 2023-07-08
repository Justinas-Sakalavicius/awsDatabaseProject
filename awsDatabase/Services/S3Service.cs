using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using awsDatabase.Constant;
using awsDatabase.Controllers;
using awsDatabase.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;
using System.Text;

namespace awsDatabase.Services
{
    public interface IS3Service
    {
        Task<ImageUploadResponse> UploadFileAsync(IFormFile file, string name);
        Task<ImageDeleteResponse> DeleteFileAsync(string key);
        Task<ImageDownloadResponse> GetFileByKeyAsync(string key);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly INotificationService _notificationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public S3Service(
            IAmazonS3 s3Client, 
            INotificationService notificationService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor
            )
        {
            _s3Client = s3Client;
            _notificationService = notificationService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
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
                    var metadata = new Dictionary<string, string>();

                    metadata["name"] = name;
                    metadata["size"] = file.Length.ToString();
                    metadata["content-type"] = file.ContentType;
                    metadata["update-date"] = DateTime.UtcNow.ToString();

                    var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                    metadata["link"] = urlHelper.Action(
                        nameof(ImagesController.DownloadImage), "Images", new { imageName = name }, 
                        urlHelper.ActionContext.HttpContext.Request.Scheme);

                    putObjectRequest.Metadata.Add("name", name);
                    putObjectRequest.Metadata.Add("size", file.Length.ToString());
                    putObjectRequest.Metadata.Add("content-type", file.ContentType);
                    putObjectRequest.Metadata.Add("update-date", DateTime.UtcNow.ToString());

                    var result = await _s3Client.PutObjectAsync(putObjectRequest);
                    
                    await _notificationService.SendMessageToQueue(CreateMessage(metadata));

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

        private string CreateMessage(Dictionary<string, string> imageMetadata)
        {
            StringBuilder builder = new();
            builder.AppendLine("Image was uploaded");
            builder.AppendLine();
            foreach (var keyValuePair in imageMetadata)
            {
                builder.AppendLine($"{keyValuePair.Key}:::{keyValuePair.Value}");
            }

            return builder.ToString();
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
