using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public static class FileSize
    {
        private const long OneKilobyte = 1024;
        private const long OneMegabyte = 1048576;
        private const long OneGigabyte = 1073741824;

        public static string GetSize(long value)
        {
            if (value <= OneKilobyte)
            {
                return string.Format("{0:0}B", value);
            }
            else if (value <= OneMegabyte)
            {
                return string.Format("{0:0.00}KB", Convert.ToDouble(value) / Convert.ToDouble(OneKilobyte));
            }
            else if (value < OneGigabyte)
            {
                return string.Format("{0:0.00}MB", Convert.ToDouble(value) / Convert.ToDouble(OneMegabyte));
            }
            else
            {
                return string.Format("{0:0.00}GB", Convert.ToDouble(value) / Convert.ToDouble(OneGigabyte));
            }
        }
    }
}
