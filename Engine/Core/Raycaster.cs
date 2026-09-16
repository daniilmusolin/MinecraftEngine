using OpenTK.Mathematics;

namespace MinecraftEngine.Engine.Core;

public static class Raycaster {
    private const float MAX_DISTANCE = 8f;
    private const float STEP = 0.02f;

    public static Vector3? GetTargetBlock(Vector3 origin, Vector3 direction, World.World world) {
        for (float dist = 0; dist < MAX_DISTANCE; dist += STEP) {
            Vector3 pos = origin + direction * dist;
            int x = (int)Math.Floor(pos.X);
            int y = (int)Math.Floor(pos.Y);
            int z = (int)Math.Floor(pos.Z);

            if (world.IsBlockAt(new Vector3(x, y, z))) {
                return new Vector3(x, y, z);
            }
        }
        return null;
    }

    public static Vector3? GetBlockFace(Vector3 origin, Vector3 direction, World.World world, Vector3 blockPos) {
        float bestDist = float.MaxValue;
        Vector3? bestFace = null;

        // Проверяем все 6 граней блока
        Vector3[] faces = {
            new Vector3(1, 0, 0),   // +X
            new Vector3(-1, 0, 0),  // -X
            new Vector3(0, 1, 0),   // +Y
            new Vector3(0, -1, 0),  // -Y
            new Vector3(0, 0, 1),   // +Z
            new Vector3(0, 0, -1)   // -Z
        };

        foreach (var face in faces) {
            // Центр грани блока
            Vector3 faceCenter = blockPos + new Vector3(0.5f, 0.5f, 0.5f) + face * 0.5f;

            // Расстояние от луча до центра грани
            Vector3 toFace = faceCenter - origin;
            float projection = Vector3.Dot(toFace, direction);

            if (projection > 0 && projection < MAX_DISTANCE) {
                Vector3 closestPoint = origin + direction * projection;
                float distance = Vector3.Distance(closestPoint, faceCenter);

                // Проверяем, что точка попадания находится в пределах грани
                if (distance < 0.1f) {
                    return face;
                }
            }
        }

        // Если точное попадание не найдено, определяем по ближайшей грани
        Vector3 localPoint = origin + direction * GetHitDistance(origin, direction, blockPos);
        localPoint -= blockPos;

        float dx = Math.Abs(localPoint.X - 0.5f);
        float dy = Math.Abs(localPoint.Y - 0.5f);
        float dz = Math.Abs(localPoint.Z - 0.5f);

        if (dx >= dy && dx >= dz) {
            return new Vector3(localPoint.X > 0.5f ? 1 : -1, 0, 0);
        } else if (dy >= dz) {
            return new Vector3(0, localPoint.Y > 0.5f ? 1 : -1, 0);
        } else {
            return new Vector3(0, 0, localPoint.Z > 0.5f ? 1 : -1);
        }
    }

    private static float GetHitDistance(Vector3 origin, Vector3 direction, Vector3 blockPos) {
        Vector3 center = blockPos + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 toCenter = center - origin;
        float projection = Vector3.Dot(toCenter, direction);

        for (float dist = Math.Max(0, projection - 0.5f); dist <= projection + 0.5f; dist += 0.01f) {
            Vector3 point = origin + direction * dist;
            int x = (int)Math.Floor(point.X);
            int y = (int)Math.Floor(point.Y);
            int z = (int)Math.Floor(point.Z);

            if (x == (int)blockPos.X && y == (int)blockPos.Y && z == (int)blockPos.Z) {
                return dist;
            }
        }

        return projection;
    }
}