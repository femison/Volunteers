using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper
{
    private static readonly string Key = "1234567890123456";  // Ключ (должен быть 16 символов для AES-128)
    private static readonly string Iv = "1234567890123456";   // Инициализационный вектор (также 16 символов)

    // Метод для шифрования пароля
    public static string EncryptPassword(string plainPassword)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(Iv);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainPassword);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    // Метод для расшифровки пароля
    public static string DecryptPassword(string cipherPassword)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(Iv);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherPassword)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
