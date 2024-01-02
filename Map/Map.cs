using AxMC_Realms_Client.Map;
using AxMC_Realms_ME;
using AxMC_Realms_ME.Map;
using Microsoft.Xna.Framework;
using System.IO;

namespace nekoT
{
    class Map
    {
        public static Point Size;
        public static void Save(byte[] map, byte[] mapents, int width, string path)
        {
            // Binary Map format i designed :sunglasses:

            using (BinaryWriter bw = new(File.OpenWrite(path + ".bm")))
            {
                bw.Write(width);
                bw.Write(map.Length);
                bw.Write(map);
                bw.Write(mapents);
            }
        }
        public static void Load(string path)
        {
            byte[] entids;
            int maplength = 0;

            using (BinaryReader br = new(File.OpenRead(path + ".bm")))
            {
                Size.X = br.ReadInt32();
                Editor.byteMap = br.ReadBytes(br.ReadInt32());
                maplength = Editor.byteMap.Length;
                entids = br.ReadBytes(maplength);
            }
            Size.Y = maplength / Size.X;
            Editor.MapTiles = new Tile[maplength];
            Editor.Entities = new Entity[maplength];
            Editor.MapBlocks = new Vector2[maplength];
            if (entids.Length > 0)
            {
                for (int i = 0; i < maplength; i++)
                {
                    byte Id = Editor.byteMap[i];
                    byte EntityId = entids[i];

                    if (Id == 255) continue;

                    Editor.MapTiles[i] = new Tile(Id);

                    if (EntityId == 255) continue;

                    Editor.Entities[i] = new(EntityId);
                }
            }
            else
            {
                for (int i = 0; i < maplength; i++)
                {
                    byte id = Editor.byteMap[i];

                    if (id == 255) continue;

                    Editor.MapTiles[i] = new Tile(id);
                }
            }
        }
    }
    //                  Game1.MapTiles[index].SrcRect.X = 16 * (number % 6);
}