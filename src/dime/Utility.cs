//
//  Utility.cs
//  DiME - Digital Identity Message Envelope
//  A secure and compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;

namespace ShiftEverywhere.DiME
{
    public static class Utility
    {
        public static String ToHex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string ToBase64(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes).Trim('=');
        }

        public static string ToBase64(string str)
        {
            return Utility.ToBase64(Encoding.UTF8.GetBytes(str));
        }

        public static byte[] FromBase64(String base64)
        {
            string str = base64;
            str = str.Replace('_', '/').Replace('-', '+');
            int padding = base64.Length % 4;
            if (padding > 1)
            {
                str += padding == 2 ? "==" : "=";
            }
            return System.Convert.FromBase64String(str);
        }

        public static byte[] Combine(byte[] first, byte[] second)   
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static byte[] SubArray(byte[] array, int start, int length)
		{
			byte[] bytes = new byte[length];
			Buffer.BlockCopy(array, start, bytes, 0, length);
			return bytes;
		}

		public static byte[] SubArray(byte[] array, int start)
		{
			return Utility.SubArray(array, start, array.Length - start);
		}


    }

}
