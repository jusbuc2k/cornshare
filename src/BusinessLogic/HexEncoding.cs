using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BusinessLogic
{
    /// <summary>
    /// Provides hexidecimal encoding methods
    /// </summary>
    public static class HexEncoding
    {
        /// <summary>
        /// Parses the specified hex encoded representation of a byte array.
        /// </summary>
        /// <param name="s">A delmited or non-delimited Hex encoded byte array.</param>
        /// <returns></returns>
        public static byte[] GetBytes(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new InvalidOperationException("s cannot be null or empty.");

            char dash = '-';
            using (var ms = new MemoryStream(s.Length))
            {
                string b;
                for (int x = 0; x < s.Length; x+=2 )
                {
                    if (s[x] == dash)
                        continue;
                    b = s.Substring(x, 2);
                    ms.WriteByte(byte.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier));
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts the specified byte array to a hex encoded string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="dashed"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes, bool dashed)
        {
            if (bytes == null)
                throw new NullReferenceException("bytes cannot be null.");
            if (bytes.Length <= 0)
                throw new InvalidOperationException("bytes cannot be zero length.");

            char dash = '-';
            bool first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
            {
                if (!first && dashed)
                    sb.Append(dash);
                sb.AppendFormat("{0:x2}", b);
                first = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts the specified by array to a hex encoded string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes)
        {
            return GetString(bytes, false);
        }

        public static string ToHex(this byte[] buffer)
        {
            return HexEncoding.GetString(buffer);
        }
    }
}
