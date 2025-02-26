namespace Core.Interfaces.Services
{
    public interface ICryptoService
    {

        /// <summary>
        /// Encrypt text.
        /// </summary>
        /// <param name="plainText">Text to encrypt.</param>
        /// <returns>Encrypt text.</returns>
        public string EncryptString(string plainText);

        /// <summary>
        /// Decrypt text.
        /// </summary>
        /// <param name="cipherText">Cipher text.</param>
        /// <returns>Decrypt text.-</returns>
        public string DecryptString(string cipherText);
    }
}
