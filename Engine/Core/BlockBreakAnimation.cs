using OpenTK.Mathematics;
using MinecraftEngine.Engine.Graphics;

namespace MinecraftEngine.Engine.Core;

public class BlockBreakAnimation {
    private TextRenderer _textRenderer;
    private Vector3? _blockPos;
    private float _progress;
    private bool _isActive;

    public BlockBreakAnimation(TextRenderer textRenderer) {
        _textRenderer = textRenderer;
        _isActive = false;
        _progress = 0f;
    }

    public void Start(Vector3 blockPos) {
        _blockPos = blockPos;
        _progress = 0f;
        _isActive = true;
    }

    public void Update(float progress) {
        if (!_isActive) return;
        _progress = Math.Clamp(progress, 0f, 1f);
        if (_progress >= 1f) {
            _isActive = false;
        }
    }

    public void Stop() {
        _isActive = false;
        _progress = 0f;
        _blockPos = null;
    }

    public bool IsActive => _isActive;
    public float Progress => _progress;

    public void Render(int screenWidth, int screenHeight, Matrix4 view, Matrix4 projection, Camera camera) {
        if (!_isActive || _blockPos == null || _textRenderer == null || camera == null) return;

        // Позиция над блоком
        Vector3 worldPos = _blockPos.Value + new Vector3(0.5f, 1.2f, 0.5f);

        // Проекция 3D в 2D
        Vector4 clipPos = new Vector4(worldPos, 1.0f) * view * projection;

        if (clipPos.W <= 0) return;

        float x = (clipPos.X / clipPos.W + 1) / 2 * screenWidth;
        float y = (1 - clipPos.Y / clipPos.W) / 2 * screenHeight;

        if (x < -50 || x > screenWidth + 50 || y < -50 || y > screenHeight + 50) return;

        float dist = Vector3.Distance(camera.Position, _blockPos.Value);
        float scale = Math.Clamp(2.5f - dist * 0.06f, 0.6f, 2.5f);

        // Прогресс-бар
        int barWidth = 20;
        int filled = (int)(_progress * barWidth);
        string bar = new string('█', filled) + new string('░', barWidth - filled);

        float textWidth = bar.Length * 5f * scale;
        float textHeight = 7f * scale;

        // Фон
        float bgX = x - textWidth / 2 - 6f;
        float bgY = y - textHeight / 2 - 4f;
        float bgW = textWidth + 12f;
        float bgH = textHeight + 8f;

        for (int row = 0; row < bgH / 7f + 1; row++) {
            for (int col = 0; col < bgW / 5f + 1; col++) {
                float px = bgX + col * 5f;
                float py = bgY + row * 7f;
                _textRenderer.RenderText(" ", px, py, 1f,
                    new Vector3(0f, 0f, 0f), screenWidth, screenHeight);
            }
        }

        // Прогресс-бар
        _textRenderer.RenderText(bar, x - textWidth / 2 + 1, y - textHeight / 2 + 1, scale,
            new Vector3(0f, 0f, 0f), screenWidth, screenHeight);
        _textRenderer.RenderText(bar, x - textWidth / 2, y - textHeight / 2, scale,
            new Vector3(1f, 1f, 1f), screenWidth, screenHeight);

        // Процент
        string percentText = $"{Math.Round(_progress * 100)}%";
        float pScale = scale * 0.7f;
        float pX = x - percentText.Length * 5f * pScale / 2f;
        float pY = y - textHeight / 2 - 7f * pScale - 4f;

        _textRenderer.RenderText(percentText, pX + 1, pY + 1, pScale,
            new Vector3(0f, 0f, 0f), screenWidth, screenHeight);
        _textRenderer.RenderText(percentText, pX, pY, pScale,
            new Vector3(1f, 1f, 0f), screenWidth, screenHeight);
    }

    public void Dispose() {
        _textRenderer = null;
        _blockPos = null;
        _isActive = false;
    }
}