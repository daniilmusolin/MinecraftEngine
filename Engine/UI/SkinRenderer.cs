using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MinecraftEngine.Engine.Graphics;

namespace MinecraftEngine.Engine.UI;

public class SkinRenderer : IDisposable {
    private int _vao;
    private int _vbo;
    private int _ebo;
    private Shader _shader;
    private bool _isInitialized;

    public SkinRenderer() {
        Initialize();
    }

    private void Initialize() {
        string vertexShader = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            
            void main() {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
            }
        ";

        string fragmentShader = @"
            #version 330 core
            out vec4 FragColor;
            
            uniform vec3 color;
            
            void main() {
                FragColor = vec4(color, 1.0);
            }
        ";

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexShader);
        GL.CompileShader(vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentShader);
        GL.CompileShader(fragment);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        _shader = new Shader(program);

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        float[] vertices = {
            -0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f,  0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f,
        };

        int[] indices = {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            0,4,5, 0,5,1,
            3,2,6, 3,6,7,
            0,3,7, 0,7,4,
            1,5,6, 1,6,2
        };

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);

        _isInitialized = true;
    }

    public void Render(Vector3 position, Matrix4 view, Matrix4 projection, bool isBack) {
        if (!_isInitialized) return;

        _shader.Use();
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        float scale = 1.8f;
        Matrix4 model = Matrix4.CreateTranslation(position);
        if (isBack) {
            model *= Matrix4.CreateRotationY(MathHelper.Pi);
        }
        model *= Matrix4.CreateScale(scale);

        _shader.SetMatrix4("model", model);
        _shader.SetVector3("color", new Vector3(0.8f, 0.6f, 0.4f));

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose() {
        if (_isInitialized) {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _shader?.Dispose();
            _isInitialized = false;
        }
    }
}