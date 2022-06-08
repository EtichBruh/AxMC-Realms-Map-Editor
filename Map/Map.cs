using AxMC_Realms_Client.Map;
using AxMC_Realms_ME;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace nekoT
{
    class Map
    {
        public static Point Size;
        public static void Save(byte[] map, byte[] mapents, int width, string path)
        {
            using (FileStream stream = File.OpenWrite(path + ".json"))
            {
                Utf8JsonWriter writer = new(stream);
                writer.WriteStartObject();
                writer.WritePropertyName("Data");
                JsonSerializer.Serialize(writer, map);
                writer.WritePropertyName("Entities");
                JsonSerializer.Serialize(writer, mapents);
                writer.WriteNumber("width", width);
                writer.WriteEndObject();
                writer.Flush();
                stream.Close();
            }
        }
        public static void Load(string path)
        {
            byte[] entids;
            using (JsonDocument jsonData = JsonDocument.Parse(File.ReadAllText(path + ".json")))
            {
                Game1.byteMap = JsonSerializer.Deserialize<byte[]>(jsonData.RootElement.GetProperty("Data").GetRawText());
                try
                {
                    entids = JsonSerializer.Deserialize<byte[]>(jsonData.RootElement.GetProperty("Entities").GetRawText());
                }
                catch { entids = Array.Empty<byte>(); }
                Size.X = jsonData.RootElement.GetProperty("width").GetInt32();
            }
            Size.Y = Game1.byteMap.Length / Size.X;
            Game1.MapTiles = new Tile[Game1.byteMap.Length];
            Game1.Entities = new AxMC_Realms_ME.Map.Entity[Game1.byteMap.Length];
            Game1.MapBlocks = new Vector2[Game1.byteMap.Length];
            if (entids.Length > 0)
            {
                for (int i = 0; i < Game1.byteMap.Length; i++)
                {
                    byte Id = Game1.byteMap[i];
                    byte EntityId = entids[i];
                    if (Id == 255) continue;
                    Game1.MapTiles[i] = new Tile(Id);
                    if (EntityId == 255) continue;
                    Game1.Entities[i] = new(EntityId);
                }
            }
            else
            {
                for (int i = 0; i < Game1.byteMap.Length; i++)
                {
                    byte id = Game1.byteMap[i];
                    if (id == 255) continue;
                    Game1.MapTiles[i] = new Tile(id);
                }
            }
        }
    }
  //                  Game1.MapTiles[index].SrcRect.X = 16 * (number % 6);
}