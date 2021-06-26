namespace CUESaber.CueSaber.Native.Logitech
{
    internal class Rectangle
    {
        public int x { get; }
        public int y { get; }
        public int width { get; }
        public int height { get; }

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
}