
using System;

namespace UnityVncSharp.Drawing.Imaging
{
    public enum ImageLockMode
    {
        ReadWrite
        
    }

    public enum PixelFormat
    {
        Format32bppPArgb
    }


    public class Bitmap
    {
        public Bitmap(int w, int h, PixelFormat format)
        {
            Size = new Size(w, h);
            PixelFormat = format;
        }

        public Size Size { get; internal set; }
        public int Width
        {
            get
            {
                return Size.Y;
            }
        }

        internal BitmapData LockBits(Rectangle rectangle, ImageLockMode readWrite, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        internal void UnlockBits(BitmapData bmpd)
        {
            throw new NotImplementedException();
        }
        public PixelFormat PixelFormat
        {
            get; set;
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    
}