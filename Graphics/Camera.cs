using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AxMC_Realms_ME.Graphics
{
    public static class Camera
    {
        public static Matrix Transform { get; set; }
        public static Viewport View { get; set; }
        public static float Zoom
        {
            get { return Scale.Translation.Z; }
            set
            {
                Scale *= Matrix.CreateScale(1 + value);
                ScaleFactor = 1f / Scale.M11;
            }
        }
        public static float ScaleFactor = 1;
        private static Matrix Scale = Matrix.CreateScale(1, 1, 1);

        public static Point Position;
        public static Point TPos;
        public static void Init(int width, int height)
        {
            Position.X = 0;
            Position.Y = 0;
        }

        public static void Follow()
        {
            TPos.X = (Position.X - View.Width / 2) / 50;
            TPos.Y = (Position.Y - View.Height / 2) / 50;
            Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0)
            * Scale
            * Matrix.CreateTranslation(View.Width * .5f, View.Height * .5f, 0);
        }
    }
}
