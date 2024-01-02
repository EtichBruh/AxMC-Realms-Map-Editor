using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json;

namespace AxMC_Realms_ME.Map
{
    public enum EntityType
    {
        Entity,
        Enemy,
        Player,
        Portal,
        Container,
        Projectile
    }
    public struct EntityData
    {
        public int Id;
        public string Name;

        public Rectangle Source;
        public Vector2 Origin;

        public int Scale;
        public int Frames;
        public string Destination;
        public int[] Drops;

        public EntityType Type;
        public bool Collides;
    }

    public class Entity
    {
        public static Rectangle[] SRect;
        public static EntityData[] Data;

        public static void Load(string Path)
        {
            Data = JsonSerializer.Deserialize<EntityData[]>(File.ReadAllText(Path), new JsonSerializerOptions() { IncludeFields = true });
            SRect = new Rectangle[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                SRect[i] = Data[i].Source;
            }
        }

        public byte Id;
        //public int SpriteId;// sprite sheet id

        public Entity(byte id)
        {
            Id = id;
        }
    }
}