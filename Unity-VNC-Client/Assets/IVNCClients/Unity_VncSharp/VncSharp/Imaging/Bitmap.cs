// Unity 3D Vnc Client - Unity 3D VNC Client Library
// Copyright (C) 2017 Christophe Floutier
//
// Based on VncSharp - .NET VNC Client Library
// Copyright (C) 2008 David Humphrey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using UnityEngine;
using VNCScreen.Drawing;

namespace UnityVncSharp.Imaging
{

    /// <summary>
    /// This class is an adpatation of the Bitmap and Image from System.Drawing (unavalable in unity)
    /// </summary>
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

      //      int offset = Width - rectangle.Width;
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