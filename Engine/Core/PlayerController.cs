using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MinecraftEngine.Engine.Core;

public class PlayerController {
    private Camera _camera;
    private World.World _world;

    public bool IsFlying { get; set; } = false;
    public bool IsOnGround { get; private set; } = false;
    public float VerticalVelocity { get; private set; } = 0f;

    private bool _wasFlyingLastFrame = false;
    private float _timeSinceFlightEnabled = 0f;
    private bool _justEnabledFlight = false;
    private bool _flightKeyPressed = false;

    public PlayerController(Camera camera, World.World world) {
        _camera = camera;
        _world = world;
    }

    public void Update(float deltaTime, KeyboardState keyboard, MouseState mouse) {
        _camera.Yaw += mouse.Delta.X * PlayerSettings.MouseSensitivity;
        _camera.Pitch -= mouse.Delta.Y * PlayerSettings.MouseSensitivity;
        _camera.Pitch = Math.Clamp(_camera.Pitch, PlayerSettings.MinPitch, PlayerSettings.MaxPitch);

        IsOnGround = Collision.IsOnGround(_camera.Position, _world);

        if (_justEnabledFlight) {
            _timeSinceFlightEnabled += deltaTime;
            if (_timeSinceFlightEnabled > 0.5f) {
                _justEnabledFlight = false;
            }
        }

        if (IsFlying && IsOnGround && !_justEnabledFlight && _wasFlyingLastFrame) {
            IsFlying = false;
            VerticalVelocity = 0f;
        }
        _wasFlyingLastFrame = IsFlying;

        bool fPressed = keyboard.IsKeyPressed(Keys.F);
        if (fPressed && !_flightKeyPressed) {
            _flightKeyPressed = true;
            ToggleFlight();
        }
        if (!fPressed) {
            _flightKeyPressed = false;
        }

        float speed = PlayerSettings.WalkSpeed;
        bool isSprinting = keyboard.IsKeyDown(Keys.LeftControl);
        bool isSneaking = keyboard.IsKeyDown(Keys.LeftShift);

        if (isSprinting) {
            speed = IsFlying ? PlayerSettings.FlySprintSpeed : PlayerSettings.SprintSpeed;
        }
        if (isSneaking && !IsFlying) {
            speed = PlayerSettings.SneakSpeed;
        }
        if (IsFlying) {
            speed = isSprinting ? PlayerSettings.FlySprintSpeed : PlayerSettings.FlySpeed;
        }

        var direction = new Vector3();
        bool isMoving = false;

        if (keyboard.IsKeyDown(Keys.W)) { direction.Z += 1; isMoving = true; }
        if (keyboard.IsKeyDown(Keys.S)) { direction.Z -= 1; isMoving = true; }
        if (keyboard.IsKeyDown(Keys.A)) { direction.X -= 1; isMoving = true; }
        if (keyboard.IsKeyDown(Keys.D)) { direction.X += 1; isMoving = true; }

        _camera.UpdateHeadBob(deltaTime, isMoving && IsOnGround && !IsFlying && !isSneaking, isSprinting);

        if (!IsFlying) {
            VerticalVelocity += PlayerSettings.Gravity * deltaTime;
            if (VerticalVelocity < PlayerSettings.TerminalVelocity) {
                VerticalVelocity = PlayerSettings.TerminalVelocity;
            }

            if (keyboard.IsKeyPressed(Keys.Space) && IsOnGround) {
                VerticalVelocity = PlayerSettings.JumpSpeed;
                IsOnGround = false;
            }

            direction.Y += VerticalVelocity * deltaTime;
        } else {
            if (keyboard.IsKeyDown(Keys.Space)) direction.Y += 1;
            if (keyboard.IsKeyDown(Keys.LeftShift)) direction.Y -= 1;
            VerticalVelocity = 0f;
        }

        Vector3 moveDir;
        if (direction.Length > 0) {
            direction.Normalize();
            var forward = _camera.Front;
            var right = Vector3.Cross(forward, _camera.Up);
            right.Y = 0;
            right.Normalize();
            forward.Y = 0;
            forward.Normalize();

            moveDir = direction.X * right + direction.Z * forward + direction.Y * Vector3.UnitY;
            if (moveDir.Length > 0) moveDir.Normalize();
        } else {
            moveDir = Vector3.Zero;
        }

        if (moveDir.Length > 0 || !IsFlying) {
            var movement = moveDir * speed * deltaTime;
            if (!IsFlying) {
                movement.Y = VerticalVelocity * deltaTime;
            }

            if (!IsFlying && isSneaking && IsOnGround) {
                Vector3 testPos = _camera.Position + new Vector3(movement.X, 0, movement.Z);
                bool willBeOnGround = Collision.IsOnGround(testPos, _world);

                if (!willBeOnGround) {
                    movement.X = 0;
                    movement.Z = 0;
                    if (!IsFlying) {
                        movement.Y = VerticalVelocity * deltaTime;
                    }
                }
            }

            var newPos = Collision.Move(_camera.Position, movement, _world);

            if (newPos.Y == _camera.Position.Y && movement.Y <= 0) {
                VerticalVelocity = 0f;
                IsOnGround = true;
            }

            _camera.Position = newPos;
        }

        if (IsOnGround && VerticalVelocity < 0) {
            VerticalVelocity = 0f;
        }
    }

    public void ToggleFlight() {
        IsFlying = !IsFlying;
        VerticalVelocity = 0f;

        if (IsFlying) {
            _justEnabledFlight = true;
            _timeSinceFlightEnabled = 0f;
            IsOnGround = false;
        }
    }
}