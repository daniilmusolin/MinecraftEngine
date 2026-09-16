using MinecraftEngine.Engine.Graphics;
using MinecraftEngine.Engine.UI;
using MinecraftEngine.Engine.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace MinecraftEngine.Engine.Core;

public class Game : GameWindow {
    private World.World? _world;
    private Camera? _camera;
    private LightingShader? _shader;
    private TextureAtlas? _atlas;
    private TimeManager? _timeManager;
    private TextRenderer? _textRenderer;
    private PlayerController? _playerController;
    private Crosshair? _crosshair;
    private BlockOutline? _blockOutline;
    private BlockBreakAnimation? _blockBreakAnimation;
    private Inventory _inventory = new();
    private InventoryRenderer? _inventoryRenderer;

    private bool _wireframeMode;
    private bool _isInitialized;
    private bool _showDebugInfo;
    private float _fps;
    private int _frameCount;
    private float _frameTime;

    private float _accumulator = 0f;
    private const float PHYSICS_TIMESTEP = 1f / 60f;

    private Vector3? _targetBlock = null;
    private float _blockBreakProgress = 0f;
    private Vector3? _blockBeingBroken = null;
    private bool _isBreaking = false;
    private float _placeCooldown = 0f;
    private int _skinMode = 0;
    private SkinRenderer? _skinRenderer;

    private readonly Keys[] _hotbarKeys = {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
        Keys.D6, Keys.D7, Keys.D8, Keys.D9
    };

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings) {
        VSync = VSyncMode.On;
    }

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.53f, 0.81f, 0.92f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        CursorState = CursorState.Grabbed;
        CenterWindow();

        _camera = new Camera(new Vector3(8, 100, 8), Vector3.UnitY, -MathHelper.PiOver2);
        _shader = new LightingShader();
        _shader.Use();
        _shader.SetVector3("lightPos", new Vector3(100, 200, 0));
        _shader.SetVector3("lightColor", new Vector3(1.0f, 0.95f, 0.8f));
        _shader.SetVector3("ambientColor", new Vector3(0.4f, 0.5f, 0.7f));
        _shader.SetFloat("ambientStrength", 1.2f);
        _shader.SetVector3("viewPos", _camera.Position);

        _atlas = new TextureAtlas(16);
        _textRenderer = new TextRenderer();
        _timeManager = new TimeManager();
        _timeManager.OnTimeChanged += OnTimeChanged;
        _world = new World.World(_camera);
        _world.GenerateChunks(4);
        _playerController = new PlayerController(_camera, _world);
        _crosshair = new Crosshair(_textRenderer);
        _blockOutline = new BlockOutline();
        _blockBreakAnimation = new BlockBreakAnimation(_textRenderer);
        _inventoryRenderer = new InventoryRenderer(_inventory, _atlas, _textRenderer);
        _skinRenderer = new SkinRenderer();

        _isInitialized = true;
        Console.WriteLine("Game initialized!");
    }

    private void OnTimeChanged(float timeOfDay) {
        var (r, g, b) = _timeManager?.GetSkyColor() ?? (0.53f, 0.81f, 0.92f);
        GL.ClearColor(r, g, b, 1.0f);

        if (_shader != null) {
            float sunAngle = timeOfDay * 360.0f;
            float rad = MathHelper.DegreesToRadians(sunAngle);
            float sunHeight = MathF.Sin(rad);
            float sunHorizontal = MathF.Cos(rad);

            _shader.SetVector3("lightPos", new Vector3(sunHorizontal * 300, sunHeight * 300 + 100, 0));

            if (sunHeight < 0) {
                _shader.SetVector3("lightColor", new Vector3(0.3f, 0.3f, 0.5f));
                _shader.SetVector3("ambientColor", new Vector3(0.1f, 0.1f, 0.2f));
                _shader.SetFloat("ambientStrength", 0.3f);
            } else {
                _shader.SetVector3("lightColor", new Vector3(1.0f, 0.95f, 0.8f));
                _shader.SetVector3("ambientColor", new Vector3(0.3f, 0.4f, 0.6f));
                _shader.SetFloat("ambientStrength", 0.5f);
            }
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);

        if (!_isInitialized) return;
        if (_world == null || _shader == null || _camera == null) {
            Console.WriteLine("WARNING: World, Shader or Camera is null!");
            return;
        }

        _frameCount++;
        _frameTime += (float)args.Time;
        if (_frameTime >= 1.0f) {
            _fps = _frameCount / _frameTime;
            _frameCount = 0;
            _frameTime = 0;
        }

        // Очистка экрана
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Настройка основных состояний OpenGL для 3D рендеринга
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Настройка шейдера
        _shader.Use();
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = _camera.GetProjectionMatrix(Size.X, Size.Y);

        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _atlas?.Use(0);
        _shader.SetInt("textureAtlas", 0);
        _shader.SetVector3("viewPos", _camera.Position);

        if (_timeManager != null) {
            _shader.SetFloat("timeOfDay", _timeManager.TimeOfDay);
        }

        // ===== РЕНДЕРИНГ 3D МИРА =====
        // 1. Рендерим мир
        _world.Render(_shader);

        // 2. Рендерим солнце и облака
        _world.RenderSunAndClouds3D(_shader, _camera, Size.X, Size.Y);

        // 3. Рендерим контур блока (если есть цель)
        if (_blockOutline != null && _targetBlock != null) {
            _blockOutline.Render(_targetBlock.Value, view, projection, new Vector4(0f, 0f, 0f, 0.8f));
        }

        // 4. Рендерим анимацию разрушения блока
        _blockBreakAnimation?.Render(Size.X, Size.Y, view, projection, _camera);

        // 5. Рендерим 3D руку с предметом
        _inventoryRenderer?.DrawHand3D(_camera, Size.X, Size.Y);

        // 6. Рендерим скин (если включен)
        if (_skinMode > 0 && _skinRenderer != null) {
            Vector3 pos = _camera.Position;
            if (_skinMode == 1) {
                pos += _camera.Front * -3f;
            } else {
                pos += _camera.Front * 3f;
            }
            pos.Y = _camera.Position.Y;
            _skinRenderer.Render(pos, view, projection, _skinMode == 1);
        }

        // ===== РЕНДЕРИНГ 2D UI (GUI) =====
        // Отключаем DepthTest для GUI
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // 7. Рендерим инвентарь (2D часть)
        _inventoryRenderer?.Render(Size.X, Size.Y);

        // 8. Рендерим текстовую информацию
        if (_textRenderer != null) {
            var selected = _inventory.GetSelectedItem();
            if (!selected.IsEmpty) {
                string handText = $"[{BlockInfo.GetName(selected.Type)}] x{selected.Count}";
                _textRenderer.RenderText(handText, 20, Size.Y - 80, 2.0f,
                    new Vector3(1f, 1f, 1f), Size.X, Size.Y);
            }

            if (_targetBlock != null && _world != null) {
                var blockType = _world.GetBlock(_targetBlock.Value);
                string targetInfo = $"Looking at: {BlockInfo.GetName(blockType)}";
                _textRenderer.RenderText(targetInfo, 20, Size.Y - 110, 1.5f,
                    new Vector3(0.8f, 0.8f, 0.8f), Size.X, Size.Y);
            }

            if (_showDebugInfo) {
                DrawDebugInfo();
            }
        }

        // 9. Рендерим прицел (всегда поверх всего)
        _crosshair?.Render(Size.X, Size.Y, new Vector4(1f, 1f, 1f, 0.9f));

        // Восстанавливаем состояния OpenGL
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        base.OnUpdateFrame(args);

        if (!_isInitialized || _camera == null || _world == null || _playerController == null) return;

        try {
            float deltaTime = Math.Min((float)args.Time, 0.05f);
            _accumulator += deltaTime;
            _placeCooldown -= deltaTime;

            _timeManager?.Update(deltaTime);
            _world?.UpdateClouds(deltaTime, _timeManager?.TimeOfDay ?? 0);

            var keyboard = KeyboardState;
            var mouse = MouseState;

            // Обновление позиции камеры и игрока
            while (_accumulator >= PHYSICS_TIMESTEP) {
                _playerController.Update(PHYSICS_TIMESTEP, keyboard, mouse);
                _accumulator -= PHYSICS_TIMESTEP;
            }

            // Обновление мира
            _world.Update(_camera.Position);

            // Raycast для определения блока под прицелом
            Vector3 eyePos = _camera.EyePosition;
            Vector3 direction = _camera.Front;
            _targetBlock = Raycaster.GetTargetBlock(eyePos, direction, _world);

            // Обработка разрушения блока
            bool leftMouseDown = mouse.IsButtonDown(MouseButton.Left);
            if (leftMouseDown && _targetBlock != null) {
                if (_blockBeingBroken == null || _blockBeingBroken != _targetBlock) {
                    _blockBeingBroken = _targetBlock;
                    _blockBreakProgress = 0f;
                    _isBreaking = true;
                    _blockBreakAnimation?.Start(_targetBlock.Value);
                }

                _blockBreakProgress += deltaTime * 1.5f;
                _blockBreakAnimation?.Update(_blockBreakProgress);

                if (_blockBreakProgress >= 1f) {
                    BreakBlock(_targetBlock.Value);
                    _blockBeingBroken = null;
                    _blockBreakProgress = 0f;
                    _isBreaking = false;
                    _blockBreakAnimation?.Stop();
                }
            } else {
                if (_isBreaking) {
                    _isBreaking = false;
                    _blockBreakProgress = 0f;
                    _blockBeingBroken = null;
                    _blockBreakAnimation?.Stop();
                }
            }

            // Обработка установки блока
            if (mouse.IsButtonDown(MouseButton.Right) && _targetBlock != null && _placeCooldown <= 0) {
                if (PlaceBlock(_targetBlock.Value)) {
                    _placeCooldown = 0.15f;
                }
            }

            // Горячие клавиши для выбора слота
            for (int i = 0; i < _hotbarKeys.Length; i++) {
                if (keyboard.IsKeyPressed(_hotbarKeys[i])) {
                    _inventory.SelectedSlot = i;
                }
            }

            // Скролл для выбора слота
            float scroll = mouse.ScrollDelta.Y;
            if (scroll != 0) {
                int newSlot = _inventory.SelectedSlot - (scroll > 0 ? 1 : -1);
                if (newSlot < 0) newSlot = 8;
                if (newSlot > 8) newSlot = 0;
                _inventory.SelectedSlot = newSlot;
            }

            // Отладочные клавиши
            if (keyboard.IsKeyPressed(Keys.F3)) {
                _showDebugInfo = !_showDebugInfo;
            }
            if (keyboard.IsKeyPressed(Keys.T)) {
                _timeManager?.ToggleTimeCycle();
            }
            if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Up)) {
                _timeManager?.AddTime(0.005f);
            }
            if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Down)) {
                _timeManager?.AddTime(-0.005f);
            }
            if (keyboard.IsKeyPressed(Keys.R)) {
                Console.WriteLine("Regenerating world...");
                _world.Regenerate();
                _world.GenerateChunks(4);
            }
            if (keyboard.IsKeyPressed(Keys.Z)) {
                _wireframeMode = !_wireframeMode;
                GL.PolygonMode(TriangleFace.FrontAndBack,
                    _wireframeMode ? PolygonMode.Line : PolygonMode.Fill);
                Console.WriteLine($"Wireframe: {(_wireframeMode ? "ON" : "OFF")}");
            }
            if (keyboard.IsKeyPressed(Keys.F11)) {
                WindowState = WindowState == WindowState.Fullscreen ?
                    WindowState.Normal : WindowState.Fullscreen;
            }
            if (keyboard.IsKeyPressed(Keys.Escape)) {
                Close();
            }
            if (keyboard.IsKeyPressed(Keys.F2)) {
                int current = _world.GetViewDistance();
                int next = current == 6 ? 4 : (current == 4 ? 2 : 6);
                _world.SetViewDistance(next);
                Console.WriteLine($"View distance: {next}");
            }
            if (keyboard.IsKeyPressed(Keys.F4)) {
                float current = _timeManager?.TimeSpeed ?? 0.001f;
                if (current == 0.001f) {
                    _timeManager?.SetTimeSpeed(0.005f);
                    Console.WriteLine("Time speed: FAST");
                } else if (current == 0.005f) {
                    _timeManager?.SetTimeSpeed(0.02f);
                    Console.WriteLine("Time speed: VERY FAST");
                } else {
                    _timeManager?.SetTimeSpeed(0.001f);
                    Console.WriteLine("Time speed: NORMAL");
                }
            }
            if (keyboard.IsKeyPressed(Keys.B)) {
                _camera.ToggleHeadBob();
                Console.WriteLine($"Head bob: {(_camera.HeadBobEnabled ? "ON" : "OFF")}");
            }
            if (keyboard.IsKeyPressed(Keys.F5)) {
                _skinMode = (_skinMode + 1) % 3;
                Console.WriteLine($"Skin mode: {_skinMode} (0=off, 1=back, 2=front)");
            }

        } catch (Exception ex) {
            Console.WriteLine($"!!! UPDATE ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private void BreakBlock(Vector3 blockPos) {
        if (_world == null) return;

        var blockType = _world.GetBlock(blockPos);
        if (blockType == BlockType.Air || blockType == BlockType.Bedrock) return;

        _world.SetBlock(blockPos, BlockType.Air);
        _inventory.AddItem(blockType, 1);
    }

    private bool PlaceBlock(Vector3 blockPos) {
        if (_world == null || _camera == null) return false;

        var selectedBlock = _inventory.GetSelectedBlockType();
        if (selectedBlock == BlockType.Air) return false;

        Vector3? faceNormal = Raycaster.GetBlockFace(
            _camera.EyePosition,
            _camera.Front,
            _world,
            blockPos
        );

        if (faceNormal == null) return false;

        Vector3 placePos = blockPos + faceNormal.Value;

        if (placePos.Y < 0 || placePos.Y >= 256) return false;
        if (_world.GetBlock(placePos) != BlockType.Air) return false;

        if (IsBlockCollidingWithPlayer(placePos, _camera.Position)) {
            return false;
        }

        _world.SetBlock(placePos, selectedBlock);
        _inventory.RemoveSelectedItem(1);
        return true;
    }

    private bool IsBlockCollidingWithPlayer(Vector3 blockPos, Vector3 playerPos) {
        const float playerWidth = 0.6f;
        const float playerHeight = 1.8f;

        float halfWidth = playerWidth / 2f;

        Vector3 blockMin = blockPos;
        Vector3 blockMax = blockPos + Vector3.One;

        Vector3 playerMin = new Vector3(
            playerPos.X - halfWidth,
            playerPos.Y,
            playerPos.Z - halfWidth
        );
        Vector3 playerMax = new Vector3(
            playerPos.X + halfWidth,
            playerPos.Y + playerHeight,
            playerPos.Z + halfWidth
        );

        return (blockMin.X < playerMax.X && blockMax.X > playerMin.X &&
                blockMin.Y < playerMax.Y && blockMax.Y > playerMin.Y &&
                blockMin.Z < playerMax.Z && blockMax.Z > playerMin.Z);
    }

    private void DrawDebugInfo() {
        if (_textRenderer == null || _camera == null || _world == null || _playerController == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("Minecraft Engine");
        sb.AppendLine($"FPS: {_fps:F0}");
        sb.AppendLine($"Chunks: {_world.GetChunkCount()}");
        sb.AppendLine($"Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
        sb.AppendLine();
        sb.AppendLine($"XYZ: {_camera.Position.X:F1} / {_camera.Position.Y:F1} / {_camera.Position.Z:F1}");
        sb.AppendLine($"Yaw: {_camera.Yaw:F1} Pitch: {_camera.Pitch:F1}");
        sb.AppendLine($"Velocity: {_playerController.VerticalVelocity:F1}");
        sb.AppendLine($"OnGround: {_playerController.IsOnGround}");
        sb.AppendLine($"Flying: {_playerController.IsFlying}");

        var selected = _inventory.GetSelectedItem();
        sb.AppendLine($"Slot: {_inventory.SelectedSlot + 1}/9");
        sb.AppendLine($"Block: {BlockInfo.GetName(selected.Type)} x{selected.Count}");

        if (_targetBlock != null) {
            var blockType = _world.GetBlock(_targetBlock.Value);
            sb.AppendLine($"Target: {_targetBlock.Value.X:F0} / {_targetBlock.Value.Y:F0} / {_targetBlock.Value.Z:F0} ({BlockInfo.GetName(blockType)})");
        }

        if (_blockBreakAnimation != null && _blockBreakAnimation.IsActive) {
            sb.AppendLine($"Breaking: {_blockBreakAnimation.Progress:P0}");
        }

        sb.AppendLine();
        sb.AppendLine("[F3] Debug | [F] Flight | [1-9] Hotbar");
        sb.AppendLine("[LMB] Break | [RMB] Place | [Scroll] Switch");

        var lines = sb.ToString().Split('\n');
        float x = 10f;
        float y = 50f;
        float scale = 2.0f;
        var color = new Vector3(1f, 1f, 1f);
        var shadowColor = new Vector3(0f, 0f, 0f);

        foreach (var line in lines) {
            if (!string.IsNullOrEmpty(line)) {
                _textRenderer.RenderText(line, x + 1, y + 1, scale, shadowColor, Size.X, Size.Y);
                _textRenderer.RenderText(line, x, y, scale, color, Size.X, Size.Y);
            }
            y += 12 * scale;
        }
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload() {
        base.OnUnload();
        if (_timeManager != null) {
            _timeManager.OnTimeChanged -= OnTimeChanged;
        }
        _inventoryRenderer?.Dispose();
        _blockOutline?.Dispose();
        _crosshair?.Dispose();
        _blockBreakAnimation?.Dispose();
        _shader?.Dispose();
        _atlas?.Dispose();
        _textRenderer?.Dispose();
        _world?.Dispose();
    }
}