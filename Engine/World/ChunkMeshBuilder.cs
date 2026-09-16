using OpenTK.Mathematics;
using MinecraftEngine.Engine.Graphics;
using MinecraftEngine.Engine.Core;

namespace MinecraftEngine.Engine.World;

public class ChunkMeshBuilder {
    private readonly List<float> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<float> _uvs = new();
    private readonly List<float> _normals = new();
    private int _vertexCount;

    public Mesh BuildMesh(Chunk chunk, int chunkX, int chunkZ) {
        _vertices.Clear();
        _indices.Clear();
        _uvs.Clear();
        _normals.Clear();
        _vertexCount = 0;

        var size = Chunk.Size;
        var height = Chunk.Height;

        for (var x = 0; x < size; x++) {
            for (var y = 0; y < height; y++) {
                for (var z = 0; z < size; z++) {
                    var block = chunk.GetBlock(x, y, z);
                    if (block == BlockType.Air) continue;

                    var faces = GetVisibleFaces(chunk, x, y, z);
                    if (faces.Count == 0) continue;

                    foreach (var face in faces) {
                        AddFace(x, y, z, face, block);
                    }
                }
            }
        }

        if (_vertexCount == 0) {
            return new Mesh();
        }

        var vertexData = new float[_vertices.Count + _uvs.Count + _normals.Count];
        for (var i = 0; i < _vertexCount; i++) {
            var idx = i * 8;
            vertexData[idx] = _vertices[i * 3];
            vertexData[idx + 1] = _vertices[i * 3 + 1];
            vertexData[idx + 2] = _vertices[i * 3 + 2];
            vertexData[idx + 3] = _uvs[i * 2];
            vertexData[idx + 4] = _uvs[i * 2 + 1];
            vertexData[idx + 5] = _normals[i * 3];
            vertexData[idx + 6] = _normals[i * 3 + 1];
            vertexData[idx + 7] = _normals[i * 3 + 2];
        }

        return new Mesh(vertexData, _indices.ToArray());
    }

    private List<BlockFace> GetVisibleFaces(Chunk chunk, int x, int y, int z) {
        var faces = new List<BlockFace>();

        if (IsBlockVisible(chunk, x, y + 1, z)) faces.Add(BlockFace.Top);
        if (IsBlockVisible(chunk, x, y - 1, z)) faces.Add(BlockFace.Bottom);
        if (IsBlockVisible(chunk, x, y, z + 1)) faces.Add(BlockFace.Front);
        if (IsBlockVisible(chunk, x, y, z - 1)) faces.Add(BlockFace.Back);
        if (IsBlockVisible(chunk, x + 1, y, z)) faces.Add(BlockFace.Right);
        if (IsBlockVisible(chunk, x - 1, y, z)) faces.Add(BlockFace.Left);

        return faces;
    }

    private bool IsBlockVisible(Chunk chunk, int x, int y, int z) {
        var block = chunk.GetBlock(x, y, z);
        return block == BlockType.Air || block == BlockType.Leaves || block == BlockType.Glass || block == BlockType.Water;
    }

    private void AddFace(float x, float y, float z, BlockFace face, BlockType blockType) {
        var vertices = GetFaceVertices(face);
        var (tx, ty) = GetBlockTexture(blockType, face);
        var normal = GetFaceNormal(face);

        var atlasSize = 16f;
        var u1 = tx / atlasSize;
        var v1 = ty / atlasSize;
        var u2 = (tx + 1) / atlasSize;
        var v2 = (ty + 1) / atlasSize;

        Vector2[] uvs = GetFaceUVs(face, u1, v1, u2, v2);

        // Добавляем вершины
        for (var i = 0; i < vertices.Length; i++) {
            _vertices.Add(x + vertices[i].X);
            _vertices.Add(y + vertices[i].Y);
            _vertices.Add(z + vertices[i].Z);
            _uvs.Add(uvs[i].X);
            _uvs.Add(uvs[i].Y);
            _normals.Add(normal.X);
            _normals.Add(normal.Y);
            _normals.Add(normal.Z);
        }

        // ИСПРАВЛЕННЫЕ ИНДЕКСЫ - два треугольника образуют квадрат
        // Первый треугольник: 0-1-2, Второй: 0-2-3
        var baseIndex = _vertexCount;
        _indices.Add(baseIndex + 0);
        _indices.Add(baseIndex + 2);
        _indices.Add(baseIndex + 1);
        _indices.Add(baseIndex + 0);
        _indices.Add(baseIndex + 3);
        _indices.Add(baseIndex + 2);
        _vertexCount += 4;
    }

    private Vector3[] GetFaceVertices(BlockFace face) {
        // ВСЕ ВЕРШИНЫ ИДУТ ПО ЧАСОВОЙ СТРЕЛКЕ (для CW winding)
        // или ПРОТИВ ЧАСОВОЙ (для CCW winding)
        // Я использую ПРОТИВ ЧАСОВОЙ СТРЕЛКИ (CCW) - стандарт OpenGL
        return face switch {
            // Верх (+Y) - смотрим сверху вниз, против часовой
            BlockFace.Top => new[] {
                new Vector3(0, 1, 1),  // 0
                new Vector3(0, 1, 0),  // 1
                new Vector3(1, 1, 0),  // 2
                new Vector3(1, 1, 1)   // 3
            },
            // Низ (-Y) - смотрим снизу вверх, против часовой
            BlockFace.Bottom => new[] {
                new Vector3(0, 0, 0),  // 0
                new Vector3(0, 0, 1),  // 1
                new Vector3(1, 0, 1),  // 2
                new Vector3(1, 0, 0)   // 3
            },
            // Перед (+Z) - смотрим спереди, против часовой
            BlockFace.Front => new[] {
                new Vector3(0, 0, 1),  // 0
                new Vector3(0, 1, 1),  // 1
                new Vector3(1, 1, 1),  // 2
                new Vector3(1, 0, 1)   // 3
            },
            // Зад (-Z) - смотрим сзади, против часовой
            BlockFace.Back => new[] {
                new Vector3(1, 0, 0),  // 0
                new Vector3(1, 1, 0),  // 1
                new Vector3(0, 1, 0),  // 2
                new Vector3(0, 0, 0)   // 3
            },
            // Право (+X) - смотрим справа, против часовой
            BlockFace.Right => new[] {
                new Vector3(1, 0, 1),  // 0
                new Vector3(1, 1, 1),  // 1
                new Vector3(1, 1, 0),  // 2
                new Vector3(1, 0, 0)   // 3
            },
            // Лево (-X) - смотрим слева, против часовой
            BlockFace.Left => new[] {
                new Vector3(0, 0, 0),  // 0
                new Vector3(0, 1, 0),  // 1
                new Vector3(0, 1, 1),  // 2
                new Vector3(0, 0, 1)   // 3
            },
            _ => new Vector3[0]
        };
    }

    private Vector2[] GetFaceUVs(BlockFace face, float u1, float v1, float u2, float v2) {
        // UV в том же порядке, что и вершины
        return face switch {
            BlockFace.Top => new[] {
                new Vector2(u1, v2),  // 0
                new Vector2(u1, v1),  // 1
                new Vector2(u2, v1),  // 2
                new Vector2(u2, v2)   // 3
            },
            BlockFace.Bottom => new[] {
                new Vector2(u1, v1),  // 0
                new Vector2(u1, v2),  // 1
                new Vector2(u2, v2),  // 2
                new Vector2(u2, v1)   // 3
            },
            BlockFace.Front => new[] {
                new Vector2(u1, v1),  // 0
                new Vector2(u1, v2),  // 1
                new Vector2(u2, v2),  // 2
                new Vector2(u2, v1)   // 3
            },
            BlockFace.Back => new[] {
                new Vector2(u2, v1),  // 0
                new Vector2(u2, v2),  // 1
                new Vector2(u1, v2),  // 2
                new Vector2(u1, v1)   // 3
            },
            BlockFace.Right => new[] {
                new Vector2(u2, v1),  // 0
                new Vector2(u2, v2),  // 1
                new Vector2(u1, v2),  // 2
                new Vector2(u1, v1)   // 3
            },
            BlockFace.Left => new[] {
                new Vector2(u1, v1),  // 0
                new Vector2(u1, v2),  // 1
                new Vector2(u2, v2),  // 2
                new Vector2(u2, v1)   // 3
            },
            _ => new Vector2[0]
        };
    }

    private Vector3 GetFaceNormal(BlockFace face) {
        return face switch {
            BlockFace.Top => new Vector3(0, 1, 0),
            BlockFace.Bottom => new Vector3(0, -1, 0),
            BlockFace.Front => new Vector3(0, 0, 1),
            BlockFace.Back => new Vector3(0, 0, -1),
            BlockFace.Right => new Vector3(1, 0, 0),
            BlockFace.Left => new Vector3(-1, 0, 0),
            _ => Vector3.Zero
        };
    }

    private (int tx, int ty) GetBlockTexture(BlockType type, BlockFace face) {
        return type switch {
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