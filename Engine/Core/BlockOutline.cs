using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public class BlockOutline {
    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _shaderProgram;
    private bool _isInitialized = false;

    // Вершины куба с центром в (0,0,0) от -0.5 до 0.5
    private readonly float[] _vertices = {
        // Задняя грань (z = -0.5)
        -0.5f, -0.5f, -0.5f,  // 0
         0.5f, -0.5f, -0.5f,  // 1
         0.5f,  0.5f, -0.5f,  // 2
        -0.5f,  0.5f, -0.5f,  // 3
        // Передняя грань (z = 0.5)
        -0.5f, -0.5f,  0.5f,  // 4
         0.5f, -0.5f,  0.5f,  // 5
         0.5f,  0.5f,  0.5f,  // 6
        -0.5f,  0.5f,  0.5f   // 7
    };

    // Индексы для отрисовки линий (12 ребер куба)
    private readonly uint[] _indices = {
        0, 1, 1, 2, 2, 3, 3, 0,  // Задняя грань
        4, 5, 5, 6, 6, 7, 7, 4,  // Передняя грань
        0, 4, 1, 5, 2, 6, 3, 7   // Соединения
    };

    public BlockOutline() {
        Initialize();
    }

    private void Initialize() {
        try {
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                
                uniform mat4 uView;
                uniform mat4 uProjection;
                uniform vec3 uPosition;
                
                void main() {
                    vec3 worldPos = uPosition + aPosition;
                    gl_Position = uProjection * uView * vec4(worldPos, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                uniform vec4 uColor;
                
                void main() {
                    FragColor = uColor;
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                Console.WriteLine($"BlockOutline Vertex Shader Error: {infoLog}");
                return;
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                Console.WriteLine($"BlockOutline Fragment Shader Error: {infoLog}");
                return;
            }

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) {
                string infoLog = GL.GetProgramInfoLog(_shaderProgram);
                Console.WriteLine($"BlockOutline Program Link Error: {infoLog}");
                return;
            }

            GL.DetachShader(_shaderProgram, vertexShader);
            GL.DetachShader(_shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            _isInitialized = true;
            Console.WriteLine("BlockOutline initialized!");
        } catch (Exception ex) {
            Console.WriteLine($"BlockOutline initialization error: {ex.Message}");
        }
    }

    public void Render(Vector3 blockPos, Matrix4 view, Matrix4 projection, Vector4 color) {
        if (!_isInitialized) return;

        try {
            bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
            bool blend = GL.IsEnabled(EnableCap.Blend);
            bool cullFace = GL.IsEnabled(EnableCap.CullFace);

            if (!depthTest) GL.Enable(EnableCap.DepthTest);
            if (!blend) GL.Enable(EnableCap.Blend);
            if (cullFace) GL.Disable(EnableCap.CullFace);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.LineWidth(2.5f);

            GL.UseProgram(_shaderProgram);

            int viewLoc = GL.GetUniformLocation(_shaderProgram, "uView");
            int projLoc = GL.GetUniformLocation(_shaderProgram, "uProjection");
            int posLoc = GL.GetUniformLocation(_shaderProgram, "uPosition");
            int colorLoc = GL.GetUniformLocation(_shaderProgram, "uColor");

            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            // Смещаем на 0.5 чтобы центр блока был в центре куба
            Vector3 posWithOffset = blockPos + new Vector3(0.5f, 0.5f, 0.5f);
            GL.Uniform3(posLoc, posWithOffset);
            GL.Uniform4(colorLoc, color);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Lines, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            GL.UseProgram(0);

            if (!depthTest) GL.Disable(EnableCap.DepthTest);
            if (!blend) GL.Disable(EnableCap.Blend);
            if (cullFace) GL.Enable(EnableCap.CullFace);

            GL.LineWidth(1.0f);
        } catch (Exception ex) {
            Console.WriteLine($"BlockOutline Render error: {ex.Message}");
        }
    }

    public void Dispose() {
        if (_isInitialized) {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteProgram(_shaderProgram);
            _isInitialized = false;
        }
    }
}