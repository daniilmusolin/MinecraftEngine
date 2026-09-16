namespace MinecraftEngine.Engine.Core;

public enum BlockType {
    Air = 0,
    Grass = 1,
    Dirt = 2,
    Stone = 3,
    Wood = 4,
    Leaves = 5,
    Sand = 6,
    Water = 7,
    Bedrock = 8,
    Planks = 9,
    Cobblestone = 10,
    Brick = 11,
    Glass = 12,
    Snow = 13,
    Netherrack = 14,
    EndStone = 15,
    Obsidian = 16
}

public static class BlockInfo {
    public static string GetName(BlockType type) {
        return type switch {
            BlockType.Grass => "Grass",
            BlockType.Dirt => "Dirt",
            BlockType.Stone => "Stone",
            BlockType.Wood => "Wood",
            BlockType.Leaves => "Leaves",
            BlockType.Sand => "Sand",
            BlockType.Water => "Water",
            BlockType.Bedrock => "Bedrock",
            BlockType.Planks => "Planks",
            BlockType.Cobblestone => "Cobblestone",
            BlockType.Brick => "Brick",
            BlockType.Glass => "Glass",
            BlockType.Snow => "Snow",
            BlockType.Netherrack => "Netherrack",
            BlockType.EndStone => "EndStone",
            BlockType.Obsidian => "Obsidian",
            _ => "Air"
        };
    }

    public static bool IsSolid(BlockType type) {
        return type != BlockType.Air && type != BlockType.Water;
    }

    public static bool IsTransparent(BlockType type) {
        return type == BlockType.Air || type == BlockType.Water || type == BlockType.Leaves || type == BlockType.Glass;
    }
}