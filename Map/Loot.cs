using System;

namespace Anomalous.Entities
{
    public struct Loot
    {
        public ushort ItemId;
        public float Chance;

        public readonly bool Roll()
        {
            return Random.Shared.NextSingle() < Chance;
        }
    }
}
