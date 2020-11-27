using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GamesToGo.API.Extensions
{
    public static class HashingExtensions
    {
        public static string SHA256(this string text) => Encoding.UTF8.GetBytes(text).SHA256();
        
        public static string SHA256(this byte[] bytes) // Lo mismo pero SHA256
        {
            using var hasher = new SHA256Managed();
            return hasher.ComputeHash(bytes).ToHashString();
        }
        
        public static string SHA1(this string text) => Encoding.UTF8.GetBytes(text).SHA1();

        public static string SHA1(this byte[] bytes) //Obtiene SHA1 de una secuencia de bytes
        {
            using var hasher = new SHA1Managed();
            return hasher.ComputeHash(bytes).ToHashString();
        }
        
        public static string ToHashString(this byte[] bytes) => string.Concat(bytes.Select(by => by.ToString("X2")));
    }
}