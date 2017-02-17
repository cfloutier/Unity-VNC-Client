
using System;
using UnityEngine;

namespace UnityVncSharp.Drawing.Imaging
{
    public enum ImageLockMode
    {
        ReadWrite
    }


    public class Bitmap
    {
        Texture2D texture;

        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }

        public Bitmap(int w, int h)
        {
            Debug.Log("Build bitmap");


            size = new Size(w, h);
            texture = new Texture2D(w, h, TextureFormat.ARGB32, true);
        }

        Size size;
        public Size Size { get { return size; } }
        public int Width
        {
            get
            {
                return Size.Y;
            }
        }

        Color buildColorFrameARGB(int c)
        {

           
            float a = (float)((c & 0xFF000000) >> 24)/ 255f;
            float r = (float)((c & 0x00FF0000) >> 16) / 255f;
            float g = (float)((c & 0x0000FF00) >> 8) / 255f;
            float b = (float)((c & 0x000000FF) ) / 255f;

            //  a = b = c = 
            if (c != 0)
            {
                a = 1;
            }

            return new Color(r, g, b, a);

       //     return UnityEngine.Random.ColorHSV();
        }

        public virtual void drawRectangle(Rectangle rectangle, Framebuffer framebuffer)
        {
            // Lock the bitmap's scan-lines in RAM so we can iterate over them using pointers and update the area
            // defined in rectangle.
            Color[] colors = new Color[rectangle.Width * rectangle.Height];

            // Move pointer to position in desktop bitmap where rectangle begins
            int pos = 0;// rectangle.Y * Width + rectangle.X;

            int offset = Width - rectangle.Width;
            int row = 0;

            for (int y = 0; y < rectangle.Height; ++y)
            {
                row = y * rectangle.Width;

                for (int x = 0; x < rectangle.Width; ++x)
                {
                    colors[pos++] = buildColorFrameARGB(framebuffer[row + x]);
                }
            }   
        //    texture.GetPixels(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            texture.SetPixels(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, colors);
            texture.Apply();
        }

        public void moveRect(Point source, Rectangle rectangle, Framebuffer framebuffer)
        {
            // Given a source area, copy this region to the point specified by destination
            Color[] pSrc = texture.GetPixels();

            // Avoid exception if window is dragged bottom of screen
            if (rectangle.Top + rectangle.Height >= framebuffer.Height)
            {
                rectangle.Height = framebuffer.Height - rectangle.Top - 1;
            }

            try
            {



                // Calculate the difference between the stride of the desktop, and the pixels we really copied. 
                int nonCopiedPixelStride = Width - rectangle.Width;

                // Move source and destination pointers
                int posSrc = source.Y * Width + source.X;
                int posDest = rectangle.Y * Width + rectangle.X;


                // BUG FIX (Peter Wentworth) EPW:  we need to guard against overwriting old pixels before
                // they've been moved, so we need to work out whether this slides pixels upwards in memeory,
                // or downwards, and run the loop backwards if necessary. 
                if (posDest < posSrc)
                {   // we can copy with pointers that increment
                    for (int y = 0; y < rectangle.Height; ++y)
                    {
                        for (int x = 0; x < rectangle.Width; ++x)
                        {
                            pSrc[posDest++] = pSrc[posSrc++];
                        }

                        // Move pointers to beginning of next row in rectangle
                        posSrc += nonCopiedPixelStride;
                        posDest += nonCopiedPixelStride;
                    }
                }
                else
                {
                    // Move source and destination pointers to just beyond the furthest-from-origin 
                    // pixel to be copied.
                    posSrc += (rectangle.Height * Width) + rectangle.Width;
                    posDest += (rectangle.Height * Width) + rectangle.Width;

                    for (int y = 0; y < rectangle.Height; ++y)
                    {
                        for (int x = 0; x < rectangle.Width; ++x)
                        {
                            pSrc[posDest--] = pSrc[posSrc--];


                        }

                        // Move pointers to end of previous row in rectangle
                        posSrc -= nonCopiedPixelStride;
                        posDest -= nonCopiedPixelStride;
                    }
                }
            }
            finally
            {
                texture.Apply();

            }


        }




        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}