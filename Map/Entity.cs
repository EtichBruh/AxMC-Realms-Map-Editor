using Anomalous.Entities;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public struct EntityData
    {
        public int Id;
        public string Name;

        public Rectangle Source;
        public Vector2 Origin;

        public int Scale;
        public int Frames;
        public string Destination;
        public Loot[] Drops;

        public EntityType Type;
        public bool Collides;
    }

    public class Entity
    {
        public static Rectangle[] SRect;
        public static float[] Scale;
        public static EntityData[] Data;

        public static void Load(string Path)
        {
            Data = JsonSerializer.Deserialize<EntityData[]>(File.ReadAllText(Path), new JsonSerializerOptions() { IncludeFields = true });
            SRect = new Rectangle[Data.Length];
            Scale = new float[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                var rect = SRect[i] = Data[i].Source;
                Scale[i] = MathF.Max(16f / rect.Width, 16f / rect.Height);
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