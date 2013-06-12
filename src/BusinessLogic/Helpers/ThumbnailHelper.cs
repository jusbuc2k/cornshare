using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BusinessLogic.Helpers
{
    public class ThumbnailHelper
    {
        public static System.IO.Stream CreateThumbnail(System.IO.Stream stream)
        {
            var ms = new System.IO.MemoryStream();
            var src = Image.FromStream(stream);
            Image thumb = src.GetThumbnailImage(200, 200, () => false, IntPtr.Zero);
            thumb.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            src.Dispose();
            thumb.Dispose();
            ms.Position = 0;
            return ms;
        }
    }
}
