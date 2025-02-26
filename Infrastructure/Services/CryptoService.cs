using System;

namespace Infrastructure.Services
{
    using Core.Dtos.SecretsDto;
    using Core.Interfaces.Services;
    using System.Security.Cryptography;
    using System.Text;

    public class CryptoService : ICryptoService
    {
        #region Parameters
        private readonly CryptoSecretDto? _cryptoSecretDto;
        private readonly byte[] _key;
        #endregion

        public CryptoService(CryptoSecretDto cryptoSecretDto)
        {
            _cryptoSecretDto = cryptoSecretDto;
            if (string.IsNullOrWhiteSpace(_cryptoSecretDto.Key)) throw new ArgumentNullException(nameof(_cryptoSecretDto.Key));
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(_cryptoSecretDto.Key));
        }

        public string EncryptString(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }
            byte[] iv = aes.IV;
            byte[] encryptedContent = ms.ToArray();
            byte[] result = new byte[iv.Length + encryptedContent.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);
            return Convert.ToBase64String(result);
        }

        public string DecryptString(string cipherText)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using Aes aes = Aes.Create();
            aes.Key = _key;
            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);
            using MemoryStream ms = new(cipher);
            using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}