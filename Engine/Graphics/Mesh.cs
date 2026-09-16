using OpenTK.Graphics.OpenGL4;

namespace MinecraftEngine.Engine.Graphics;

public class Mesh : IDisposable {
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vertexCount;
    private readonly bool _hasData;
    private bool _isDisposed;

    public Mesh() {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        _hasData = false;
        _vertexCount = 0;
        _isDisposed = false;
    }

    public Mesh(float[] vertexData, int[] indices) : this() {
        _vertexCount = indices.Length;
        _hasData = vertexData.Length > 0 && indices.Length > 0;

        if (!_hasData) return;

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    public void Render(Shader shader) {
        if (!_hasData || _vertexCount == 0 || _isDisposed || shader == null) return;

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Render(LightingShader shader) {
        if (!_hasData || _vertexCount == 0 || _isDisposed || shader == null) return;

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose() {
        if (_isDisposed) return;

        _isDisposed = true;
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
    }
}