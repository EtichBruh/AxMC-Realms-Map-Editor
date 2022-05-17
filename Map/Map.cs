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
                entids = JsonSerializer.Deserialize<byte[]>(jsonData.RootElement.GetProperty("Entities").GetRawText());
                Size.X = jsonData.RootElement.GetProperty("width").GetInt32();
            }
            Size.Y = Game1.byteMap.Length / Size.X;
            Game1.MapTiles = new Tile[Game1.byteMap.Length];
            Game1.Entities = new AxMC_Realms_ME.Map.Entity[Game1.byteMap.Length];
            Game1.MapBlocks = new Vector2[Game1.byteMap.Length];
            for (int i = 0; i < Game1.byteMap.Length; i++)
            {
                byte number = Game1.byteMap[i];
                byte entity = entids[i];
                if (number == 255) continue;
                Game1.MapTiles[i] = new Tile();
                Game1.MapTiles[i].SrcRect.X = 16 * (number % 6);
                if (entity == 255) continue;
                Game1.Entities[i] = new()
                {
                    id = entity,
                    SpriteId = entity < 1 ? 0 : 1
                };
            }
        }
    }
  //                  Game1.MapTiles[index].SrcRect.X = 16 * (number % 6);
}