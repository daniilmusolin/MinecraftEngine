using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Graphics;

public class LightingShader : IDisposable {
    private int _handle;
    private bool _disposed;

    public LightingShader() {
        string vertexShader = File.ReadAllText("Shaders/chunk.glsl");
        string fragmentShader = File.ReadAllText("Shaders/fragment.glsl");

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexShader);
        GL.CompileShader(vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentShader);
        GL.CompileShader(fragment);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertex);
        GL.AttachShader(_handle, fragment);
        GL.LinkProgram(_handle);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use() {
        GL.UseProgram(_handle);
    }

    public void SetMatrix4(string name, Matrix4 matrix) {
        int location = GL.GetUniformLocation(_handle, name);
        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector) {
        int location = GL.GetUniformLocation(_handle, name);
        GL.Uniform3(location, vector);
    }

    public void SetFloat(string name, float value) {
        int location = GL.GetUniformLocation(_handle, name);
        GL.Uniform1(location, value);
    }

    public void SetInt(string name, int value) {
        int location = GL.GetUniformLocation(_handle, name);
        GL.Uniform1(location, value);
    }

    public void Dispose() {
        if (!_disposed) {
            GL.DeleteProgram(_handle);
            _disposed = true;
        }
    }
}