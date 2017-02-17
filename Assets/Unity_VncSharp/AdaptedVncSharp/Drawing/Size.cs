namespace UnityVncSharp.Drawing
{
    public class Size
    {
        public Size()
        {
            x = y = 0;
        }

        public Size(int X, int Y)
        {
            this.X = 0;
            this.Y = 0;
        }

        int y;
        int x;

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (value < 0)
                    value = 0;

                x = value;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if (value < 0)
                    value = 0;

                y = value;
            }
        }

        public int Width
        {
            get
            {
                return x;
            }
            set
            {
                if (value < 0)
                    value = 0;

                x = value;
            }
        }
        public int Height
        {
            get
            {
                return y;
            }
            set
            {
                if (value < 0)
                    value = 0;

                y = value;
            }
        }
    }

}