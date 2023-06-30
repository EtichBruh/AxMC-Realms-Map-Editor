using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AxMC_Realms_ME.Map
{
    public class Entity
    {
        public static Texture2D SpriteSheets;
        public static Rectangle[] SRect = {
            new (0,0,8,9),
            new (8,0,8,8),
            new (16,0,9,8),
            new (25,0,6,9),
            new (31,0,7,11),
            new (38,0,11,13),
            new (49,0,12,15),
        };
        public byte Id;
        //public int SpriteId;// sprite sheet id

        public Entity(byte id)
        {
            Id = id;
        }
    }
}