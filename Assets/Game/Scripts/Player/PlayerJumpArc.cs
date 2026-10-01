using System.Collections.Generic;
using UnityEngine;

// Simula el salto con los mismos pasos que PlayerMovement para dibujar el arco (simple y doble) en la escena.
public static class PlayerJumpArc
{
    private const int MaxSteps = 1000;

    private static readonly Color SingleJumpColor = new Color(1f, 0.85f, 0.2f, 0.9f);
    private static readonly Color AirJumpColor = new Color(0.3f, 0.9f, 1f, 0.9f);
    private static readonly List<Vector2> Points = new List<Vector2>();

    public struct Result
    {
        public float Height;
        public float Distance;
    }

    // Corriendo a velocidad máxima y con el salto mantenido. Los saltos en el aire se lanzan en cada punto más alto.
    public static Result Simulate(PlayerMovementConfigSO config, Vector2 start, int direction, int airJumps, List<Vector2> points)
    {
        float dt = Time.fixedDeltaTime;
        var position = start;
        var velocity = new Vector2(config.MaxRunSpeed * direction, config.JumpVelocity);
        float maxHeight = 0f;
        int airJumpsLeft = airJumps;

        points?.Clear();
        points?.Add(position);

        for (int i = 0; i < MaxSteps; i++)
        {
            Vector2 previous = position;
            position += velocity * dt;
            maxHeight = Mathf.Max(maxHeight, position.y - start.y);

            if (velocity.y < 0f && position.y <= start.y)
            {
                float t = Mathf.InverseLerp(previous.y, position.y, start.y);
                position = Vector2.Lerp(previous, position, t);
                points?.Add(position);
                break;
            }
            points?.Add(position);

            velocity.x = PlayerMovement.StepHorizontal(config, velocity.x, direction, false, dt);
            velocity.y = PlayerMovement.StepVertical(config, velocity.y, false, dt);
            if (velocity.y <= 0f && airJumpsLeft > 0)
            {
                velocity.y = config.AirJumpVelocity;
                airJumpsLeft--;
            }
        }

        return new Result { Height = maxHeight, Distance = Mathf.Abs(position.x - start.x) };
    }

    // El hueco salvable suma el ancho del collider: despega con el borde trasero y aterriza con el delantero.
    public static void DrawGizmo(PlayerMovementConfigSO config, Vector2 feet, int direction, float bodyWidth)
    {
        DrawArc(config, feet, direction, bodyWidth, 0, SingleJumpColor, "Salto");
        if (config.AirJumps > 0)
        {
            string label = config.AirJumps == 1 ? "Doble salto" : $"{config.AirJumps + 1} saltos";
            DrawArc(config, feet, direction, bodyWidth, config.AirJumps, AirJumpColor, label);
        }
    }

    private static void DrawArc(PlayerMovementConfigSO config, Vector2 feet, int direction, float bodyWidth, int airJumps, Color color, string label)
    {
        Result result = Simulate(config, feet, direction, airJumps, Points);

        Gizmos.color = color;
        for (int i = 1; i < Points.Count; i++)
        {
            Gizmos.DrawLine(Points[i - 1], Points[i]);
        }

        // Altura máxima y punto de aterrizaje a la misma altura que el salto.
        Vector2 landing = Points[Points.Count - 1];
        float apexY = feet.y + result.Height;
        Gizmos.DrawLine(new Vector2(feet.x - 0.5f * direction, apexY), new Vector2(landing.x, apexY));
        Gizmos.DrawLine(landing + Vector2.down * 0.25f, landing + Vector2.up * 0.25f);

#if UNITY_EDITOR
        var style = new GUIStyle(UnityEditor.EditorStyles.miniBoldLabel) { normal = { textColor = color } };
        UnityEditor.Handles.Label(new Vector3(landing.x, apexY + 0.6f),
            $"{label}: alto {result.Height:0.0} · hueco máx {result.Distance + bodyWidth:0.0} tiles", style);
#endif
    }
}
