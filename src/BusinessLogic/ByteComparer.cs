using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLogic
{
    public class ByteComparer
    {
        public static bool AreEqual(byte[] a, byte[] b)
        {
            return ByteComparerInternal.AreEqual(a, b);
        }        
    }
}
