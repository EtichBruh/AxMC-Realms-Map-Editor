using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AxMC_Realms_Client.Map
{
    public class Tile
    {
        //public static Texture2D TileSet;
        public static Vector2 SharedPos = Vector2.Zero;
        public static int nextTileSrcPos;
        public Rectangle SrcRect = new(0,0,16,16);
        public Tile(int id)
        {
            SrcRect.X = 16 * (id % 7); // 7 is the amount of tiles on MCRTile
        }
        public Tile()
        {
            SrcRect.X = nextTileSrcPos;
        }
    }
}
