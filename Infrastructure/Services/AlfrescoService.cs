namespace Infrastructure.Services
{
    using Application.Interfaces.Services;
    using Core.Dtos.Blobs;
    using Core.Dtos.SecretsDto;
    using Microsoft.AspNetCore.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;

    public class AlfrescoService(HttpClient httpClient, AlfrescoSecretDto alfrescoSecretDto) : IAlfrescoService
    {
        #region Properties 
        private readonly AlfrescoSecretDto _alfrescoSecretDto = alfrescoSecretDto;
        private readonly HttpClient _httpClient = httpClient;
        private string? _authToken;
        #endregion

        public async Task<string> UploadFileAsync(UploadBlobDto<IFormFile> uploadBlobDto, string folderId)
        {
            try
            {
                if (uploadBlobDto.File == null)
                    throw new ArgumentNullException(nameof(uploadBlobDto.File), "The file cannot be null.");

                using var memoryStream = new MemoryStream();
                await uploadBlobDto.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var authToken = await GetAuthTokenAsync();

                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(memoryStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(uploadBlobDto.File.ContentType ?? "application/octet-stream");

                var safeFileName = Uri.EscapeDataString(uploadBlobDto.File.FileName);
                content.Add(fileContent, "filedata", safeFileName);

                var url = $"{_alfrescoSecretDto.Uri}/alfresco/api/-default-/public/alfresco/versions/1/nodes/{folderId}/children?alf_ticket={authToken}";

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseBody);
                var fileId = doc.RootElement.GetProperty("entry").GetProperty("id").GetString();

                return fileId!;
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"HTTP error uploading file to Alfresco: {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> DownloadFileAsync(string fileId)
        {
            var authToken = await GetAuthTokenAsync();
            var url = $"{_alfrescoSecretDto.Uri}/alfresco/api/-default-/public/alfresco/versions/1/nodes/{fileId}/content?alf_ticket={authToken}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> GetFileUrlAsync(string fileId)
        {
            var authToken = await GetAuthTokenAsync();
            return $"{_alfrescoSecretDto.Uri}/alfresco/api/-default-/public/alfresco/versions/1/nodes/{fileId}/content?alf_ticket={authToken}";
        }

        public async Task<IEnumerable<string>> SearchFilesAsync(string query)
        {
            var authToken = await GetAuthTokenAsync();
            var url = $"{_alfrescoSecretDto.Uri}/alfresco/api/-default-/public/search/versions/1/search?term={query}&alf_ticket={authToken}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("list").GetProperty("entries")
                .EnumerateArray()
                .Select(e => e.GetProperty("entry").GetProperty("id").GetString()!)
                .ToList();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            if (!string.IsNullOrEmpty(_authToken))
                return _authToken;

            var requestData = new { userId = _alfrescoSecretDto.Username, password = _alfrescoSecretDto.Password };
            var response = await _httpClient.PostAsJsonAsync($"{_alfrescoSecretDto.Uri}/alfresco/api/-default-/public/authentication/versions/1/tickets", requestData);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            _authToken = doc.RootElement.GetProperty("entry").GetProperty("id").GetString();
            return _authToken!;
        }
    }
}