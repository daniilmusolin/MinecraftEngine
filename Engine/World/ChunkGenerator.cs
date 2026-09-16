using MinecraftEngine.Engine.Core;
using MinecraftEngine.Engine.Terrain;

namespace MinecraftEngine.Engine.World;

public class ChunkGenerator {
    private readonly SimplexNoise _heightNoise;
    private readonly SimplexNoise _detailNoise;
    private readonly SimplexNoise _caveNoise;
    private readonly SimplexNoise _treeNoise;
    private readonly Random _random;

    public ChunkGenerator() {
        var seed = new Random().Next();
        _heightNoise = new SimplexNoise(seed);
        _detailNoise = new SimplexNoise(seed + 1);
        _caveNoise = new SimplexNoise(seed + 2);
        _treeNoise = new SimplexNoise(seed + 3);
        _random = new Random(seed);
    }

    public BlockType[,,] GenerateChunk(int chunkX, int chunkZ) {
        var blocks = new BlockType[Chunk.Size, Chunk.Height, Chunk.Size];

        for (var x = 0; x < Chunk.Size; x++) {
            for (var z = 0; z < Chunk.Size; z++) {
                var worldX = chunkX * Chunk.Size + x;
                var worldZ = chunkZ * Chunk.Size + z;

                var height = GetHeight(worldX, worldZ);
                var terrainType = GetTerrainType(worldX, worldZ, height);

                for (var y = 0; y < Chunk.Height; y++) {
                    if (y == 0) {
                        blocks[x, y, z] = BlockType.Bedrock;
                        continue;
                    }

                    if (y > height) {
                        blocks[x, y, z] = BlockType.Air;
                        continue;
                    }

                    // Пещеры
                    if (IsCave(worldX, y, worldZ)) {
                        blocks[x, y, z] = BlockType.Air;
                        continue;
                    }

                    // Слои
                    if (y == height) {
                        blocks[x, y, z] = terrainType;
                    } else if (y > height - 4) {
                        blocks[x, y, z] = BlockType.Dirt;
                    } else {
                        blocks[x, y, z] = BlockType.Stone;
                    }
                }
            }
        }

        // Деревья
        GenerateTrees(blocks, chunkX, chunkZ);

        return blocks;
    }

    private int GetHeight(int x, int z) {
        // Основной рельеф
        var h1 = _heightNoise.Fractal(x * 0.005f, z * 0.005f, 4) * 50f + 64f;
        // Детали
        var h2 = _detailNoise.Fractal(x * 0.015f, z * 0.015f, 3) * 15f;
        // Горы
        var h3 = _heightNoise.Fractal(x * 0.001f, z * 0.001f, 2) * 30f;

        var height = h1 + h2 + h3;

        // Биомы
        var biome = _heightNoise.Fractal(x * 0.002f, z * 0.002f, 2);
        if (biome < -0.3f) height = 30f + h2 * 0.3f;      // Океан
        else if (biome < -0.1f) height = 55f + h2 * 0.5f; // Равнина
        else if (biome < 0.2f) height = 70f + h2 * 0.7f;  // Холмы
        else height = 100f + h3 * 0.5f;                   // Горы

        return (int)Math.Clamp(height, 5, 150);
    }

    private BlockType GetTerrainType(int x, int z, int height) {
        var noise = _heightNoise.Fractal(x * 0.01f, z * 0.01f, 2);

        if (height < 35) return BlockType.Sand;
        if (height < 40) return BlockType.Grass;

        return noise switch {
            < -0.2f when height > 50 => BlockType.Sand,
            < 0.1f => BlockType.Grass,
            < 0.3f => BlockType.Stone,
            _ => BlockType.Grass
        };
    }

    private bool IsCave(int x, int y, int z) {
        var noise = _caveNoise.Fractal(x * 0.04f, y * 0.04f, z * 0.04f, 3, 2.5f, 0.5f);
        var threshold = 0.3f + (y - 20f) / 100f * 0.1f;
        return noise > threshold && y > 8 && y < 60;
    }

    private void GenerateTrees(BlockType[,,] blocks, int chunkX, int chunkZ) {
        for (var x = 2; x < Chunk.Size - 2; x += 3) {
            for (var z = 2; z < Chunk.Size - 2; z += 3) {
                var worldX = chunkX * Chunk.Size + x;
                var worldZ = chunkZ * Chunk.Size + z;

                var treeNoise = _treeNoise.Fractal(worldX * 0.1f, worldZ * 0.1f, 1);
                if (treeNoise < 0.4f) continue;

                var height = 0;
                for (var y = Chunk.Height - 1; y >= 0; y--) {
                    if (blocks[x, y, z] != BlockType.Air) {
                        height = y;
                        break;
                    }
                }

                if (height > 0 && blocks[x, height, z] == BlockType.Grass) {
                    GenerateTree(blocks, x, height + 1, z);
                }
            }
        }
    }

    private void GenerateTree(BlockType[,,] blocks, int x, int y, int z) {
        var treeHeight = 4 + _random.Next(4);

        for (var i = 0; i < treeHeight; i++) {
            if (y + i < Chunk.Height) {
                blocks[x, y + i, z] = BlockType.Wood;
            }
        }

        var radius = 2;
        for (var dx = -radius; dx <= radius; dx++) {
            for (var dz = -radius; dz <= radius; dz++) {
                for (var dy = -radius; dy <= radius; dy++) {
                    if (dx * dx + dy * dy + dz * dz > radius * radius + 1) continue;

                    var lx = x + dx;
                    var ly = y + treeHeight - 1 + dy;
                    var lz = z + dz;

                    if (lx >= 0 && lx < Chunk.Size && ly >= 0 && ly < Chunk.Height && lz >= 0 && lz < Chunk.Size) {
                        if (blocks[lx, ly, lz] == BlockType.Air) {
                            blocks[lx, ly, lz] = BlockType.Leaves;
                        }
                    }
                }
            }
        }
    }
}