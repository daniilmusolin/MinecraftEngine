using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public static class Collision {
    public const float PlayerWidth = 0.6f;
    public const float PlayerHeight = 1.8f;
    private const float EPSILON = 0.001f;

    public static bool IsBlockAt(float x, float y, float z, World.World world) {
        int bx = (int)Math.Floor(x);
        int by = (int)Math.Floor(y);
        int bz = (int)Math.Floor(z);
        return world.IsBlockAt(new Vector3(bx, by, bz));
    }

    public static bool IsColliding(float x, float y, float z, World.World world) {
        float r = PlayerWidth / 2f - EPSILON;
        float h = PlayerHeight - EPSILON;

        // Проверяем все углы bounding box
        // Низ
        for (int ix = -1; ix <= 1; ix += 2) {
            for (int iz = -1; iz <= 1; iz += 2) {
                if (IsBlockAt(x + ix * r, y, z + iz * r, world))
                    return true;
            }
        }

        // Верх
        for (int ix = -1; ix <= 1; ix += 2) {
            for (int iz = -1; iz <= 1; iz += 2) {
                if (IsBlockAt(x + ix * r, y + h, z + iz * r, world))
                    return true;
            }
        }

        return false;
    }

    public static bool IsOnGround(Vector3 pos, World.World world) {
        float y = pos.Y - 0.01f;
        float r = PlayerWidth / 2f - 0.01f;

        // Проверяем несколько точек под ногами
        for (float dx = -r; dx <= r + 0.001f; dx += r) {
            for (float dz = -r; dz <= r + 0.001f; dz += r) {
                if (IsBlockAt(pos.X + dx, y, pos.Z + dz, world)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static Vector3 Move(Vector3 pos, Vector3 move, World.World world) {
        float x = pos.X;
        float y = pos.Y;
        float z = pos.Z;

        // Движение по X
        float newX = x + move.X;
        if (!IsColliding(newX, y, z, world)) {
            x = newX;
        }

        // Движение по Y
        float newY = y + move.Y;
        if (!IsColliding(x, newY, z, world)) {
            y = newY;
        }

        // Движение по Z
        float newZ = z + move.Z;
        if (!IsColliding(x, y, newZ, world)) {
            z = newZ;
        }

        return new Vector3(x, y, z);
    }
}