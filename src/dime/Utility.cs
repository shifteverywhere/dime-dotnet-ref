//
//  Utility.cs
//  Di:ME - Digital Identity Message Envelope
//  A secure and compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2022 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

namespace DiME
{
    /// <summary>
    /// Utility support methods.
    /// </summary>
    public static class Utility
    {

        private static readonly RNGCryptoServiceProvider RngCsp = new();
        
        /// <summary>
        /// Will generates random bytes.
        /// </summary>
        /// <param name="size">The number of bytes to generate.</param>
        /// <returns>A byte array with the generated bytes.</returns>
        public static byte[] RandomBytes(int size)
        {
            var value = new byte[size];
            RngCsp.GetBytes(value);
            return value;
        }

        /// <summary>
        /// Encode a byte array as a hexadecimal string.
        /// </summary>
        /// <param name="bytes">Byte array to encode.</param>
        /// <returns>Hexadecimal string.</returns>
        public static string ToHex(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                hex.Append($"{b:x2}");
            return hex.ToString();
        }

        /// <summary>
        /// Encode a byte array as a base 64 string.
        /// </summary>
        /// <param name="bytes">Byte array to encode.</param>
        /// <returns>Base 64 encoded string.</returns>
        public static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).Trim('=');
        }

        /// <summary>
        /// Encode a string as base 64.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <returns>Base 64 encoded string.</returns>
        public static string ToBase64(string str)
        {
            return ToBase64(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Decode a base 64 encoded string.
        /// </summary>
        /// <param name="base64">String to decode.</param>
        /// <returns>Decoded byte array.</returns>
        public static byte[] FromBase64(String base64)
        {
            var str = base64;
            str = str.Replace('_', '/').Replace('-', '+');
            var padding = base64.Length % 4;
            if (padding > 1)
            {
                str += padding == 2 ? "==" : "=";
            }
            return Convert.FromBase64String(str);
        }

        /// <summary>
        /// Combine two byte arrays.
        /// </summary>
        /// <param name="first">First byte array.</param>
        /// <param name="second">Second byte array.</param>
        /// <returns>First + second combined.</returns>
        public static byte[] Combine(byte[] first, byte[] second)   
        {
            var bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        /// <summary>
        /// Extract a sub-array from a byte array.
        /// </summary>
        /// <param name="array">The original byte array.</param>
        /// <param name="start">The start position in of the sub-array in the original array.</param>
        /// <param name="length">The length of the sub-array.</param>
        /// <returns>The extracted sub-array.</returns>
        public static byte[] SubArray(byte[] array, int start, int length)
		{
			var bytes = new byte[length];
			Buffer.BlockCopy(array, start, bytes, 0, length);
			return bytes;
		}

        /// <summary>
        /// Extract a sub-array from a byte array.
        /// </summary>
        /// <param name="array">The original byte array.</param>
        /// <param name="start">The start position in of the sub-array in the original array.</param>
        /// <returns>The extracted sub-array.</returns>
		public static byte[] SubArray(byte[] array, int start)
		{
			return SubArray(array, start, array.Length - start);
		}

        /// <summary>
        /// Prefixes a byte to a byte array.
        /// </summary>
        /// <param name="prefix">The byte to prefix.</param>
        /// <param name="array">The byte array to prefix to.</param>
        /// <returns>A byte array with a prefix.</returns>
        public static byte[] Prefix(byte prefix, byte[] array)
        {
            var bytes = new byte[array.Length + 1];
            array.CopyTo(bytes, 1);
            bytes[0] = prefix;
            return bytes;
        }
        
        /// <summary>
        /// Format as a RFC 3339 date.
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>A string with a RF 3339 formatted date.</returns>
        public static string ToTimestamp(DateTime date)
        {
            return XmlConvert.ToString(date, XmlDateTimeSerializationMode.Utc);
        }

        /// <summary>
        /// Parse RFC 3339 to a DateTime object.
        /// </summary>
        /// <param name="timestamp">The date to parse.</param>
        /// <returns>A DateTime object.</returns>
        public static DateTime FromTimestamp(string timestamp)  
        {
            return DateTime.Parse(timestamp).ToUniversalTime();
        } 

    }

}
