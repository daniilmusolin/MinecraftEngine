using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Graphics;

public class TextRenderer : IDisposable {
    private readonly int _vao;
    private readonly int _vbo;
    private readonly Shader _shader;
    private readonly Dictionary<char, Character> _characters = new();
    private bool _isInitialized = false;

    public struct Character {
        public Vector2 Size;
        public Vector2 Bearing;
        public float Advance;
        public int[]? Bitmap;
    }

    public TextRenderer() {
        try {
            string vertexSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPos;
                layout (location = 1) in vec2 aTexCoord;
                
                uniform mat4 projection;
                out vec2 TexCoords;
                
                void main() {
                    gl_Position = projection * vec4(aPos, 0.0, 1.0);
                    TexCoords = aTexCoord;
                }
            ";

            string fragmentSource = @"
                #version 330 core
                in vec2 TexCoords;
                out vec4 FragColor;
                uniform vec3 textColor;
                
                void main() {
                    FragColor = vec4(textColor, 1.0);
                }
            ";

            // Компилируем вершинный шейдер
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            // Проверяем ошибки
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                Console.WriteLine($"Vertex Shader Error: {infoLog}");
                return;
            }

            // Компилируем фрагментный шейдер
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                Console.WriteLine($"Fragment Shader Error: {infoLog}");
                return;
            }

            // Создаем программу
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Program Link Error: {infoLog}");
                return;
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            _shader = new Shader(program);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Позиция (2 float) + Текстурные координаты (2 float) = 4 float
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            GeneratePixelFont();
            _isInitialized = true;
            Console.WriteLine("TextRenderer initialized successfully!");
        } catch (Exception ex) {
            Console.WriteLine($"TextRenderer initialization error: {ex.Message}");
        }
    }

    private void GeneratePixelFont() {
        var fontData = new Dictionary<char, byte[]> {
            ['A'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 },
            ['B'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110 },
            ['C'] = new byte[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110 },
            ['D'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b11110 },
            ['E'] = new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111 },
            ['F'] = new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b10000 },
            ['G'] = new byte[] { 0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b10001, 0b01110 },
            ['H'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 },
            ['I'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b11111 },
            ['J'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b10100, 0b01100 },
            ['K'] = new byte[] { 0b10001, 0b10010, 0b10100, 0b11000, 0b10100, 0b10010, 0b10001 },
            ['L'] = new byte[] { 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b11111 },
            ['M'] = new byte[] { 0b10001, 0b11011, 0b10101, 0b10101, 0b10001, 0b10001, 0b10001 },
            ['N'] = new byte[] { 0b10001, 0b11001, 0b10101, 0b10011, 0b10001, 0b10001, 0b10001 },
            ['O'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 },
            ['P'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000 },
            ['R'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10010, 0b10001 },
            ['S'] = new byte[] { 0b01111, 0b10000, 0b10000, 0b01110, 0b00001, 0b00001, 0b11110 },
            ['T'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100 },
            ['U'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 },
            ['W'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10101, 0b10101, 0b11011, 0b10001 },
            ['Y'] = new byte[] { 0b10001, 0b10001, 0b01010, 0b00100, 0b00100, 0b00100, 0b00100 },
            ['Z'] = new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b10000, 0b11111 },
            ['0'] = new byte[] { 0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110 },
            ['1'] = new byte[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 },
            ['2'] = new byte[] { 0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111 },
            ['3'] = new byte[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110 },
            ['4'] = new byte[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010 },
            ['5'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110 },
            ['6'] = new byte[] { 0b00110, 0b01000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110 },
            ['7'] = new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b00100, 0b00100, 0b00100 },
            ['8'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110 },
            ['9'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00010, 0b01100 },
            [' '] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000 },
            ['.'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b01100, 0b01100 },
            [','] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b01100, 0b01000 },
            ['!'] = new byte[] { 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00000, 0b00100 },
            ['?'] = new byte[] { 0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b00000, 0b00100 },
            [':'] = new byte[] { 0b00000, 0b01100, 0b01100, 0b00000, 0b01100, 0b01100, 0b00000 },
            ['('] = new byte[] { 0b00010, 0b00100, 0b01000, 0b01000, 0b01000, 0b00100, 0b00010 },
            [')'] = new byte[] { 0b01000, 0b00100, 0b00010, 0b00010, 0b00010, 0b00100, 0b01000 },
            ['+'] = new byte[] { 0b00000, 0b00100, 0b00100, 0b11111, 0b00100, 0b00100, 0b00000 },
            ['-'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b11111, 0b00000, 0b00000, 0b00000 },
            ['='] = new byte[] { 0b00000, 0b00000, 0b11111, 0b00000, 0b11111, 0b00000, 0b00000 },
            ['/'] = new byte[] { 0b00000, 0b00001, 0b00010, 0b00100, 0b01000, 0b10000, 0b00000 },
            ['*'] = new byte[] { 0b00100, 0b10101, 0b01110, 0b11111, 0b01110, 0b10101, 0b00100 },
            ['<'] = new byte[] { 0b00010, 0b00100, 0b01000, 0b10000, 0b01000, 0b00100, 0b00010 },
            ['>'] = new byte[] { 0b01000, 0b00100, 0b00010, 0b00001, 0b00010, 0b00100, 0b01000 },
            ['['] = new byte[] { 0b11100, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b11100 },
            [']'] = new byte[] { 0b00111, 0b00001, 0b00001, 0b00001, 0b00001, 0b00001, 0b00111 },
            ['^'] = new byte[] { 0b00100, 0b01010, 0b10001, 0b00000, 0b00000, 0b00000, 0b00000 },
            ['_'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b11111 },
            ['`'] = new byte[] { 0b01000, 0b00100, 0b00010, 0b00000, 0b00000, 0b00000, 0b00000 },
            ['{'] = new byte[] { 0b00010, 0b00100, 0b00100, 0b01000, 0b00100, 0b00100, 0b00010 },
            ['|'] = new byte[] { 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100 },
            ['}'] = new byte[] { 0b01000, 0b00100, 0b00100, 0b00010, 0b00100, 0b00100, 0b01000 },
            ['~'] = new byte[] { 0b00000, 0b00000, 0b00000, 0b01010, 0b10100, 0b00000, 0b00000 },
        };

        foreach (var (c, data) in fontData) {
            _characters[c] = new Character {
                Size = new Vector2(5, 7),
                Bearing = new Vector2(0, 0),
                Advance = 6,
                Bitmap = data.Select(b => (int)b).ToArray()
            };
        }
        Console.WriteLine($"Font loaded with {_characters.Count} characters");
    }

    public void RenderText(string text, float x, float y, float scale, Vector3 color, int screenWidth, int screenHeight) {
        if (string.IsNullOrEmpty(text) || !_isInitialized) {
            return;
        }

        try {
            _shader.Use();
            _shader.SetVector3("textColor", color);

            var projection = Matrix4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
            _shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(_vao);

            float currentX = x;
            float currentY = y;

            foreach (char c in text) {
                if (c == '\n') {
                    currentY += 9 * scale;
                    currentX = x;
                    continue;
                }

                if (!_characters.TryGetValue(c, out var ch) || ch.Bitmap == null) continue;

                for (int row = 0; row < 7; row++) {
                    for (int col = 0; col < 5; col++) {
                        if ((ch.Bitmap[row] & (1 << (4 - col))) != 0) {
                            float px = currentX + col * scale;
                            float py = currentY + row * scale;
                            float pw = scale;
                            float ph = scale;

                            // Вершины: позиция (x,y) + текстурные координаты (не используются)
                            float[] vertices = {
                                px,     py + ph, 0f, 0f,
                                px,     py,     0f, 1f,
                                px + pw, py,     1f, 1f,
                                px,     py + ph, 0f, 0f,
                                px + pw, py,     1f, 1f,
                                px + pw, py + ph, 1f, 0f
                            };

                            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
                            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                        }
                    }
                }

                currentX += ch.Advance * scale;
            }

            GL.BindVertexArray(0);
        } catch (Exception ex) {
            Console.WriteLine($"TextRenderer error: {ex.Message}");
        }
    }

    public void Dispose() {
        if (_isInitialized) {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            _shader?.Dispose();
            _isInitialized = false;
        }
    }
}