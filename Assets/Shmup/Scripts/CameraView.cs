using UnityEngine;

namespace Shmup
{
    // Lo que se ve en pantalla en este momento, medido en tiles.
    public static class CameraView
    {
        public static float HalfHeight => Camera.main.orthographicSize;
        public static float HalfWidth => HalfHeight * Camera.main.aspect;
        public static float RightEdge => Camera.main.transform.position.x + HalfWidth;

        // ¿Está el punto en pantalla? El margen amplía la zona por fuera de los bordes.
        public static bool Contains(Vector2 point, float margin)
        {
            Vector2 offset = point - (Vector2)Camera.main.transform.position;
            return Mathf.Abs(offset.x) <= HalfWidth + margin && Mathf.Abs(offset.y) <= HalfHeight + margin;
        }
    }
}
