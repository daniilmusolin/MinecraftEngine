using OpenTK.Mathematics;
using System.Collections.Concurrent;
using MinecraftEngine.Engine.Graphics;
using MinecraftEngine.Engine.Core;

namespace MinecraftEngine.Engine.World;

public class World : IDisposable {
    private readonly ConcurrentDictionary<Vector2i, Chunk> _chunks = new();
    private readonly ChunkGenerator _generator;
    private readonly ChunkMeshBuilder _meshBuilder;
    private readonly object _lock = new();
    private Vector3 _playerPos;
    private int _viewDistance = 6;
    private bool _needsUpdate;
    private readonly List<Chunk> _visibleChunks = new();
    private int _lastChunkCount = -1;
    private Vector3 _lastPlayerPos;

    private Sun? _sun;
    private List<Cloud> _clouds = new();

    public World(Camera camera) {
        _generator = new ChunkGenerator();
        _meshBuilder = new ChunkMeshBuilder();
        _playerPos = camera.Position;
        _needsUpdate = true;
        _sun = new Sun();

        var random = new Random();
        for (int i = 0; i < 30; i++) {
            float x = random.Next(-200, 200);
            float z = random.Next(-200, 200);
            float size = 8f + random.Next(5, 15);
            float speed = 0.2f + random.Next(0, 5) * 0.05f;
            var cloud = new Cloud(new Vector3(x, 120, z), size, speed);
            _clouds.Add(cloud);
        }
    }

    public void UpdateClouds(float deltaTime, float timeOfDay) {
        foreach (var cloud in _clouds) {
            cloud.Update(deltaTime);
        }
        _sun?.Update(timeOfDay);
    }

    public void GenerateChunks(int viewDistance) {
        _viewDistance = Math.Min(viewDistance, 4);
        _needsUpdate = true;
        UpdateChunks();
    }

    public void Update(Vector3 playerPos) {
        if (Vector3.Distance(playerPos, _playerPos) > 1f) {
            _playerPos = playerPos;
            _needsUpdate = true;
        }

        if (_needsUpdate) {
            UpdateChunks();
            _needsUpdate = false;
        }

        foreach (var chunk in _chunks.Values) {
            if (chunk.NeedsRebuild) {
                lock (_lock) {
                    chunk.RebuildMesh(_meshBuilder);
                }
            }
        }
    }

    private void UpdateChunks() {
        var centerX = (int)Math.Floor(_playerPos.X / 16);
        var centerZ = (int)Math.Floor(_playerPos.Z / 16);

        var newChunks = new HashSet<Vector2i>();
        var radius = _viewDistance;

        for (var x = -radius; x <= radius; x++) {
            for (var z = -radius; z <= radius; z++) {
                var dist = Math.Sqrt(x * x + z * z);
                if (dist > radius) continue;

                var pos = new Vector2i(centerX + x, centerZ + z);
                newChunks.Add(pos);

                if (!_chunks.ContainsKey(pos)) {
                    var chunk = new Chunk(pos.X, pos.Z, _generator);
                    lock (_lock) {
                        _chunks.TryAdd(pos, chunk);
                        chunk.RebuildMesh(_meshBuilder);
                    }
                }
            }
        }

        var toRemove = _chunks.Keys.Where(k => !newChunks.Contains(k)).ToList();
        foreach (var key in toRemove) {
            if (_chunks.TryRemove(key, out var chunk)) {
                chunk.Dispose();
            }
        }
    }

    public int GetViewDistance() {
        return _viewDistance;
    }

    public void SetViewDistance(int distance) {
        _viewDistance = Math.Max(2, Math.Min(distance, 8));
        _needsUpdate = true;
    }

    public void Render(LightingShader shader) {
        if (shader == null) return;

        if (_chunks.Count != _lastChunkCount || Vector3.Distance(_playerPos, _lastPlayerPos) > 10f) {
            _visibleChunks.Clear();
            foreach (var chunk in _chunks.Values) {
                if (chunk.IsReady) {
                    _visibleChunks.Add(chunk);
                }
            }
            _lastChunkCount = _chunks.Count;
            _lastPlayerPos = _playerPos;
        }

        foreach (var chunk in _visibleChunks) {
            chunk.Render(shader);
        }
    }

    public void RenderSunAndClouds3D(LightingShader shader, Camera camera, int screenWidth, int screenHeight) {
        if (shader == null) return;
        _sun?.Render(shader, camera, screenWidth, screenHeight);
        foreach (var cloud in _clouds) {
            cloud.Render(shader, camera);
        }
    }

    public BlockType GetBlock(Vector3 pos) {
        int bx = (int)Math.Floor(pos.X);
        int by = (int)Math.Floor(pos.Y);
        int bz = (int)Math.Floor(pos.Z);

        int chunkX = (int)Math.Floor(bx / 16f);
        int chunkZ = (int)Math.Floor(bz / 16f);
        int localX = bx - chunkX * 16;
        int localZ = bz - chunkZ * 16;

        if (by < 0 || by >= 256) return BlockType.Air;

        if (_chunks.TryGetValue(new Vector2i(chunkX, chunkZ), out var chunk)) {
            return chunk.GetBlock(localX, by, localZ);
        }
        return BlockType.Air;
    }

    public void SetBlock(Vector3 pos, BlockType type) {
        int bx = (int)Math.Floor(pos.X);
        int by = (int)Math.Floor(pos.Y);
        int bz = (int)Math.Floor(pos.Z);

        int chunkX = (int)Math.Floor(bx / 16f);
        int chunkZ = (int)Math.Floor(bz / 16f);
        int localX = bx - chunkX * 16;
        int localZ = bz - chunkZ * 16;

        if (by < 0 || by >= 256) return;

        if (_chunks.TryGetValue(new Vector2i(chunkX, chunkZ), out var chunk)) {
            chunk.SetBlock(localX, by, localZ, type);
            MarkNeighborChunksForRebuild(chunkX, chunkZ);
        }
    }

    private void MarkNeighborChunksForRebuild(int chunkX, int chunkZ) {
        for (int dx = -1; dx <= 1; dx++) {
            for (int dz = -1; dz <= 1; dz++) {
                if (dx == 0 && dz == 0) continue;

                var pos = new Vector2i(chunkX + dx, chunkZ + dz);
                if (_chunks.TryGetValue(pos, out var chunk)) {
                    chunk.MarkForRebuild();
                }
            }
        }
    }

    public bool IsBlockAt(Vector3 pos) {
        return GetBlock(pos) != BlockType.Air;
    }

    public int GetChunkCount() {
        return _chunks.Count;
    }

    public void Regenerate() {
        foreach (var chunk in _chunks.Values) {
            chunk.Dispose();
        }
        _chunks.Clear();
        _needsUpdate = true;
    }

    public void Dispose() {
        foreach (var chunk in _chunks.Values) {
            chunk.Dispose();
        }
        _chunks.Clear();
        _sun?.Dispose();
        foreach (var cloud in _clouds) {
            cloud.Dispose();
        }
        _clouds.Clear();
    }
}

public struct Vector2i {
    public int X { get; }
    public int Z { get; }

    public Vector2i(int x, int z) {
        X = x;
        Z = z;
    }

    public override bool Equals(object? obj) {
        if (obj is Vector2i other) {
            return X == other.X && Z == other.Z;
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(X, Z);
}
