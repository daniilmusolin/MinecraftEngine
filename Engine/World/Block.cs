using MinecraftEngine.Engine.Core;

namespace MinecraftEngine.Engine.World;

public class Block {
    public BlockType Type { get; set; }
    public bool IsTransparent => Type == BlockType.Air || Type == BlockType.Leaves || Type == BlockType.Glass || Type == BlockType.Water;
    public bool IsSolid => Type != BlockType.Air;
    public bool IsFullBlock => Type != BlockType.Air && Type != BlockType.Leaves && Type != BlockType.Glass && Type != BlockType.Water;

    public Block(BlockType type) {
        Type = type;
    }

    public (int x, int y) GetTextureUV(BlockFace face) {
        return Type switch {
            BlockType.Grass => face == BlockFace.Top ? (0, 0) : face == BlockFace.Bottom ? (2, 0) : (3, 0),
            BlockType.Dirt => (2, 0),
            BlockType.Stone => (1, 0),
            BlockType.Wood when face == BlockFace.Top || face == BlockFace.Bottom => (4, 0),
            BlockType.Wood => (5, 0),
            BlockType.Leaves => (6, 0),
            BlockType.Sand => (7, 0),
            BlockType.Water => (8, 0),
            BlockType.Bedrock => (9, 0),
            BlockType.Planks => (0, 1),
            BlockType.Cobblestone => (1, 1),
            BlockType.Brick => (2, 1),
            BlockType.Glass => (3, 1),
            BlockType.Snow => (4, 1),
            BlockType.Netherrack => (5, 1),
            BlockType.EndStone => (6, 1),
            BlockType.Obsidian => (7, 1),
            _ => (0, 0)
        };
    }
}
