using System;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseERP.Core.Common
{
    /// <summary>
    /// Extension methods for common operations
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Generate SHA256 hash with salt for password hashing
        /// </summary>
        public static string HashPassword(this string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = password + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generate random salt for password hashing
        /// </summary>
        public static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        public static bool VerifyPassword(this string password, string storedHash, string salt)
        {
            var computedHash = password.HashPassword(salt);
            return computedHash == storedHash;
        }

        /// <summary>
        /// Truncate string to maximum length
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// Check if string is null or whitespace
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Convert decimal to string with currency format
        /// </summary>
        public static string ToCurrencyString(this decimal value, string currencySymbol = "$")
        {
            return $"{currencySymbol}{value:N2}";
        }

        /// <summary>
        /// Get default value if null
        /// </summary>
        public static T DefaultIfNull<T>(this T? value, T defaultValue) where T : struct
        {
            return value ?? defaultValue;
        }

        /// <summary>
        /// Round decimal to specified places
        /// </summary>
        public static decimal RoundTo(this decimal value, int decimalPlaces = 2)
        {
            return Math.Round(value, decimalPlaces);
        }
    }
}
