namespace UnityVncSharp.Drawing
{
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