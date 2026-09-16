using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public static class Input {
    private static KeyboardState _keyboardState;
    private static MouseState _mouseState;
    private static KeyboardState _previousKeyboardState;
    private static MouseState _previousMouseState;

    public static void Update(KeyboardState keyboard, MouseState mouse) {
        _previousKeyboardState = _keyboardState;
        _previousMouseState = _mouseState;
        _keyboardState = keyboard;
        _mouseState = mouse;
    }

    public static bool IsKeyDown(Keys key) => _keyboardState.IsKeyDown(key);
    public static bool IsKeyUp(Keys key) => !_keyboardState.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key) => _keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    public static bool IsKeyReleased(Keys key) => !_keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);

    public static Vector2 GetMousePosition() => new(_mouseState.X, _mouseState.Y);
    public static Vector2 GetMouseDelta() => new(_mouseState.Delta.X, _mouseState.Delta.Y);
    public static bool IsMouseButtonDown(MouseButton button) => _mouseState.IsButtonDown(button);
    public static bool IsMouseButtonPressed(MouseButton button) => _mouseState.IsButtonDown(button) && !_previousMouseState.IsButtonDown(button);
    public static float GetScrollDelta() => _mouseState.ScrollDelta.Y;

    public static bool LeftMouseDown => IsMouseButtonDown(MouseButton.Left);
    public static bool RightMouseDown => IsMouseButtonDown(MouseButton.Right);
    public static bool MiddleMouseDown => IsMouseButtonDown(MouseButton.Middle);
    public static bool LeftMousePressed => IsMouseButtonPressed(MouseButton.Left);
    public static bool RightMousePressed => IsMouseButtonPressed(MouseButton.Right);

    public static bool IsCursorVisible { get; set; } = true;
    public static bool IsCursorLocked { get; set; } = false;
}