using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public class Camera {
    public const float EyeHeight = 1.6f;

    public Vector3 Position { get; set; }
    public Vector3 Front { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float Fov { get; set; } = 75f;

    private float _bobTimer = 0f;
    public bool HeadBobEnabled { get; set; } = true;

    public Camera(Vector3 position, Vector3 up, float yaw) {
        Position = position;
        Up = up;
        Yaw = yaw;
        UpdateVectors();
    }

    public void UpdateHeadBob(float deltaTime, bool isMoving, bool isSprinting) {
        if (!HeadBobEnabled) return;

        if (isMoving) {
            _bobTimer += deltaTime * (isSprinting ? 14f : 9f);
        } else {
            _bobTimer *= 0.9f;
            if (Math.Abs(_bobTimer) < 0.01f) _bobTimer = 0f;
        }
    }

    private Vector3 GetHeadBobOffset() {
        if (_bobTimer == 0f || !HeadBobEnabled) return Vector3.Zero;
        float bob = MathF.Sin(_bobTimer) * 0.06f;
        float bobX = MathF.Sin(_bobTimer * 0.6f) * 0.03f;
        return new Vector3(bobX, bob, 0f);
    }

    public void UpdateVectors() {
        var front = new Vector3(
            MathF.Cos(Pitch) * MathF.Cos(Yaw),
            MathF.Sin(Pitch),
            MathF.Cos(Pitch) * MathF.Sin(Yaw)
        );
        Front = Vector3.Normalize(front);
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    public Vector3 EyePosition => Position + new Vector3(0, EyeHeight, 0) + GetHeadBobOffset();

    public void ToggleHeadBob() {
        HeadBobEnabled = !HeadBobEnabled;
        if (!HeadBobEnabled) {
            _bobTimer = 0f;
        }
    }

    public Matrix4 GetViewMatrix() {
        UpdateVectors();
        Vector3 eyePos = EyePosition;
        return Matrix4.LookAt(eyePos, eyePos + Front, Up);
    }

    public Matrix4 GetProjectionMatrix(float width, float height) {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(Fov),
            width / height,
            0.1f,
            1000f
        );
    }

    public Vector3 GetForward() => Front;
    public Vector3 GetRight() => Right;
}