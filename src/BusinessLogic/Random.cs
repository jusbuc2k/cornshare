using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public static class SecureRandom
    {
        public static byte[] GetRandomBytes(int length)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buffer = new byte[length];

            rng.GetBytes(buffer);

            return buffer;
        }
    }
}
