using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MinecraftEngine.Engine.Graphics;
using MinecraftEngine.Engine.Core;
using System.Collections.Generic;

namespace MinecraftEngine.Engine.UI;

public class InventoryRenderer : IDisposable {
    private Inventory _inventory;
    private TextureAtlas _atlas;
    private Shader _uiShader;
    private TextRenderer _textRenderer;
    private int _vao;
    private int _vbo;
    private int _ebo;
    private bool _isInitialized;

    private float _slotSize = 50f;
    private float _padding = 4f;
    private int _hotbarSize = 9;

    private struct RectData {
        public int VertexStart;
        public int IndexStart;
        public int VertexCount;
        public int IndexCount;
        public Vector3 Color;
        public bool UseTexture;
    }

    private List<RectData> _rectData = new List<RectData>();

    public InventoryRenderer(Inventory inventory, TextureAtlas atlas, TextRenderer textRenderer) {
        _inventory = inventory;
        _atlas = atlas;
        _textRenderer = textRenderer;
        InitializeUI();
    }

    private void InitializeUI() {
        string vertexShader = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            
            uniform mat4 projection;
            
            out vec2 TexCoord;
            
            void main() {
                gl_Position = projection * vec4(aPosition, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
        ";

        string fragmentShader = @"
            #version 330 core
            in vec2 TexCoord;
            out vec4 FragColor;
            
            uniform sampler2D textureAtlas;
            uniform vec3 color;
            uniform bool useTexture;
            
            void main() {
                if (useTexture) {
                    FragColor = texture(textureAtlas, TexCoord);
                } else {
                    FragColor = vec4(color, 1.0);
                }
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

        _uiShader = new Shader(program);

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 10000 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        _isInitialized = true;
    }

    public void Render(int screenWidth, int screenHeight) {
        if (!_isInitialized) return;

        _uiShader.Use();

        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
        _uiShader.SetMatrix4("projection", projection);
        _uiShader.SetInt("textureAtlas", 0);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.DepthTest);

        float totalWidth = _hotbarSize * (_slotSize + _padding) - _padding;
        float startX = (screenWidth - totalWidth) / 2f;
        float y = screenHeight - _slotSize - 30f;

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        List<float> allVertices = new List<float>();
        List<int> allIndices = new List<int>();
        _rectData.Clear();

        int vertexOffset = 0;
        int indexOffset = 0;

        for (int i = 0; i < _hotbarSize; i++) {
            float x = startX + i * (_slotSize + _padding);
            bool isSelected = i == _inventory.SelectedSlot;

            Vector3 bgColor = isSelected ? new Vector3(0.4f, 0.4f, 0.5f) : new Vector3(0.15f, 0.15f, 0.2f);
            Vector3 borderColor = isSelected ? new Vector3(1f, 1f, 1f) : new Vector3(0.4f, 0.4f, 0.4f);

            AddRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                           x, y, _slotSize, _slotSize, bgColor, false);

            AddRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                           x, y, _slotSize, 1f, borderColor, false);
            AddRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                           x, y + _slotSize - 1f, _slotSize, 1f, borderColor, false);
            AddRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                           x, y, 1f, _slotSize, borderColor, false);
            AddRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                           x + _slotSize - 1f, y, 1f, _slotSize, borderColor, false);

            var item = _inventory[i];
            if (!item.IsEmpty) {
                float iconSize = _slotSize * 0.6f;
                float ix = x + (_slotSize - iconSize) / 2f;
                float iy = y + (_slotSize - iconSize) / 2f;

                Vector2[] uv = GetBlockUV(item.Type);
                AddTexturedRectVertices(allVertices, allIndices, _rectData, ref vertexOffset, ref indexOffset,
                                       ix, iy, iconSize, iconSize, uv);
            }
        }

        if (allVertices.Count > 0) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, allVertices.Count * sizeof(float), allVertices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, allIndices.Count * sizeof(int), allIndices.ToArray(), BufferUsageHint.DynamicDraw);

            foreach (var rect in _rectData) {
                _uiShader.SetVector3("color", rect.Color);
                _uiShader.SetInt("useTexture", rect.UseTexture ? 1 : 0);

                if (rect.UseTexture) {
                    _atlas.Use(0);
                }

                GL.DrawElements(PrimitiveType.Triangles, rect.IndexCount,
                               DrawElementsType.UnsignedInt, rect.IndexStart * sizeof(int));
            }
        }

        for (int i = 0; i < _hotbarSize; i++) {
            float x = startX + i * (_slotSize + _padding);
            var item = _inventory[i];

            if (!item.IsEmpty && item.Count > 1) {
                string countStr = item.Count.ToString();
                float countScale = 1.5f;
                float cx = x + _slotSize - countStr.Length * 5f * countScale - 6f;
                float cy = y + _slotSize - 7f * countScale - 4f;
                _textRenderer.RenderText(countStr, cx + 1, cy + 1, countScale, new Vector3(0f, 0f, 0f), screenWidth, screenHeight);
                _textRenderer.RenderText(countStr, cx, cy, countScale, new Vector3(1f, 1f, 1f), screenWidth, screenHeight);
            }
        }

        GL.BindVertexArray(0);
        GL.Disable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
    }

    public void DrawHand3D(Camera camera, int screenWidth, int screenHeight) {
        var selectedItem = _inventory.GetSelectedItem();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        _uiShader.Use();
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix(screenWidth, screenHeight);
        _uiShader.SetMatrix4("view", view);
        _uiShader.SetMatrix4("projection", projection);
        _uiShader.SetInt("textureAtlas", 0);

        float scale = 0.5f;
        float rotY = 0.5f;
        float rotX = -0.2f;

        Matrix4 model = Matrix4.CreateRotationY(rotY) * Matrix4.CreateRotationX(rotX);
        model *= Matrix4.CreateScale(scale);
        model *= Matrix4.CreateTranslation(new Vector3(0.8f, -0.6f, -1.2f));

        _uiShader.SetMatrix4("model", model);

        float[] cubeVerts = {
            -0.5f, -0.5f, -0.5f,  0f, 0f,
             0.5f, -0.5f, -0.5f,  1f, 0f,
             0.5f,  0.5f, -0.5f,  1f, 1f,
            -0.5f,  0.5f, -0.5f,  0f, 1f,
            -0.5f, -0.5f,  0.5f,  0f, 0f,
             0.5f, -0.5f,  0.5f,  1f, 0f,
             0.5f,  0.5f,  0.5f,  1f, 1f,
            -0.5f,  0.5f,  0.5f,  0f, 1f
        };

        int[] cubeIndices = {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            0,4,5, 0,5,1,
            3,2,6, 3,6,7,
            0,3,7, 0,7,4,
            1,5,6, 1,6,2
        };

        int handVAO = GL.GenVertexArray();
        int handVBO = GL.GenBuffer();
        int handEBO = GL.GenBuffer();

        GL.BindVertexArray(handVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, handVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, cubeVerts.Length * sizeof(float), cubeVerts, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, handEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, cubeIndices.Length * sizeof(int), cubeIndices, BufferUsageHint.DynamicDraw);

        Vector3 skinColor = new Vector3(0.8f, 0.6f, 0.4f);
        Vector3 sleeveColor = new Vector3(0.2f, 0.4f, 0.7f);

        _uiShader.SetVector3("color", sleeveColor);
        _uiShader.SetInt("useTexture", 0);
        GL.DrawElements(PrimitiveType.Triangles, cubeIndices.Length, DrawElementsType.UnsignedInt, 0);

        Matrix4 handModel = Matrix4.CreateRotationY(rotY) * Matrix4.CreateRotationX(rotX);
        handModel *= Matrix4.CreateScale(new Vector3(0.25f, 0.6f, 0.25f));
        handModel *= Matrix4.CreateTranslation(new Vector3(0.8f, -0.7f, -1.2f));

        _uiShader.SetMatrix4("model", handModel);
        _uiShader.SetVector3("color", skinColor);
        GL.DrawElements(PrimitiveType.Triangles, cubeIndices.Length, DrawElementsType.UnsignedInt, 0);

        if (!selectedItem.IsEmpty) {
            Vector2[] uv = GetBlockUV(selectedItem.Type);
            _atlas.Use(0);
            _uiShader.SetInt("useTexture", 1);

            Matrix4 itemModel = Matrix4.CreateRotationY(rotY + 0.3f) * Matrix4.CreateRotationX(rotX);
            itemModel *= Matrix4.CreateScale(new Vector3(0.3f, 0.3f, 0.3f));
            itemModel *= Matrix4.CreateTranslation(new Vector3(0.85f, -0.45f, -1.15f));

            _uiShader.SetMatrix4("model", itemModel);

            float[] itemVerts = {
                -0.5f, -0.5f, -0.5f,  uv[0].X, uv[0].Y,
                 0.5f, -0.5f, -0.5f,  uv[1].X, uv[1].Y,
                 0.5f,  0.5f, -0.5f,  uv[2].X, uv[2].Y,
                -0.5f,  0.5f, -0.5f,  uv[3].X, uv[3].Y,
                -0.5f, -0.5f,  0.5f,  uv[0].X, uv[0].Y,
                 0.5f, -0.5f,  0.5f,  uv[1].X, uv[1].Y,
                 0.5f,  0.5f,  0.5f,  uv[2].X, uv[2].Y,
                -0.5f,  0.5f,  0.5f,  uv[3].X, uv[3].Y
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, handVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, itemVerts.Length * sizeof(float), itemVerts, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cubeIndices.Length * sizeof(int), cubeIndices, BufferUsageHint.DynamicDraw);

            GL.DrawElements(PrimitiveType.Triangles, cubeIndices.Length, DrawElementsType.UnsignedInt, 0);
        }

        GL.DeleteVertexArray(handVAO);
        GL.DeleteBuffer(handVBO);
        GL.DeleteBuffer(handEBO);

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }

    private void AddRectVertices(List<float> vertices, List<int> indices, List<RectData> rectData,
                                 ref int vertexOffset, ref int indexOffset,
                                 float x, float y, float width, float height, Vector3 color, bool useTexture) {
        RectData data = new RectData {
            VertexStart = vertexOffset,
            IndexStart = indexOffset,
            VertexCount = 4,
            IndexCount = 6,
            Color = color,
            UseTexture = useTexture
        };
        rectData.Add(data);

        float[] verts = {
            x, y, 0f, 0f,
            x, y + height, 0f, 1f,
            x + width, y + height, 1f, 1f,
            x + width, y, 1f, 0f
        };
        vertices.AddRange(verts);

        indices.Add(vertexOffset + 0);
        indices.Add(vertexOffset + 1);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 0);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 3);

        vertexOffset += 4;
        indexOffset += 6;
    }

    private void AddTexturedRectVertices(List<float> vertices, List<int> indices, List<RectData> rectData,
                                         ref int vertexOffset, ref int indexOffset,
                                         float x, float y, float width, float height, Vector2[] uv) {
        RectData data = new RectData {
            VertexStart = vertexOffset,
            IndexStart = indexOffset,
            VertexCount = 4,
            IndexCount = 6,
            Color = Vector3.One,
            UseTexture = true
        };
        rectData.Add(data);
        float[] verts = {
            x, y, uv[0].X, uv[0].Y,
            x, y + height, uv[1].X, uv[1].Y,
            x + width, y + height, uv[2].X, uv[2].Y,
            x + width, y, uv[3].X, uv[3].Y
        };
        vertices.AddRange(verts);

        indices.Add(vertexOffset + 0);
        indices.Add(vertexOffset + 1);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 0);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 3);

        vertexOffset += 4;
        indexOffset += 6;
    }

    private Vector2[] GetBlockUV(BlockType type) {
        int tx = 0, ty = 0;

        switch (type) {
            case BlockType.Grass: tx = 3; ty = 0; break;
            case BlockType.Dirt: tx = 2; ty = 0; break;
            case BlockType.Stone: tx = 1; ty = 0; break;
            case BlockType.Wood: tx = 5; ty = 0; break;
            case BlockType.Leaves: tx = 6; ty = 0; break;
            case BlockType.Sand: tx = 7; ty = 0; break;
            case BlockType.Planks: tx = 0; ty = 1; break;
            case BlockType.Cobblestone: tx = 1; ty = 1; break;
            case BlockType.Brick: tx = 2; ty = 1; break;
            case BlockType.Glass: tx = 3; ty = 1; break;
            case BlockType.Snow: tx = 4; ty = 1; break;
            case BlockType.Netherrack: tx = 5; ty = 1; break;
            case BlockType.EndStone: tx = 6; ty = 1; break;
            case BlockType.Obsidian: tx = 7; ty = 1; break;
            default: tx = 0; ty = 0; break;
        }

        float u1 = tx / 16f;
        float v1 = ty / 16f;
        float u2 = (tx + 1) / 16f;
        float v2 = (ty + 1) / 16f;

        return new Vector2[] {
            new Vector2(u1, v2),
            new Vector2(u1, v1),
            new Vector2(u2, v1),
            new Vector2(u2, v2)
        };
    }

    public void Dispose() {
        if (_isInitialized) {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _uiShader?.Dispose();
            _isInitialized = false;
        }
    }
}