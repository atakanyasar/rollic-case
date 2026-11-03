using System.Collections.Generic;
using Data.Levels;
using UnityEngine;
namespace Gameplay.Model
{
    public class Level
    {
        public int Id { get; }
        public int TimeLimitSeconds { get; }
        public Grid Grid { get; }
        public Dictionary<int, Block> Blocks { get; } = new();
        public bool IsCompleted => Blocks.Count == 0;

        public Level(LevelData data)
        {
            Id = data.Id;
            TimeLimitSeconds = data.TimeLimitSeconds;
            Grid = new Grid(data.Width, data.Height);

            foreach (var blockData in data.Blocks)
            {
                var block = new Block(blockData.Color, new List<Vector2Int>(blockData.Offsets), blockData.Movement, blockData.IceCount);
                Grid.EmplaceBlock(block, blockData.Position.y, blockData.Position.x);
                Blocks.Add(blockData.Id, block);
            }

            foreach (var doorData in data.Doors)
            {
                Grid.Doors.Add(new Door(doorData.Color, doorData.Side, doorData.Start, doorData.End));
            }
        }

        public bool TryBeginExit(Block block, Vector2Int direction, out Door exitDoor)
        {
            foreach (var door in Grid.Doors)
            {
                if (door.Direction == direction && Grid.CanExit(block, door))
                {
                    block.IsExiting = true;
                    exitDoor = door;
                    return true;
                }
            }

            exitDoor = null;
            return false;
        }

        public void RemoveBlock(Block block)
        {
            foreach (var entry in Blocks)
            {
                if (entry.Value == block)
                {
                    Grid.RemoveBlock(block);
                    block.IsExiting = false;
                    Blocks.Remove(entry.Key);
                    foreach (var remaining in Blocks.Values)
                    {
                        remaining.BreakIce();
                    }
                    return;
                }
            }
        }
    }
}
