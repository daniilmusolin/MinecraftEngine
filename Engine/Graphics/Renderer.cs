using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MinecraftEngine.Engine.Core;

namespace MinecraftEngine.Engine.Graphics;

public class Renderer : IDisposable {
    private readonly Shader _shader;
    private readonly TextureAtlas _atlas;
    private readonly List<IRenderable> _renderQueue = new();
    private readonly object _lock = new();

    public int ViewDistance { get; set; } = 64;
    public float FogDensity { get; set; } = 0.01f;
    public Vector3 FogColor { get; set; } = new(0.53f, 0.81f, 0.92f);
    public bool WireframeMode { get; set; }
    public bool ShowChunkBorders { get; set; }

    public Renderer(Shader shader, TextureAtlas atlas) {
        _shader = shader;
        _atlas = atlas;
    }

    public void AddRenderable(IRenderable renderable) {
        lock (_lock) {
            _renderQueue.Add(renderable);
        }
    }

    public void RemoveRenderable(IRenderable renderable) {
        lock (_lock) {
            _renderQueue.Remove(renderable);
        }
    }

    public void Clear() {
        lock (_lock) {
            _renderQueue.Clear();
        }
    }

    public void Render(Camera camera) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        
        if (WireframeMode) {
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        } else {
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        _shader.Use();
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix(1920, 1080));
        _shader.SetFloat("fogDensity", FogDensity);
        _shader.SetVector3("fogColor", FogColor);
        _shader.SetVector3("cameraPos", camera.Position);

        _atlas.Use(0);
        _shader.SetInt("textureAtlas", 0);

        lock (_lock) {
            foreach (var renderable in _renderQueue) {
                if (renderable.IsVisible(camera)) {
                    renderable.Render(_shader);
                }
            }
        }

        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    public void Dispose() {
        Clear();
    }
}

public interface IRenderable {
    void Render(Shader shader);
    bool IsVisible(Camera camera);
}