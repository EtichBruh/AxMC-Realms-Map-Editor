using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMC_Realms_ME.Map
{
    public class Entity
    {
        public static Texture2D[] SpriteSheets = new Texture2D[2];
        public static Rectangle[] SRect = { new Rectangle(0,0,9,9), new Rectangle(0,0,8,8), new Rectangle(8,0,9,8) };
        public byte Id;
        public int SpriteId;// sprite sheet id

        public Entity(byte id)
        {
            Id = id;
            SpriteId = id < 1 ? 0 : 1;
        }
    }
}