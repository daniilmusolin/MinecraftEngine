using MinecraftEngine.Engine.Core;
using MinecraftEngine.Engine.World;

namespace MinecraftEngine.Engine.Utils;

public static class GreedyMeshing {
    public class MeshData {
        public int X, Y, Z;
        public int Width, Height, Depth;
        public BlockType BlockType;
        public BlockFace Face;
        public int[] Vertices = Array.Empty<int>();
        public int[] Indices = Array.Empty<int>();
        public float[] UVs = Array.Empty<float>();
    }

    public static List<MeshData> OptimizeChunk(BlockType[,,] blocks, int size) {
        var meshes = new List<MeshData>();
        var visited = new bool[size, size, size];
        
        for (var z = 0; z < size; z++) {
            for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                    var block = blocks[x, y, z];
                    if (block == BlockType.Air || visited[x, y, z]) continue;
                    
                    if (x + 1 < size && blocks[x + 1, y, z] != BlockType.Air) continue;
                    
                    var width = 1;
                    var height = 1;
                    
                    while (x + width < size && blocks[x + width, y, z] == block && !visited[x + width, y, z]) width++;
                    
                    var canExpand = true;
                    while (y + height < size && canExpand) {
                        for (var i = 0; i < width; i++) {
                            if (blocks[x + i, y + height, z] != block || visited[x + i, y + height, z]) {
                                canExpand = false;
                                break;
                            }
                            if (x + 1 < size && blocks[x + 1 + i, y + height, z] != BlockType.Air) {
                                canExpand = false;
                                break;
                            }
                        }
                        if (canExpand) height++;
                    }
                    
                    for (var i = 0; i < width; i++) {
                        for (var j = 0; j < height; j++) {
                            visited[x + i, y + j, z] = true;
                        }
                    }
                    
                    meshes.Add(new MeshData {
                        X = x, Y = y, Z = z,
                        Width = width, Height = height, Depth = 1,
                        BlockType = block,
                        Face = BlockFace.Right
                    });
                }
            }
        }
        
        visited = new bool[size, size, size];
        for (var x = 0; x < size; x++) {
            for (var y = 0; y < size; y++) {
                for (var z = 0; z < size; z++) {
                    var block = blocks[x, y, z];
                    if (block == BlockType.Air || visited[x, y, z]) continue;
                    
                    if (z + 1 < size && blocks[x, y, z + 1] != BlockType.Air) continue;
                    
                    var width = 1;
                    var height = 1;
                    
                    while (z + width < size && blocks[x, y, z + width] == block && !visited[x, y, z + width]) width++;
                    
                    var canExpand = true;
                    while (y + height < size && canExpand) {
                        for (var i = 0; i < width; i++) {
                            if (blocks[x, y + height, z + i] != block || visited[x, y + height, z + i]) {
                                canExpand = false;
                                break;
                            }
                        }
                        if (canExpand) height++;
                    }
                    
                    for (var i = 0; i < width; i++) {
                        for (var j = 0; j < height; j++) {
                            visited[x, y + j, z + i] = true;
                        }
                    }
                    
                    meshes.Add(new MeshData {
                        X = x, Y = y, Z = z,
                        Width = width, Height = height, Depth = 1,
                        BlockType = block,
                        Face = BlockFace.Front
                    });
                }
            }
        }
        
        visited = new bool[size, size, size];
        for (var x = 0; x < size; x++) {
            for (var z = 0; z < size; z++) {
                for (var y = 0; y < size; y++) {
                    var block = blocks[x, y, z];
                    if (block == BlockType.Air || visited[x, y, z]) continue;
                    
                    if (y + 1 < size && blocks[x, y + 1, z] != BlockType.Air) continue;
                    
                    var width = 1;
                    var depth = 1;
                    
                    while (x + width < size && blocks[x + width, y, z] == block && !visited[x + width, y, z]) width++;
                    
                    var canExpand = true;
                    while (z + depth < size && canExpand) {
                        for (var i = 0; i < width; i++) {
                            if (blocks[x + i, y, z + depth] != block || visited[x + i, y, z + depth]) {
                                canExpand = false;
                                break;
                            }
                        }
                        if (canExpand) depth++;
                    }
                    
                    for (var i = 0; i < width; i++) {
                        for (var j = 0; j < depth; j++) {
                            visited[x + i, y, z + j] = true;
                        }
                    }
                    
                    meshes.Add(new MeshData {
                        X = x, Y = y, Z = z,
                        Width = width, Height = 1, Depth = depth,
                        BlockType = block,
                        Face = BlockFace.Top
                    });
                }
            }
        }
        
        return meshes;
    }
}