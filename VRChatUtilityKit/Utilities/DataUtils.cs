using System;
using System.Security.Cryptography;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of utilities for data.
    /// </summary>
    public static class DataUtils
    {
        /// <summary>
        /// Returns the SHA256 hash of the given byte array.
        /// </summary>
        /// <param name="data">The byte array to get the hash of</param>
        public static string CalculateSHA256Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
                return Convert.ToBase64String(sha256.ComputeHash(data));
        }
    }
}
