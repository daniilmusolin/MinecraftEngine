using MinecraftEngine.Engine.Core;
using MinecraftEngine.Engine.Graphics;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.World;

public class Cloud : IDisposable {
    private readonly Mesh _mesh;
    private readonly Vector3 _position;
    private readonly float _size;
    private readonly float _speed;
    private float _offset;

    public Cloud(Vector3 position, float size, float speed) {
        _position = position;
        _size = size;
        _speed = speed;
        _offset = 0;
        _mesh = CreateCloudMesh();
    }

    private Mesh CreateCloudMesh() {
        var vertices = new List<float>();
        var indices = new List<int>();
        var uvs = new List<float>();
        var normals = new List<float>();

        var cloudParts = new[] {
            new Vector3(0, 0, 0),
            new Vector3(-0.3f, 0.1f, 0.2f),
            new Vector3(0.3f, 0.1f, -0.2f),
            new Vector3(-0.2f, -0.1f, -0.3f),
            new Vector3(0.2f, -0.1f, 0.3f),
            new Vector3(0, 0.2f, 0),
            new Vector3(-0.4f, 0, -0.1f),
            new Vector3(0.4f, 0, 0.1f),
        };

        var partSize = 0.4f;
        var indexOffset = 0;

        foreach (var part in cloudParts) {
            var pos = part * _size;
            var size = partSize * _size;

            var verts = new Vector3[] {
                new Vector3(-size, -size, -size) + pos,
                new Vector3(size, -size, -size) + pos,
                new Vector3(size, size, -size) + pos,
                new Vector3(-size, size, -size) + pos,
                new Vector3(-size, -size, size) + pos,
                new Vector3(size, -size, size) + pos,
                new Vector3(size, size, size) + pos,
                new Vector3(-size, size, size) + pos,
            };

            foreach (var v in verts) {
                vertices.Add(v.X);
                vertices.Add(v.Y);
                vertices.Add(v.Z);
                uvs.Add(0);
                uvs.Add(0);
                normals.Add(0);
                normals.Add(1);
                normals.Add(0);
            }

            int[] triIndices = {
                0,1,2, 0,2,3,
                4,6,5, 4,7,6,
                0,4,5, 0,5,1,
                3,2,6, 3,6,7,
                0,3,7, 0,7,4,
                1,5,6, 1,6,2
            };

            foreach (var i in triIndices) {
                indices.Add(indexOffset + i);
            }

            indexOffset += 8;
        }

        var vertexData = new float[vertices.Count + uvs.Count + normals.Count];
        for (int i = 0; i < vertices.Count / 3; i++) {
            int idx = i * 8;
            vertexData[idx] = vertices[i * 3];
            vertexData[idx + 1] = vertices[i * 3 + 1];
            vertexData[idx + 2] = vertices[i * 3 + 2];
            vertexData[idx + 3] = uvs[i * 2];
            vertexData[idx + 4] = uvs[i * 2 + 1];
            vertexData[idx + 5] = normals[i * 3];
            vertexData[idx + 6] = normals[i * 3 + 1];
            vertexData[idx + 7] = normals[i * 3 + 2];
        }

        return new Mesh(vertexData, indices.ToArray());
    }

    public void Update(float deltaTime) {
        _offset += deltaTime * _speed;
        if (_offset > 1000f) _offset -= 1000f;
    }

    public void Render(LightingShader shader, Camera camera) {
        if (shader == null) return;

        var model = Matrix4.CreateTranslation(
            _position.X + _offset,
            _position.Y,
            _position.Z
        );
        model *= Matrix4.CreateScale(_size);

        shader.SetMatrix4("model", model);
        _mesh.Render(shader);
    }

    public void Dispose() {
        _mesh?.Dispose();
    }
}