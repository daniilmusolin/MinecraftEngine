using MinecraftEngine.Engine.Core;
using MinecraftEngine.Engine.Graphics;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.World;

public class Chunk : IDisposable {
    public const int Size = 16;
    public const int Height = 256;

    private readonly int _chunkX;
    private readonly int _chunkZ;
    private readonly BlockType[,,] _blocks;
    private Mesh _mesh;
    private bool _needsRebuild = true;
    private bool _isReady;
    private readonly object _lock = new();

    public bool NeedsRebuild => _needsRebuild;
    public bool IsReady => _isReady;

    private Matrix4 _cachedModelMatrix;
    private bool _matrixDirty = true;

    public Chunk(int x, int z, ChunkGenerator generator) {
        _chunkX = x;
        _chunkZ = z;
        _blocks = generator.GenerateChunk(x, z);
        _mesh = new Mesh();
    }

    public void RebuildMesh(ChunkMeshBuilder builder) {
        lock (_lock) {
            _mesh.Dispose();
            _mesh = builder.BuildMesh(this, _chunkX, _chunkZ);
            _needsRebuild = false;
            _isReady = true;
            _matrixDirty = true;
        }
    }

    public BlockType GetBlock(int x, int y, int z) {
        if (x < 0 || x >= Size || y < 0 || y >= Height || z < 0 || z >= Size) {
            return BlockType.Air;
        }
        return _blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, BlockType type) {
        if (x < 0 || x >= Size || y < 0 || y >= Height || z < 0 || z >= Size) return;

        _blocks[x, y, z] = type;
        _needsRebuild = true;
    }

    public void MarkForRebuild() {
        _needsRebuild = true;
    }

    public void Render(LightingShader shader) {
        if (!_isReady) return;

        if (_matrixDirty) {
            _cachedModelMatrix = Matrix4.CreateTranslation(_chunkX * Size, 0, _chunkZ * Size);
            _matrixDirty = false;
        }

        shader.SetMatrix4("model", _cachedModelMatrix);
        _mesh.Render(shader);
    }

    public void Dispose() {
        _mesh?.Dispose();
    }
}