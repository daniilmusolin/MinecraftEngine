using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text;

namespace MinecraftEngine.Engine.Graphics;

public class Shader : IDisposable {
    private readonly int _handle;
    private readonly Dictionary<string, int> _uniforms = new();

    public Shader(int programHandle) {
        _handle = programHandle;
        _uniforms = new Dictionary<string, int>();

        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var uniforms);
        for (var i = 0; i < uniforms; i++) {
            var key = GL.GetActiveUniform(_handle, i, out _, out _);
            var location = GL.GetUniformLocation(_handle, key);
            _uniforms.Add(key, location);
        }
    }

    public Shader(string vertexPath, string fragmentPath) {
        Console.WriteLine($"Loading vertex shader: {vertexPath}");
        Console.WriteLine($"Loading fragment shader: {fragmentPath}");

        if (!File.Exists(vertexPath)) {
            throw new FileNotFoundException($"Vertex shader not found: {vertexPath}");
        }
        if (!File.Exists(fragmentPath)) {
            throw new FileNotFoundException($"Fragment shader not found: {fragmentPath}");
        }

        var vertexShader = CompileShader(vertexPath, ShaderType.VertexShader);
        var fragmentShader = CompileShader(fragmentPath, ShaderType.FragmentShader);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
        GL.LinkProgram(_handle);

        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var success);
        if (success == 0) {
            var info = GL.GetProgramInfoLog(_handle);
            throw new Exception($"Shader link error: {info}");
        }

        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var uniforms);
        for (var i = 0; i < uniforms; i++) {
            var key = GL.GetActiveUniform(_handle, i, out _, out _);
            var location = GL.GetUniformLocation(_handle, key);
            _uniforms.Add(key, location);
        }
    }

    private int CompileShader(string path, ShaderType type) {
        var src = File.ReadAllText(path);
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, src);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success == 0) {
            var info = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Shader compile error ({path}): {info}");
            GL.DeleteShader(shader);
            throw new Exception($"Shader compile error: {info}");
        }

        return shader;
    }

    public Matrix4 GetMatrix4(string name) {
        if (!_uniforms.TryGetValue(name, out var location)) return Matrix4.Identity;

        float[] values = new float[16];
        GL.GetUniform(_handle, location, values);
        return new Matrix4(
            values[0], values[1], values[2], values[3],
            values[4], values[5], values[6], values[7],
            values[8], values[9], values[10], values[11],
            values[12], values[13], values[14], values[15]
        );
    }

    public void Use() => GL.UseProgram(_handle);

    public int GetHandle() => _handle;

    public void SetMatrix4(string name, Matrix4 matrix) {
        if (!_uniforms.TryGetValue(name, out var location)) return;
        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector) {
        if (!_uniforms.TryGetValue(name, out var location)) return;
        GL.Uniform3(location, vector);
    }

    public void SetFloat(string name, float value) {
        if (!_uniforms.TryGetValue(name, out var location)) return;
        GL.Uniform1(location, value);
    }

    public void SetInt(string name, int value) {
        if (!_uniforms.TryGetValue(name, out var location)) return;
        GL.Uniform1(location, value);
    }

    public void Dispose() => GL.DeleteProgram(_handle);
}