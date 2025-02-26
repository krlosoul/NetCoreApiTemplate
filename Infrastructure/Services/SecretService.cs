namespace Infrastructure.Services
{
    using System.Collections;
    using System.Reflection;
    using System.Text.Json;
    using Core.Interfaces.Services;
    using Microsoft.Extensions.Options;
    using VaultSharp;
    using VaultSharp.Core;
    using VaultSharp.V1.AuthMethods.UserPass;
    using Core.Dtos.AppSettingsDto;

    public class SecretService : ISecretService
    {
        #region Properties
        private readonly VaultAppSettingDto? _vaultDto;
        private readonly IVaultClient _vaultClient;
        #endregion

        public SecretService(IOptions<VaultAppSettingDto> vaultDto)
        {
            _vaultDto = vaultDto.Value;
            if (string.IsNullOrEmpty(_vaultDto?.Username) || string.IsNullOrEmpty(_vaultDto?.Password))throw new ArgumentException($"The parameters are not configured {_vaultDto}");
            var authMethod = new UserPassAuthMethodInfo(_vaultDto?.Username, _vaultDto?.Password);
            var vaultClientSettings = new VaultClientSettings(_vaultDto?.Uri, authMethod);
            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<T> GetSecretAsync<T>(string? secretName) where T : new()
        {
            try
            {
                var secret = await _vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(secretName);
                var dto = new T();
                foreach (var kvp in secret.Data)
                {
                    var property = typeof(T).GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null && property.CanWrite)
                    {
                        try
                        {
                            if (kvp.Value is null || kvp.Value is JsonElement { ValueKind: JsonValueKind.Null })
                            {
                                property.SetValue(dto, null);
                            }
                            else
                            {
                                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                                if (kvp.Value is JsonElement jsonElement)
                                {
                                    var convertedValue = ConvertJsonElement(jsonElement, propertyType);
                                    property.SetValue(dto, convertedValue);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"Expected JsonElement but got {kvp.Value.GetType()}.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error converting property '{kvp.Key}' to type {property.PropertyType}: {ex.Message}", ex);
                        }
                    }
                }

                return dto;
            }
            catch (VaultApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"The secret '{secretName}' was not found in Vault.", ex);
            }
            catch (VaultApiException ex)
            {
                throw new VaultApiException($"Vault API error retrieving secret '{secretName}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while retrieving the secret '{secretName}': {ex.Message}", ex);
            }
        }

        #region Private Method
        private static object? ConvertJsonElement(JsonElement jsonElement, Type propertyType)
        {
            if (propertyType == typeof(List<string>) && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var element in jsonElement.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        list.Add(element.GetString()!);
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported JsonValueKind in array: {element.ValueKind}");
                    }
                }
                return list;
            }

            if (propertyType == typeof(Dictionary<string, string>) && jsonElement.ValueKind == JsonValueKind.Object)
            {
                var dictionary = new Dictionary<string, string>();
                foreach (var property in jsonElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        dictionary[property.Name] = property.Value.GetString()!;
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported JsonValueKind in dictionary value: {property.Value.ValueKind}");
                    }
                }
                return dictionary;
            }
            if (propertyType.IsClass && jsonElement.ValueKind == JsonValueKind.Object)
            {
                return JsonSerializer.Deserialize(jsonElement.GetRawText(), propertyType);
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = propertyType.GetGenericArguments()[0];

                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    var listType = typeof(List<>).MakeGenericType(itemType);
                    var list = (IList)Activator.CreateInstance(listType)!;

                    foreach (var element in jsonElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.String)
                        {
                            var item = JsonSerializer.Deserialize(element.GetRawText(), itemType);
                            list.Add(item);
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported JsonValueKind in list: {element.ValueKind}");
                        }
                    }
                    return list;
                }
            }

            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number when propertyType == typeof(int) => jsonElement.GetInt32(),
                JsonValueKind.Number when propertyType == typeof(long) => jsonElement.GetInt64(),
                JsonValueKind.Number when propertyType == typeof(double) => jsonElement.GetDouble(),
                JsonValueKind.String when propertyType == typeof(string) => jsonElement.GetString(),
                JsonValueKind.True or JsonValueKind.False when propertyType == typeof(bool) => jsonElement.GetBoolean(),
                _ => throw new NotSupportedException($"Unsupported JsonValueKind or type: {jsonElement.ValueKind}, {propertyType}")
            };
        }
        #endregion
    }
}