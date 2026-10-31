using System.Collections.Generic;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay
{
    public class Level
    {
        public int Id { get; }
        public int TimeLimitSeconds { get; }
        public Grid Grid { get; }
        public Dictionary<int, Block> Blocks { get; } = new();

        public Level(LevelData data)
        {
            Id = data.Id;
            TimeLimitSeconds = data.TimeLimitSeconds;
            Grid = new Grid(data.Width, data.Height);

            foreach (var blockData in data.Blocks)
            {
                var block = new Block(blockData.Color, new List<Vector2Int>(blockData.Offsets));
                Grid.EmplaceBlock(block, blockData.Position.y, blockData.Position.x);
                Blocks.Add(blockData.Id, block);
            }

            foreach (var doorData in data.Doors)
            {
                Grid.Doors.Add(new Door(doorData.Color, doorData.Side, doorData.Start, doorData.End));
            }
        }
    }
}
