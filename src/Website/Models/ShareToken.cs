using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public struct ShareToken
    {
        public static ShareToken Parse(string base64)
        {
            return new ShareToken(Convert.FromBase64String(Decode(base64)));
        }

        private static string Encode(string value)
        {
            return value.Replace('/', '-').Replace('+', '_');
        }

        private static string Decode(string value)
        {
            return value.Replace('-', '/').Replace('_', '+');
        }

        public ShareToken(byte[] randomBytes, int fileID)
        {
            this._buffer = new byte[24];
            this.RandomBytes = randomBytes;
            this.FileID = fileID;
        }

        public ShareToken(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentException("buffer cannot be null", "buffer");

            if (buffer.Length != 24)
                throw new ArgumentException("buffer must be exactly 24 bytes in length", "buffer");

            _buffer = buffer;
        }

        private byte[] _buffer;

        public byte[] RandomBytes 
        {
            get{
                var bytes = new byte[20];
                Array.Copy(this._buffer, bytes, 20);
                return bytes;
            }
            set{
                Array.Copy(value, this._buffer, 20);
            }
        }

        public int FileID
        {
            get
            {
                return BitConverter.ToInt32(this._buffer, 20);
            }
            set
            {
                BitConverter.GetBytes(value).CopyTo(this._buffer, 20);
            }
        }
        
        public string ToBase64String()
        {
            return Encode(Convert.ToBase64String(_buffer));
        }

        public byte[] GetBytes()
        {
            return this._buffer;
        }
    }
}