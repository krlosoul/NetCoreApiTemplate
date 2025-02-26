namespace Application.Interfaces.Services
{
    using Core.Dtos.Blobs;
    using Microsoft.AspNetCore.Http;

    public interface IAlfrescoService
    {
        /// <summary>
        /// Upload a file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public Task<string> UploadFileAsync(UploadBlobDto<IFormFile> uploadBlobDto, string folderId);

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public Task<byte[]> DownloadFileAsync(string fileId);

        /// <summary>
        /// Get file in Url.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public Task<string> GetFileUrlAsync(string fileId);

        /// <summary>
        /// Search file.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<IEnumerable<string>> SearchFilesAsync(string query);
    }
}