namespace Application.Interfaces.Services
{
    using Core.Dtos.Blobs;
    using Microsoft.AspNetCore.Http;

    public interface IMinioService
	{
        #region Queries
        /// <summary>
        /// Get file in container.
        /// </summary>
        /// <param name="getBlobDto">The GetBlobDto.</param>
        /// <returns>&lt;BlobDto&gt;.</returns>
        Task<BlobDto> GetFileAsync(GetBlobDto getBlobDto);
        #endregion

        #region Commands
        /// <summary>
        /// Upload a file.
        /// </summary>
        /// <param name="uploadBlobDto">The UploadBlobDto&lt;IFormFile&gt;.</param>
        /// <param name="file">The file.</param>
        /// <returns>&lt;BlobDto&gt;.</returns>
        Task<BlobDto> UploadFileAsync(UploadBlobDto<IFormFile> uploadBlobDto);

        /// <summary>
        /// Deleted a file with filename.
        /// </summary>
        /// <param name="getBlobDto">The GetBlobDto.</param>
        /// <returns>&lt;BlobDto&gt;.</returns>
        Task<BlobDto> DeleteFileAsync(GetBlobDto getBlobDto);
        #endregion
    }
}