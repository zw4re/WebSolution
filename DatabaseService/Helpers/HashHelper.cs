using System.Security.Cryptography;
using System.Text;

namespace DatabaseService.Helpers
{
    public static class HashHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // veriyi hash'le
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // byte dizisini hex string'e çevir
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
