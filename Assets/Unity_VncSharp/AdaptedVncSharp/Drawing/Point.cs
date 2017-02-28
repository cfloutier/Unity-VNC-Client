namespace UnityVncSharp.Drawing
{
    public class Point
    {
        public Point()
        {
            X = Y = 0;
        }

        public Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public int X;
        public int Y;
    }

}