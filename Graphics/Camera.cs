using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMC_Realms_ME.Graphics
{
    public static class Camera
    {
        public static Matrix Transform { get; set; }
        public static Viewport View { get; set; }

        public static float Zoom { get { return Scale.Translation.Z; } set
            {
                Scale *= Matrix.CreateScale(1 + value);
            }
        }
        private static Matrix Scale = Matrix.CreateScale(1,1,1);

        static Vector2 Position;
        public static void Init(int width, int height)
        {
            Position.X = 0;
            Position.Y = 0;
        }

        public static void Follow()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Position.X -= 1 / Scale.M11;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                Position.X += 1 / Scale.M11;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Position.Y -= 1 / Scale.M11;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Position.Y += 1 / Scale.M11;
            }
            Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0)
            * Scale
            * Matrix.CreateTranslation(View.Width * .5f, View.Height * .5f, 0);
        }
    }
}
