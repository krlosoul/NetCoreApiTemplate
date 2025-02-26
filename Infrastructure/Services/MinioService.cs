namespace Infrastructure.Services
{
    using Application.Interfaces.Services;
    using Core.Dtos.Blobs;
    using Core.Dtos.SecretsDto;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.StaticFiles;
    using Minio;
    using Minio.DataModel.Args;
    using Minio.Exceptions;

    public class MinioService : IMinioService
    {
        #region Properties
        private readonly IMinioClient _minioClient;
        private readonly MinioSecretDto? _minioDto;
        #endregion

        public MinioService(MinioSecretDto minioSecretDto)
        {
            try{
                _minioDto = minioSecretDto;
                var endpoint = _minioDto.Endpoint;
                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uriResult) ||  (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    throw new InvalidEndpointException($"Invalid MinIO endpoint: {endpoint}");
                }
                _minioClient = new MinioClient()
                    .WithEndpoint(uriResult.Host, uriResult.Port)
                    .WithCredentials(_minioDto.AccessKey, _minioDto.SecretKey)
                    .Build();
                    
                }          
            catch(MinioException me){
                throw new MinioException(me.Message.ToString());
            }

        }

        #region Queries
        public async Task<BlobDto> GetFileAsync(GetBlobDto getBlobDto)
        {
            try{
                string fullUri = await GetPresignedUrlAsync($"{getBlobDto.FileName}");
                return new BlobDto
                {
                    Name = getBlobDto.FileName,
                    Uri = fullUri,
                    ContentType = GetContentType($"{getBlobDto.FileName}")
                };
            }
            catch (MinioException me)
            {
                throw new MinioException($"Error get file to MinIO: {me.Message}", me);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }
        #endregion

        #region Commands
        public async Task<BlobDto> UploadFileAsync(UploadBlobDto<IFormFile> uploadBlobDto)
        {
            try
            {
                using var stream = (uploadBlobDto.File?.OpenReadStream()) ?? throw new ArgumentNullException(nameof(uploadBlobDto.File), "The file stream cannot be null.");
                var fileName = uploadBlobDto.File?.FileName.ToLower();
                var contentType = uploadBlobDto.File?.ContentType ?? "application/octet-stream";
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_minioDto?.BucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                var presignedUrl = await GetPresignedUrlAsync($"{fileName}");

                return new BlobDto
                {
                    Name = fileName,
                    Uri = presignedUrl
                };
            }
            catch (MinioException me)
            {
                throw new MinioException($"Error uploading file to MinIO: {me.Message}", me);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public async Task<BlobDto> DeleteFileAsync(GetBlobDto getBlobDto)
        {
            try{
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_minioDto?.BucketName)
                    .WithObject(getBlobDto.FileName));

            return new BlobDto { Name = getBlobDto.FileName };
            }
            catch (MinioException me)
            {
                throw new MinioException($"Error remove file to MinIO: {me.Message}", me);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }
        #endregion

        #region Private Methods
        private async Task<string> GetPresignedUrlAsync(string objectName, int expiresInSeconds = 3600)
        {
            var presignedUrlArgs = new PresignedGetObjectArgs()
                .WithBucket(_minioDto?.BucketName)
                .WithObject(objectName)
                .WithExpiry(expiresInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedUrlArgs);
        }

        private static string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string? contentType)) contentType = "application/octet-stream";
            return contentType;
        }      
        #endregion
    }
}