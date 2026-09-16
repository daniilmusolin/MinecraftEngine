using MinecraftEngine.Engine.Core;
using System.Collections.Concurrent;

namespace MinecraftEngine.Engine.World;

public class BlockRegistry {
    private static BlockRegistry? _instance;
    private static readonly object _lock = new();

    public static BlockRegistry Instance {
        get {
            if (_instance == null) {
                lock (_lock) {
                    _instance ??= new BlockRegistry();
                }
            }
            return _instance;
        }
    }

    private readonly ConcurrentDictionary<BlockType, BlockData> _blocks = new();
    private readonly ConcurrentDictionary<string, BlockType> _nameToType = new();

    private BlockRegistry() {
        RegisterAll();
    }

    private void RegisterAll() {
        Register(BlockType.Air, "Air", false, false);
        Register(BlockType.Grass, "Grass", true, true);
        Register(BlockType.Dirt, "Dirt", true, true);
        Register(BlockType.Stone, "Stone", true, true);
        Register(BlockType.Wood, "Wood", true, true);
        Register(BlockType.Leaves, "Leaves", true, false);
        Register(BlockType.Sand, "Sand", true, true);
        Register(BlockType.Water, "Water", true, false);
        Register(BlockType.Bedrock, "Bedrock", true, true);
        Register(BlockType.Planks, "Planks", true, true);
        Register(BlockType.Cobblestone, "Cobblestone", true, true);
        Register(BlockType.Brick, "Brick", true, true);
        Register(BlockType.Glass, "Glass", true, false);
        Register(BlockType.Snow, "Snow", true, true);
        Register(BlockType.Netherrack, "Netherrack", true, true);
        Register(BlockType.EndStone, "EndStone", true, true);
        Register(BlockType.Obsidian, "Obsidian", true, true);
    }

    public void Register(BlockType type, string name, bool isSolid, bool isFullBlock) {
        var data = new BlockData {
            Type = type,
            Name = name,
            IsSolid = isSolid,
            IsFullBlock = isFullBlock,
            IsTransparent = !isSolid || type == BlockType.Leaves || type == BlockType.Glass || type == BlockType.Water
        };
        _blocks[type] = data;
        _nameToType[name] = type;
    }

    public BlockData GetData(BlockType type) {
        return _blocks.TryGetValue(type, out var data) ? data : default;
    }

    public BlockType GetType(string name) {
        return _nameToType.TryGetValue(name, out var type) ? type : BlockType.Air;
    }

    public bool IsSolid(BlockType type) => GetData(type).IsSolid;
    public bool IsTransparent(BlockType type) => GetData(type).IsTransparent;
    public bool IsFullBlock(BlockType type) => GetData(type).IsFullBlock;

    public IEnumerable<BlockType> GetBlockTypes() => _blocks.Keys;
    public IEnumerable<BlockData> GetBlockData() => _blocks.Values;
}