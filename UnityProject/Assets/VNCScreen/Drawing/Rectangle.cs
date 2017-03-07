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


namespace VNCScreen.Drawing
{
    /// <summary>
    /// This class is an adpatation of the Rectangle from System.Drawing (unavalable in unity)
    /// </summary>
    public class Rectangle 
    {
        public Rectangle(Point pos, Size size)
        {
            this.pos = pos;
            this.size = size;
        }

        public Rectangle(int x, int y, int w, int h)
        {
            pos = new Point(x,y);
            size = new Size(w,h);
        }

        public Rectangle()
        {
            pos = new Point(0, 0);
            size = new Size(0, 0);
        }

        Point pos;
        Size size;

        public override string ToString()
        {
            return pos.X + "-" + pos.Y + " | " + size.X + "x" + size.Y;
        }

        public int Top
        {
            get
            {
                return pos.Y;
            }
            set
            {
                pos.Y = value;
            }
        }

        public int Left
        {
            get
            {
                return pos.X;
            }
            set
            {
                pos.X = value;
            }
        }

        public int Right
        {
            get
            {
                return pos.X + size.X;
            }
            set
            {
                size.X = value - pos.X;
            }
        }
        public int Bottom
        {
            get
            {
                return pos.Y + size.Y;
            }
            set
            {
                size.Y = value - pos.Y;
            }
        }

        public int Height
        {
            get
            {
                return size.Y;
            }

            set
            {
                size.Y = value;
            }
        }
        public int Width
        {
            get
            {
                return size.X;
            }

            set
            {
                size.X = value;
            }
        }

        public int X
        {
            get
            {
                return pos.X;
            }
            set
            {
                pos.X = value;
            }
        }
        public int Y
        {
            get
            {
                return pos.Y;
            }
            set
            {
                pos.Y = value;
            }
        }
    }

}