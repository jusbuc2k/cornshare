using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLogic
{
    internal class ByteComparerInternal
    {
        public static bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            if (a.Length == 0)
                return true;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }       
    }
}
