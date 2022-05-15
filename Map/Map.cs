using AxMC_Realms_ME;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace AxMC_Realms_Client.Map
{
    class Map
    {
        public static Point Size;
        public static void Save(byte[] map, int width, string path)
        {
            using (FileStream stream = File.OpenWrite(path + ".json"))
            {
                Utf8JsonWriter writer = new(stream);
                writer.WriteStartObject();
                writer.WritePropertyName("Data");
                JsonSerializer.Serialize(writer, map);
                writer.WriteNumber("width", width);
                writer.WriteEndObject();
                writer.Flush();
                stream.Close();
            }
        }
        public static void Load(string path)
        {
            using (JsonDocument jsonData = JsonDocument.Parse(File.ReadAllText(path + ".json")))
            {
                Game1.byteMap = JsonSerializer.Deserialize<byte[]>(jsonData.RootElement.GetProperty("Data").GetRawText());
                Size.X = jsonData.RootElement.GetProperty("width").GetInt32();
            }
            Size.Y = Game1.byteMap.Length / Size.X;
            Game1.MapTiles = new Tile[Game1.byteMap.Length];
            Game1.MapBlocks = new Vector2[Game1.byteMap.Length];
            for (int i = 0; i < Game1.byteMap.Length; i++)
            {
                byte number = Game1.byteMap[i];
                if (number == 255) continue;
                Game1.MapTiles[i] = new Tile();
                Game1.MapTiles[i].SrcRect.X = 16 * (number % 6);
            }
        }
    }
  //                  Game1.MapTiles[index].SrcRect.X = 16 * (number % 6);
}