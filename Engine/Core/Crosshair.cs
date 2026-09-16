using MinecraftEngine.Engine.Graphics;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public class Crosshair {
    private TextRenderer _textRenderer;
    private bool _isInitialized = false;

    public Crosshair(TextRenderer textRenderer) {
        _textRenderer = textRenderer;
        _isInitialized = true;
    }

    public void Render(int screenWidth, int screenHeight, Vector4 color) {
        if (!_isInitialized || _textRenderer == null) return;

        Vector3 color3 = new Vector3(1.0f, 1.0f, 1.0f);

        // Уменьшаем масштаб для тонкости
        float scale = 2.5f;
        float cx = screenWidth / 2f;
        float cy = screenHeight / 2f;

        float charW = 5f * scale;
        float charH = 7f * scale;

        float x = cx - charW / 2f;
        float y = cy - charH / 2f;

        // Только один раз, без дублирования
        _textRenderer.RenderText("+", x, y, scale, color3, screenWidth, screenHeight);
    }

    public void Dispose() {
        _isInitialized = false;
    }
}