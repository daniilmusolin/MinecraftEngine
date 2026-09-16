using MinecraftEngine.Engine.Core;
using MinecraftEngine.Engine.Graphics;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftEngine.Engine.World;

public class Sun : IDisposable {
    private readonly Mesh _mesh;
    private float _angle;
    private readonly float _distance = 500f;
    private readonly float _height = 250f;
    private readonly float _size = 40f;
    private Shader _sunShader;
    private bool _disposed;

    public Sun() {
        _mesh = CreateSunMesh();
        _angle = 0.25f * MathHelper.TwoPi;
        _sunShader = CreateSunShader();
    }

    private Shader CreateSunShader() {
        string vertexShader = @"
            #version 460 core

            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec2 aTexCoord;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec2 TexCoord;

            void main() {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                TexCoord = aTexCoord;
            }
        ";

        string fragmentShader = @"
            #version 460 core

            in vec2 TexCoord;
            out vec4 FragColor;

            uniform vec3 sunColor;
            uniform float timeOfDay;

            void main() {
                vec2 center = TexCoord - 0.5;
                float dist = length(center);
                if (dist > 0.5) discard;
                
                float alpha = 1.0 - smoothstep(0.2, 0.5, dist);
                vec3 color = sunColor * (1.0 + 0.3 * (1.0 - dist * 2.0));
                
                FragColor = vec4(color, alpha);
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

        return new Shader(program);
    }

    private Mesh CreateSunMesh() {
        float size = 1f;
        float[] vertices = {
            -size, -size, 0,  0f, 0f,
             size, -size, 0,  1f, 0f,
             size,  size, 0,  1f, 1f,
            -size,  size, 0,  0f, 1f,
        };
        int[] indices = { 0, 1, 2, 0, 2, 3 };

        float[] vertexData = new float[4 * 5];
        for (int i = 0; i < 4; i++) {
            int idx = i * 5;
            vertexData[idx] = vertices[i * 3];
            vertexData[idx + 1] = vertices[i * 3 + 1];
            vertexData[idx + 2] = vertices[i * 3 + 2];
            vertexData[idx + 3] = vertices[i * 3 + 3];
            vertexData[idx + 4] = vertices[i * 3 + 4];
        }

        return new Mesh(vertexData, indices);
    }

    public void Update(float timeOfDay) {
        _angle = timeOfDay * MathHelper.TwoPi;
    }

    public void Render(LightingShader lightingShader, Camera camera, int screenWidth, int screenHeight) {
        if (_mesh == null || _sunShader == null) return;

        // ЖЁСТКО ФИКСИРУЕМ позицию - солнце всегда над головой
        var sunPos = new Vector3(
            0f,           // X - по центру
            300f,         // Y - высоко в небе
            0f            // Z - по центру
        );

        // Или относительно камеры:
        // var sunPos = camera.Position + new Vector3(0, 300, 0);

        var model = Matrix4.CreateTranslation(sunPos) * Matrix4.CreateScale(_size);

        _sunShader.Use();
        _sunShader.SetMatrix4("model", model);
        _sunShader.SetMatrix4("view", camera.GetViewMatrix());
        _sunShader.SetMatrix4("projection", camera.GetProjectionMatrix(screenWidth, screenHeight));
        _sunShader.SetVector3("sunColor", new Vector3(1f, 0.9f, 0.4f));

        // ВКЛЮЧАЕМ DepthTest
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _mesh.Render(_sunShader);

        GL.Disable(EnableCap.Blend);
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        _mesh?.Dispose();
        _sunShader?.Dispose();
    }
}