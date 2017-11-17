using System;
using System.Security.Cryptography;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public static class SecurityHash
    {
        public static string GetSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public static string GetHash(string text)
        {
            // SHA512 is disposable by inheritance.
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Get the hashed string.
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static string GetHash1(string text)
        {
            // SHA1 is disposable by inheritance.
            using (var sha1 = SHA1.Create())
            {
                // Send a sample text to hash.
                var hashedBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Get the hashed string.
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static string GetPwdSecurity(string passWord,string salt)
        {
            return GetHash(passWord + salt);
        }
    }
}
