
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
            texture.filterMode = FilterMode.Point;
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
      //      Debug.Log("drawRectangle " + rectangle.ToString());

            Color[] colors =   new Color[rectangle.Width * rectangle.Height];
      
            int pos = 0;

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
            Color[] pSrc = texture.GetPixels(source.X, source.Y, rectangle.Width, rectangle.Height);
            texture.SetPixels(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, pSrc);      
            texture.Apply();
        }




        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}