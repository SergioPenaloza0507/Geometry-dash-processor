using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using System.IO;

namespace UnofficialEmguCVPackForUnity.Utils
{
    public static class MiscUtils
    {
        public static Texture2D ToTexture2D(this Bitmap b)
        {
            MemoryStream st = new MemoryStream();
            b.Save(st, b.RawFormat);
            var buffer = new byte[st.Length];
            st.Position = 0;
            st.Read(buffer, 0, buffer.Length);
            Texture2D convert = new Texture2D(b.Width, b.Height);
            convert.LoadImage(buffer);
            return convert;
        }
    }
}
