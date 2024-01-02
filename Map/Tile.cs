using AxMC_Realms_ME;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace AxMC_Realms_Client.Map
{
    public struct TileData
    {
        public ushort Id;
        public string Name;
        public bool Wall;
        public int Frames;
    }
    public class Tile
    {
        public static Rectangle[] SRects = new Rectangle[byte.MaxValue];
        public static int[] AnimatedTiles = new int[byte.MaxValue];
        public static float[] AnimatedTilesTimer = [];
        public static TileData[] Data;
        public static Vector2 SharedPos = Vector2.Zero;
        public static int nextTileSrcPos;
        public Rectangle SrcRect = new(0, 0, 16, 16);

        public static void Initialize(string Path)
        {
            Data = JsonSerializer.Deserialize<TileData[]>(File.ReadAllText(Path), Editor.JsonOptions) ?? Array.Empty<TileData>();

            for (int i = 0; i < byte.MaxValue; i++)
            {
                SRects[i] = new(16 * i, 0, 16, 16);
            }
            int j = 0;
            int newlength = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].Frames > 0)
                {
                    AnimatedTiles[i - j] = i;
                    newlength++;
                }
                else
                    j++;
            }

            AnimatedTilesTimer = new float[newlength];

            Array.Resize(ref AnimatedTiles, newlength);
        }
        public Tile(int id)
        {
            SrcRect.X = 16 * (id % Editor.numTiles); // 7 is the amount of tiles on MCRTile
        }
        public Tile()
        {
            SrcRect.X = nextTileSrcPos;
        }
    }
}
