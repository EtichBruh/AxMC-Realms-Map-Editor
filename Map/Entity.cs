using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AxMC_Realms_ME.Map
{
    public class Entity
    {
        public static Texture2D SpriteSheets;
        public static Rectangle[] SRect = {
            new Rectangle(0,0,8,9),
            new Rectangle(8,0,8,8),
            new Rectangle(16,0,9,8),
            new Rectangle(25,0,6,9),
            new Rectangle(31,0,7,11),
            new Rectangle(38,0,11,13)};
        public byte Id;
        //public int SpriteId;// sprite sheet id

        public Entity(byte id)
        {
            Id = id;
        }
    }
}